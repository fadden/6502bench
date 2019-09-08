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
they're re-used.  This is useful for short loops.

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
        /// <summary>
        /// A pairing of an offset with a label string.  (Essentially mAnattribs[n].Symbol
        /// with all the fluff trimmed away.)
        /// 
        /// The label string isn't actually all that useful, since we can pull it back out
        /// of anattrib, but it makes life a little easier during debugging.  These get
        /// put into a List, so switching to a plain int offset doesn't necessarily help us
        /// much because the ints get boxed.
        /// </summary>
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

            // Step 1: generate source/target pairs and global label list
            GenerateLists();

            // Step 2: walk through the list of global symbols, identifying source/target
            // pairs that cross them.  If a pair matches, the target label is added to the
            // mGlobalLabels list, and removed from the pair list.
            for (int index = 0; index < mGlobalLabels.Count; index++) {
                FindIntersectingPairs(mGlobalLabels[index]);
            }

            // Step 3: for each local label, add an entry to the map with the appropriate
            // local-label syntax.
            LabelMap = new Dictionary<string, string>();
            for (int i = 0; i < mProject.FileDataLength; i++) {
                if (mGlobalFlags[i]) {
                    continue;
                }
                Symbol sym = mProject.GetAnattrib(i).Symbol;
                if (sym == null) {
                    continue;
                }

                LabelMap[sym.Label] = LocalPrefix + sym.Label;
            }

            // Take out the trash.
            mGlobalLabels.Clear();
            mOffsetPairs.Clear();
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

            for (int i = 0; i < mProject.FileDataLength; i++) {
                Symbol sym = mProject.GetAnattrib(i).Symbol;
                if (sym == null) {
                    // No label at this offset.
                    continue;
                }

                if (first || sym.SymbolType != Symbol.Type.LocalOrGlobalAddr) {
                    first = false;
                    mGlobalFlags[i] = true;
                    mGlobalLabels.Add(new OffsetLabel(i, sym.Label));

                    // Don't add to pairs list.
                    continue;
                }

                // If nothing actually references this label, the xref set will be empty.
                XrefSet xrefs = mProject.GetXrefSet(i);
                if (xrefs != null) {
                    foreach (XrefSet.Xref xref in xrefs) {
                        if (!xref.IsByName) {
                            continue;
                        }

                        mOffsetPairs.Add(new OffsetPair(xref.Offset, i));
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

            int globOffset = glabel.Offset;
            for (int i = 0; i < mOffsetPairs.Count; i++) {
                OffsetPair pair = mOffsetPairs[i];

                // If the destination was marked global earlier, remove and ignore this entry.
                // Note this also means that pair.DstOffset != label.Offset.
                if (mGlobalFlags[pair.DstOffset]) {
                    mOffsetPairs.RemoveAt(i);
                    i--;
                    continue;
                }

                // Check to see if the global label falls between the source and destination
                // offsets.
                //
                // If the reference source is itself a global label, it can reference local
                // labels forward, but not backward.  We need to take that into account for
                // the case where label.Offset==pair.SrcOffset.
                bool intersect;
                if (pair.SrcOffset < pair.DstOffset) {
                    // Forward reference.  src==glob is ok
                    intersect = pair.SrcOffset < globOffset && pair.DstOffset >= globOffset;
                } else {
                    // Backward reference.  src==glob is bad
                    intersect = pair.SrcOffset >= globOffset && pair.DstOffset <= globOffset;
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
        /// Adjusts the label map so that only local variables start with an underscore ('_').
        /// This is necessary for assemblers like 64tass that use a leading underscore to
        /// indicate that a label should be local.
        /// 
        /// This may be called even if label localization is disabled.  In that case we just
        /// create an empty label map and populate as needed.
        /// 
        /// Only call this if underscores are used to indicate local labels.
        /// </summary>
        public void MaskLeadingUnderscores() {
            bool allGlobal = false;
            if (LabelMap == null) {
                allGlobal = true;
                LabelMap = new Dictionary<string, string>();
            }

            // Throw out the original local label generation.
            LabelMap.Clear();

            // Use this to test for uniqueness.  We add all labels here as we go, not just the
            // ones being remapped.  For each label we either add the original or the localized
            // form.
            SortedList<string, string> allLabels = new SortedList<string, string>();

            for (int i = 0; i < mProject.FileDataLength; i++) {
                Symbol sym = mProject.GetAnattrib(i).Symbol;
                if (sym == null) {
                    // No label at this offset.
                    continue;
                }

                string newLabel;
                if (allGlobal || mGlobalFlags[i]) {
                    // Global symbol.  Don't let it start with '_'.
                    if (sym.Label.StartsWith("_")) {
                        // There's an underscore here that was added by the user.  Stick some
                        // other character in front.
                        newLabel = "X" + sym.Label;
                    } else {
                        // No change needed.
                        newLabel = sym.Label;
                    }
                } else {
                    // Local symbol.
                    if (sym.Label.StartsWith("_")) {
                        // The original starts with one or more underscores.  Adding another
                        // will create a "__" label, which is reserved in 64tass.
                        newLabel = "_X" + sym.Label;
                    } else {
                        newLabel = "_" + sym.Label;
                    }
                }

                // Make sure it's unique.
                string uniqueLabel = newLabel;
                int uval = 1;
                while (allLabels.ContainsKey(uniqueLabel)) {
                    uniqueLabel = newLabel + uval.ToString();
                }
                allLabels.Add(uniqueLabel, uniqueLabel);

                // If it's different, add it to the label map.
                if (sym.Label != uniqueLabel) {
                    LabelMap.Add(sym.Label, uniqueLabel);
                }
            }

            Debug.WriteLine("UMAP: allcount=" + allLabels.Count + " mapcount=" + LabelMap.Count);
        }
    }
}
