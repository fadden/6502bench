/*
 * Copyright 2019 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/*
Some assemblers support "local labels", with varying definitions of scope and features.
Generally speaking, local labels only need to be unique within a certain limited scope, and
they aren't included in end-of-assembly symbol lists.

One popular form defines its scope as being between two global labels.  So this is allowed:

  glob1    lda #$00
  :local   sta $00
  glob2    lda #$01
  :local   sta $01

but this would cause an error:

  glob1    lda #$00
  :local   sta $00
  glob2    lda #$01
           bne :local

because the local symbol table is cleared when a global symbol is encountered.

Another common form allows backward references to labels that don't go out of scope until
they're re-used.  This is useful for short loops.  (We use this for variables.)

As a further limitation, assemblers seem to want the first label encountered in a program
to be global.

The Symbol.SymbolType enum allows a label to be defined as "local or global".  We can output
these with the local-symbol syntax, potentially rewriting them to have non-unique names like
"loop", but we can't promote (demote?) a label to local unless there are no references to it
that cross a global label.

The cross-reference table we generate as part of the analysis process provides a full list of
label references, so we just need to iterate through the label list until we can't find
anything else that needs to be made global.

Because the definition of "local label" is somewhat assembler-specific, it's best to defer
this analysis to code generation time, when the specific characteristics of the target
assembler can be taken into account.

References to an offset can be numeric or symbolic.  A purely numeric reference like "LDA $2000"
will always map to the offset associated with address $2000, but a symbolic reference might be
offset.  For example, the LDA instruction could reference a label at $2008 as "LDA FOO-8".
The assembler cares about the symbolic references, not the actual offsets or addresses.  For
this reason we can ignore references to an address with a label if those references don't
actually use the label.  (One consequence of this is that formatting an operand as hex
eliminates it from the set of things for us to consider.  Also, ORG directives have no effect
on the localizer.)

Labels that are marked as global, but to which there are no references, could in theory be
elided.  To do this we would have to omit them from the generated code, which would be
annoying and weird if (say) the user added them to label an external entry point.


The eventual output of our efforts is a map from the original symbol name to the local symbol
name.  This must be applied to both labels and operands.
*/

namespace SourceGen.AsmGen {
    public class LabelLocalizer {
        // Prefix string to use for labels that start with '_' when generating code for
        // assemblers that assign a special meaning to leading underscores.
        private const string NO_UNDER_PFX = "X";

        /// <summary>
        /// A pairing of an offset with a label string.  (Essentially mAnattribs[n].Symbol
        /// with all the fluff trimmed away.)
        /// </summary>
        /// <remarks>
        /// The label string isn't actually all that useful, since we can pull it back out
        /// of anattrib, but it makes life a little easier during debugging.  These get
        /// put into a List, so simply storing a plain int offset it's much better (in terms
        /// of memory and allocations) because the ints get boxed.
        /// </remarks>
        private class OffsetLabel {
            public int Offset { get; private set; }
            public string Label { get; private set; }

            public OffsetLabel(int offset, string label) {
                Offset = offset;
                Label = label;
            }

            public override string ToString() {
                return "+" + Offset.ToString("x6") + "(" + Label + ")";
            }
        }

        /// <summary>
        /// A pair of offsets.  An operand (instruction or data) at the source offset
        /// references a label at the destination offset.
        /// </summary>
        private class OffsetPair {
            public int SrcOffset { get; private set; }  // offset from which reference is made
            public int DstOffset { get; private set; }  // offset being referred to

            public OffsetPair(int src, int dst) {
                SrcOffset = src;
                DstOffset = dst;
            }

            public override string ToString() {
                return "src=+" + SrcOffset.ToString("x6") + " dst=+" + DstOffset.ToString("x6");
            }
        }

        /// <summary>
        /// Map from label string to local label string.  This will be null until Analyze()
        /// has executed.
        /// </summary>
        public Dictionary<string, string> LabelMap { get; private set; }

        /// <summary>
        /// String to prefix to local labels.  Usually a single character, like ':' or '@'.
        /// </summary>
        public string LocalPrefix { get; set; }

        /// <summary>
        /// Set this if the declaration of a local variable ends the current scope.
        /// </summary>
        public bool QuirkVariablesEndScope { get; set; }

        /// <summary>
        /// Set this if global variables are not allowed to have the same name as an opcode
        /// mnemonic.
        /// </summary>
        public bool QuirkNoOpcodeMnemonics { get; set; }

        /// <summary>
        /// Project reference.
        /// </summary>
        private DisasmProject mProject;

        // Work state.
        private List<OffsetLabel> mGlobalLabels = new List<OffsetLabel>();
        private List<OffsetPair> mOffsetPairs = new List<OffsetPair>();
        private BitArray mGlobalFlags;


        public LabelLocalizer(DisasmProject project) {
            mProject = project;
            mGlobalFlags = new BitArray(mProject.FileDataLength);

            LocalPrefix = "!?";
        }

        /// <summary>
        /// Applies the LabelMap to the label.  If the LabelMap is null, or does not have an
        /// entry for the label, the original label is returned.
        /// </summary>
        /// <param name="label">Label to convert.</param>
        /// <returns>New label, or original label.</returns>
        public string ConvLabel(string label) {
            if (LabelMap != null) {
                if (LabelMap.TryGetValue(label, out string newLabel)) {
                    label = newLabel;
                }
            }
            return label;
        }

        /// <summary>
        /// Analyzes labels to identify which ones may be treated as non-global.
        /// </summary>
        public void Analyze() {
            Debug.Assert(LocalPrefix.Length > 0);

            // Init global flags list.  An entry is set if the associated offset has a global
            // label.  It will be false if the entry has a local label, or no label.
            mGlobalFlags.SetAll(false);

            // Currently we only support the "local labels have scope that ends at a global
            // label" variety.  The basic idea is to start by assuming that everything not
            // explicitly marked global is local, and then identify situations like this:
            //
            //           lda :local
            //   global  eor #$ff
            //   :local  sta $00
            //
            // The reference crosses a global label, so the "target" label must be made global.
            // This can have ripple effects, so we have to iterate.  Note it doesn't matter
            // whether "global" is referenced anywhere.
            //
            // The current algorithm uses a straightforward O(n^2) approach.

            //
            // Step 1: generate source/target pairs and global label list
            //
            GenerateLists();

            //
            // Step 2: walk through the list of global symbols, identifying source/target
            // pairs that cross them.  If a pair matches, the target label is added to the
            // end of the mGlobalLabels list, and removed from the pair list.
            //
            // When we're done, mGlobalFlags[] will identify the offsets with global labels.
            //
            for (int index = 0; index < mGlobalLabels.Count; index++) {
                FindIntersectingPairs(mGlobalLabels[index]);
            }

            // We're done with these.  Take out the trash.
            mGlobalLabels.Clear();
            mOffsetPairs.Clear();

            //
            // Step 3: remap global labels.  There are three reasons we might need to do this:
            //  (1) It has a leading underscore AND LocalPrefix is '_'.
            //  (2) The label matches an opcode mnemonic (case-insensitive) AND NoOpcodeMnemonics
            //      is set.
            //  (3) It's a non-unique local that got promoted to global.
            //
            // In each case we need to modify the label to meet the assembler requirements, and
            // then modify the label until it's unique.
            //
            LabelMap = new Dictionary<string, string>();
            Dictionary<string, string> allGlobalLabels = new Dictionary<string, string>();
            bool remapUnders = (LocalPrefix == "_");

            Dictionary<string, Asm65.OpDef> opNames = null;
            if (QuirkNoOpcodeMnemonics) {
                // Create a searchable list of opcode names using the current CPU definition.
                // (All tested assemblers that failed on opcode names only did so for names
                // that were part of the current definition, e.g. "TSB" was accepted as a label
                // when the CPU was set to 6502.)
                opNames = new Dictionary<string, Asm65.OpDef>();
                Asm65.CpuDef cpuDef = mProject.CpuDef;
                for (int i = 0; i < 256; i++) {
                    Asm65.OpDef op = cpuDef.GetOpDef(i);
                    // There may be multiple entries with the same name (e.g. "NOP").  That's fine.
                    opNames[op.Mnemonic.ToUpperInvariant()] = op;
                }
            }

            for (int i = 0; i < mProject.FileDataLength; i++) {
                if (!mGlobalFlags[i]) {
                    continue;
                }
                Symbol sym = mProject.GetAnattrib(i).Symbol;
                if (sym == null) {
                    // Should only happen when we insert a dummy global label for the
                    // "variables end scope" quirk.
                    continue;
                }

                string newLabel = sym.LabelWithoutTag;
                if (remapUnders && newLabel[0] == '_') {
                    newLabel = NO_UNDER_PFX + newLabel;
                    // This could cause a conflict with an existing label.  It's rare but
                    // possible.
                    if (allGlobalLabels.ContainsKey(newLabel)) {
                        newLabel = MakeUnique(newLabel, allGlobalLabels);
                    }
                }
                if (opNames != null && opNames.ContainsKey(newLabel.ToUpperInvariant())) {
                    // Clashed with mnemonic.  Uniquify it.
                    newLabel = MakeUnique(newLabel, allGlobalLabels);
                }

                // We might have remapped something earlier and it happens to match this label.
                // If so, we can either remap the current label, or remap the previous label
                // a little harder.  The easiest thing to do is remap the current label.
                if (allGlobalLabels.ContainsKey(newLabel)) {
                    newLabel = MakeUnique(newLabel, allGlobalLabels);
                }

                // If we've changed it, add it to the map.
                if (newLabel != sym.Label) {
                    LabelMap[sym.Label] = newLabel;
                }

                allGlobalLabels.Add(newLabel, newLabel);
            }

            //
            // Step 4: remap local labels.  There are two operations here.
            //
            // For each pair of global labels that have locals between them, we need to walk
            // through the locals and confirm that they don't clash with each other.  If they
            // do, we need to uniquify them within the local scope.  (This is only an issue
            // for non-unique locals.)
            //
            // Once a unique name has been found, we add an entry to LabelMap that has the
            // label with the LocalPrefix and without the non-unique tag.
            //
            // We also need to deal with symbols with a leading underscore when
            // LocalPrefix is '_'.
            //
            int startGlobal = -1;
            int numLocals = 0;

            // Allocate a Dictionary here and pass it through so we don't have to allocate
            // a new one each time.
            Dictionary<string, string> scopedLocals = new Dictionary<string, string>();
            for (int i = 0; i < mProject.FileDataLength; i++) {
                if (mGlobalFlags[i]) {
                    if (startGlobal < 0) {
                        // very first one
                        startGlobal = i;
                        continue;
                    } else if (numLocals > 0) {
                        // There were locals following the previous global.  Process them.
                        ProcessLocals(startGlobal, i, scopedLocals);
                        startGlobal = i;
                        numLocals = 0;
                    } else {
                        // Two adjacent globals.
                        startGlobal = i;
                    }
                } else {
                    // Not a global.  Is there a local symbol here?
                    Symbol sym = mProject.GetAnattrib(i).Symbol;
                    if (sym != null) {
                        numLocals++;
                    }
                }
            }
            if (numLocals != 0) {
                // do the last bit
                ProcessLocals(startGlobal, mProject.FileDataLength, scopedLocals);
            }
        }

        /// <summary>
        /// Generates the initial mGlobalFlags and mGlobalLabels lists, as well as the
        /// full cross-reference pair list.
        /// </summary>
        private void GenerateLists() {
            // For every offset that has a label, add an entry to the source/target pair list
            // for every offset that references it.
            //
            // If the label isn't marked as "local or global", add it to the global-label list.
            //
            // The first label encountered is always treated as global.  Note it may not appear
            // at offset zero.

            bool first = true;

            for (int offset = 0; offset < mProject.FileDataLength; offset++) {
                // Find all user labels and auto labels.
                Symbol sym = mProject.GetAnattrib(offset).Symbol;

                // In cc65, variable declarations end the local label scope.  We insert a
                // fake global symbol if we encounter a table with a nonzero number of entries.
                if (QuirkVariablesEndScope &&
                        mProject.LvTables.TryGetValue(offset, out LocalVariableTable value) &&
                        value.Count > 0) {
                    mGlobalFlags[offset] = true;
                    mGlobalLabels.Add(new OffsetLabel(offset, "!VARTAB!"));
                    continue;
                }
                if (sym == null) {
                    // No label at this offset.
                    continue;
                }

                if (first || !sym.CanBeLocal) {
                    first = false;
                    mGlobalFlags[offset] = true;
                    mGlobalLabels.Add(new OffsetLabel(offset, sym.Label));

                    // Don't add to pairs list.
                    continue;
                }

                // If nothing actually references this label, the xref set will be empty.
                XrefSet xrefs = mProject.GetXrefSet(offset);
                if (xrefs != null) {
                    foreach (XrefSet.Xref xref in xrefs) {
                        if (!xref.IsByName) {
                            continue;
                        }

                        mOffsetPairs.Add(new OffsetPair(xref.Offset, offset));
                    }
                }
            }
        }

        /// <summary>
        /// Identifies all label reference pairs that cross the specified global label.  When
        /// a matching pair is found, the pair's destination label is marked as global and
        /// added to the global label list.
        /// </summary>
        /// <param name="glabel">Global label of interest.</param>
        private void FindIntersectingPairs(OffsetLabel glabel) {
            Debug.Assert(mGlobalFlags[glabel.Offset]);

            int glabOffset = glabel.Offset;
            for (int i = 0; i < mOffsetPairs.Count; i++) {
                OffsetPair pair = mOffsetPairs[i];

                // If the destination was marked global earlier, remove the entry and move on.
                if (mGlobalFlags[pair.DstOffset]) {
                    mOffsetPairs.RemoveAt(i);
                    i--;
                    continue;
                }

                // Check to see if the global label falls between the source and destination
                // offsets.
                //
                // If the reference source is itself a global label, it can reference local
                // labels forward, but not backward (i.e. if it crosses itself, the destination
                // must be made global).  We need to take that into account for the case where
                // label.Offset==pair.SrcOffset.
                bool intersect;
                if (pair.SrcOffset < pair.DstOffset) {
                    // Forward reference.  src==glab is ok
                    intersect = pair.SrcOffset < glabOffset && pair.DstOffset >= glabOffset;
                } else {
                    // Backward reference.  src==glab is bad
                    intersect = pair.SrcOffset >= glabOffset && pair.DstOffset <= glabOffset;
                }

                if (intersect) {
                    //Debug.WriteLine("Global " + glabel + " btwn " + pair + " (" +
                    //    mProject.GetAnattrib(pair.DstOffset).Symbol.Label + ")");

                    // Change the destination label to global.
                    mGlobalFlags[pair.DstOffset] = true;
                    mGlobalLabels.Add(new OffsetLabel(pair.DstOffset,
                        mProject.GetAnattrib(pair.DstOffset).Symbol.Label));

                    // Carefully remove it from the list we're iterating through.
                    mOffsetPairs.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Generates map entries for local labels defined between the two globals.
        /// </summary>
        /// <param name="startGlobal">Offset of first global.</param>
        /// <param name="endGlobal">Offset of second global.  If this range is at the end of the
        ///   file, this offset may be one past the end.</param>
        /// <param name="scopedLocals">Work object (minor alloc optimization).</param>
        private void ProcessLocals(int startGlobal, int endGlobal,
                Dictionary<string, string> scopedLocals) {
            //Debug.WriteLine("ProcessLocals: +" + startGlobal.ToString("x6") +
            //    " - +" + endGlobal.ToString("x6"));
            scopedLocals.Clear();
            bool remapUnders = (LocalPrefix == "_");

            for (int i = startGlobal + 1; i < endGlobal; i++) {
                Debug.Assert(!mGlobalFlags[i]);
                Symbol sym = mProject.GetAnattrib(i).Symbol;
                if (sym == null) {
                    continue;       // no label here
                }

                string newLabel = sym.LabelWithoutTag;
                if (remapUnders && newLabel[0] == '_') {
                    newLabel = LocalPrefix + NO_UNDER_PFX + newLabel;
                } else {
                    newLabel = LocalPrefix + newLabel;
                }

                if (scopedLocals.ContainsKey(newLabel)) {
                    newLabel = MakeUnique(newLabel, scopedLocals);
                }

                // Map from the original symbol label to the local form.  This works for
                // unique and non-unique locals.
                LabelMap[sym.Label] = newLabel;

                scopedLocals.Add(newLabel, newLabel);
            }
        }

        /// <summary>
        /// Alters a label to make it unique.  This may be called with a label that is unique
        /// but illegal (e.g. an instruction mnemonic), so we guarantee that the label returned
        /// is different from the argument.
        /// </summary>
        /// <remarks>
        /// We can't put a '_' at the front or an 'L' at the end (LDAL), since that could run
        /// afoul of the things we're trying to work around.  We don't want to mess with the
        /// start of the string since it may or may not have the LocalPrefix on it.
        /// </remarks>
        /// <param name="label">Label to uniquify.</param>
        /// <param name="allLabels">Dictionary to uniquify against.</param>
        /// <returns>Modified label</returns>
        private static string MakeUnique(string label, Dictionary<string, string> allLabels) {
            int uval = 0;
            string uniqueLabel;
            do {
                uval++;
                uniqueLabel = label + uval.ToString();
            } while (allLabels.ContainsKey(uniqueLabel));

            return uniqueLabel;
        }
    }
}
