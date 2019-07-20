/*
 * Copyright 2018 faddenSoft
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
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using CommonUtil;
using PluginCommon;

namespace SourceGenWF.Sandbox {
    /// <summary>
    /// This manages the PluginDll directory, which holds the compiled form of the extension
    /// scripts.  When a script is requested, this checks to see if the compiled form
    /// already exists.  If not, or the script source file is newer than the DLL file, the
    /// compiler is executed.
    /// 
    /// This is global -- it's not tied to an active project.
    /// 
    /// If an assembly is still loaded, the file on disk will be locked by the operating
    /// system and can't be replaced.  So long as the plugins run in an AppDomain sandbox,
    /// the locks will be cleared when the AppDomain is unloaded.
    /// </summary>
    public static class PluginDllCache {
        private const string PLUGIN_DIR_NAME = "PluginDll";

        /// <summary>
        /// List of assemblies for the CompilerParameters.ReferencedAssemblies argument.
        /// </summary>
        private static readonly string[] sRefAssem = new string[] {
            // Need this for various things to work, like System.Collections.Generic.
            "netstandard.dll",

            // Plugins are implemented in terms of interfaces defined here.
            "PluginCommon.dll",

            // Common utility functions.
            "CommonUtil.dll",
        };

        /// <summary>
        /// Path to plugin directory.
        /// </summary>
        private static string sPluginDirPath;


        /// <summary>
        /// Computes the path to the plugin directory.  Does not attempt to verify that it exists.
        /// </summary>
        /// <returns>Plugin directory path, or null if we can't find the application data
        ///   area.</returns>
        public static string GetPluginDirPath() {
            if (sPluginDirPath == null) {
                string runtimeUp = Path.GetDirectoryName(RuntimeDataAccess.GetDirectory());
                if (runtimeUp == null) {
                    return null;
                }
                sPluginDirPath = Path.Combine(runtimeUp, PLUGIN_DIR_NAME);
            }
            return sPluginDirPath;
        }

        /// <summary>
        /// Prepares the plugin directory.  Creates it and copies PluginCommon.dll in.
        /// Throws an exception if something fails.
        /// </summary>
        public static void PreparePluginDir() {
            string dstDir = GetPluginDirPath();
            if (File.Exists(dstDir) && !Directory.Exists(dstDir)) {
                throw new IOException(
                    string.Format(Properties.Resources.ERR_FILE_EXISTS_NOT_DIR, dstDir));
            }
            Directory.CreateDirectory(dstDir);

            // TODO(someday): try to remove *.dll where the modification date is more than a
            // week old -- this will prevent us from accreting stuff indefinitely.

            // Copy PluginCommon and CommonUtil over.
            CopyIfNewer(typeof(PluginCommon.PluginManager).Assembly.Location, dstDir);
            CopyIfNewer(typeof(CommonUtil.CRC32).Assembly.Location, dstDir);
        }

        /// <summary>
        /// Copies a DLL file if it's not present in the destination directory, or
        /// if it's newer than what's in the destination directory.
        /// </summary>
        /// <param name="srcDll">Full path to DLL file.</param>
        /// <param name="dstDir">Destination directory.</param>
        private static void CopyIfNewer(string srcDll, string dstDir) {
            string dstFile = Path.Combine(dstDir, Path.GetFileName(srcDll));
            if (FileUtil.FileMissingOrOlder(dstFile, srcDll)) {
                Debug.WriteLine("Copying " + srcDll + " to " + dstFile);
                File.Copy(srcDll, dstFile, true);
            }

            // Should we copy the .pdb files too, if they exist?  If they don't exist in
            // the source directory, do we need to remove them from the destination directory?
        }

        /// <summary>
        /// Prepares the DLL for the specified script, compiling it if necessary.
        /// </summary>
        /// <param name="scriptIdent">Script identifier.</param>
        /// <param name="projectPathName">Project file name, used for naming project-local
        ///   files.  May be empty if the project hasn't been named yet (in which case
        ///   project-local files will cause a failure).</param>
        /// <param name="report">Report with errors and warnings.</param>
        /// <returns>Full path to DLL, or null if compilation failed.</returns>
        public static string GenerateScriptDll(string scriptIdent, string projectPathName,
                out FileLoadReport report) {
            ExternalFile ef = ExternalFile.CreateFromIdent(scriptIdent);
            if (ef == null) {
                Debug.Assert(false);
                report = new FileLoadReport("CreateFromIdent failed");
                return null;
            }
            string projectDir = string.Empty;
            if (!string.IsNullOrEmpty(projectPathName)) {
                projectDir = Path.GetDirectoryName(projectPathName);
            }
            string srcPathName = ef.GetPathName(projectDir);

            // Fail if the source script doesn't exist.  If a previously-compiled DLL is present
            // we could just continue to use it, but that seems contrary to expectation, and
            // means that you won't notice that your project is broken until you clear out
            // the DLL directory.
            if (!File.Exists(srcPathName)) {
                report = new FileLoadReport(srcPathName);
                report.Add(FileLoadItem.Type.Error,
                    string.Format(Properties.Resources.ERR_FILE_NOT_FOUND, srcPathName));
                return null;
            }

            string destFileName = ef.GenerateDllName(projectPathName);
            string destPathName = Path.Combine(GetPluginDirPath(), destFileName);

            // Compile if necessary.
            if (FileUtil.FileMissingOrOlder(destPathName, srcPathName)) {
                Debug.WriteLine("Compiling " + srcPathName + " to " + destPathName);
                Assembly asm = CompileCode(srcPathName, destPathName, out report);
                if (asm == null) {
                    return null;
                }
            } else {
                Debug.WriteLine("NOT recompiling " + srcPathName);
                report = new FileLoadReport(srcPathName);
            }

            return destPathName;
        }

        /// <summary>
        /// Compiles the script from the specified pathname into an Assembly.
        /// </summary>
        /// <param name="scriptPathName">Script pathname.</param>
        /// <param name="dllPathName">Full pathname for output DLL.</param>
        /// <param name="report">Errors and warnings reported by the compiler.</param>
        /// <returns>Reference to script instance, or null on failure.</returns>
        private static Assembly CompileCode(string scriptPathName, string dllPathName,
                out FileLoadReport report) {
            report = new FileLoadReport(scriptPathName);

            Microsoft.CSharp.CSharpCodeProvider csProvider =
                new Microsoft.CSharp.CSharpCodeProvider();

            CompilerParameters parms = new CompilerParameters();
            // We want a DLL, not an EXE.
            parms.GenerateExecutable = false;
            // Save to disk so other AppDomain can load it.
            parms.GenerateInMemory = false;
            // Be vocal about warnings.
            parms.WarningLevel = 3;
            // Optimization is nice.
            parms.CompilerOptions = "/optimize";
            // Output file name.  Must be named appropriately so it can be found.
            parms.OutputAssembly = dllPathName;
            // Add dependencies.
            parms.ReferencedAssemblies.AddRange(sRefAssem);
#if DEBUG
            // This creates a .pdb file, which allows breakpoints to work.
            parms.IncludeDebugInformation = true;
#endif

            // Using the "from file" version has an advantage over the "from source"
            // version in that the debugger can find the source file, so things like
            // breakpoints work correctly.
            CompilerResults cr = csProvider.CompileAssemblyFromFile(parms, scriptPathName);
            CompilerErrorCollection cec = cr.Errors;
            foreach (CompilerError ce in cr.Errors) {
                report.Add(ce.Line, ce.Column,
                    ce.IsWarning ? FileLoadItem.Type.Warning : FileLoadItem.Type.Error,
                    ce.ErrorText);
            }
            if (cr.Errors.HasErrors) {
                return null;
            } else {
                Debug.WriteLine("Compilation successful");
                return cr.CompiledAssembly;
            }
        }

        /// <summary>
        /// Finds the first concrete class that implements IPlugin, and
        /// constructs an instance.
        /// </summary>
        public static IPlugin ConstructIPlugin(Assembly asm) {
            foreach (Type type in asm.GetExportedTypes()) {
                // Using a System.Linq extension method.
                if (type.IsClass && !type.IsAbstract &&
                    type.GetInterfaces().Contains(typeof(IPlugin))) {

                    ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                    IPlugin iplugin = (IPlugin)ctor.Invoke(null);
                    Debug.WriteLine("Created instance: " + iplugin);
                    return iplugin;
                }
            }
            return null;
        }
    }
}
