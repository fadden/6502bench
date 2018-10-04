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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using CommonUtil;
using PluginCommon;

namespace SourceGen.Sandbox {
    /// <summary>
    /// Maintains a collection of IPlugin instances, or communicates with the remote
    /// PluginManager that holds the collection.  Whether the plugins are instantiated
    /// locally depends on how the class is constructed.
    /// </summary>
    public class ScriptManager {
        public const string FILENAME_EXT = ".cs";
        public static readonly string FILENAME_FILTER = Properties.Resources.FILE_FILTER_CS;

        /// <summary>
        /// If true, the DomainManager will use the keep-alive timer hack.
        /// </summary>
        public static bool UseKeepAliveHack { get; set; }

        /// <summary>
        /// Reference to DomainManager, if we're using one.
        /// </summary>
        public DomainManager DomainMgr { get; private set; }

        /// <summary>
        /// Collection of loaded plugins, if we're not using a DomainManager.
        /// </summary>
        private Dictionary<string, IPlugin> mActivePlugins;

        /// <summary>
        /// Reference to project, from which we can get the file data and project path name.
        /// </summary>
        private DisasmProject mProject;


        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptManager(DisasmProject proj) {
            mProject = proj;

            if (!proj.UseMainAppDomainForPlugins) {
                DomainMgr = new DomainManager(UseKeepAliveHack);
                DomainMgr.CreateDomain("Plugin Domain", PluginDllCache.GetPluginDirPath());
                DomainMgr.PluginMgr.SetFileData(proj.FileData);
            } else {
                mActivePlugins = new Dictionary<string, IPlugin>();
            }
        }

        /// <summary>
        /// Cleans up, discarding the AppDomain if one was created.  Do not continue to use
        /// the object after calling this.
        /// </summary>
        public void Cleanup() {
            if (DomainMgr != null) {
                DomainMgr.Dispose();
                DomainMgr = null;
            }
            mActivePlugins = null;
            mProject = null;
        }

        /// <summary>
        /// Clears the list of plugins.  This does not unload assemblies.  Call this when
        /// the list of extension scripts configured into the project has changed.
        /// </summary>
        public void Clear() {
            if (DomainMgr == null) {
                mActivePlugins.Clear();
            } else {
                DomainMgr.PluginMgr.ClearPluginList();
            }
        }

        /// <summary>
        /// Attempts to load the specified plugin.  If the plugin is already loaded, this
        /// does nothing.  If not, the assembly is loaded and an instance is created.
        /// </summary>
        /// <param name="scriptIdent">Script identifier.</param>
        /// <param name="report">Report with errors and warnings.</param>
        /// <returns>True on success.</returns>
        public bool LoadPlugin(string scriptIdent, out FileLoadReport report) {
            // Make sure the most recent version is compiled.
            string dllPath = PluginDllCache.GenerateScriptDll(scriptIdent,
                mProject.ProjectPathName, out report);
            if (dllPath == null) {
                return false;
            }

            if (DomainMgr == null) {
                if (mActivePlugins.TryGetValue(scriptIdent, out IPlugin plugin)) {
                    return true;
                }
                Assembly asm = Assembly.LoadFile(dllPath);
                plugin = PluginDllCache.ConstructIPlugin(asm);
                mActivePlugins.Add(scriptIdent, plugin);
                report = new FileLoadReport(dllPath);       // empty report
                return true;
            } else {
                IPlugin plugin = DomainMgr.PluginMgr.LoadPlugin(dllPath, scriptIdent);
                return plugin != null;
            }
        }

        public IPlugin GetInstance(string scriptIdent) {
            if (DomainMgr == null) {
                if (mActivePlugins.TryGetValue(scriptIdent, out IPlugin plugin)) {
                    return plugin;
                }
                Debug.Assert(false);
                return null;
            } else {
                return DomainMgr.PluginMgr.GetPlugin(scriptIdent);
            }
        }

        /// <summary>
        /// Generates a list of references to instances of loaded plugins.
        /// </summary>
        /// <returns>Newly-created list of plugin references.</returns>
        public List<IPlugin> GetAllInstances() {
            if (DomainMgr == null) {
                List<IPlugin> list = new List<IPlugin>(mActivePlugins.Count);
                foreach (KeyValuePair<string, IPlugin> kvp in mActivePlugins) {
                    list.Add(kvp.Value);
                }
                return list;
            } else {
                return DomainMgr.PluginMgr.GetActivePlugins();
            }
        }

        /// <summary>
        /// Prepares all active scripts for action.
        /// </summary>
        /// <param name="appRef">Reference to object providing app services.</param>
        public void PrepareScripts(IApplication appRef) {
            List<PlatSym> platSyms = GeneratePlatSymList();

            if (DomainMgr == null) {
                foreach (KeyValuePair<string, IPlugin> kvp in mActivePlugins) {
                    kvp.Value.Prepare(appRef, mProject.FileData, platSyms);
                }
            } else {
                DomainMgr.PluginMgr.PreparePlugins(appRef, platSyms);
            }
        }

        /// <summary>
        /// Gathers a list of platform symbols from the project's symbol table.
        /// </summary>
        private List<PlatSym> GeneratePlatSymList() {
            List<PlatSym> platSyms = new List<PlatSym>();
            SymbolTable symTab = mProject.SymbolTable;

            foreach (Symbol sym in symTab) {
                if (!(sym is DefSymbol)) {
                    // ignore user labels
                    continue;
                }
                DefSymbol defSym = sym as DefSymbol;
                if (defSym.SymbolSource != Symbol.Source.Platform) {
                    // ignore project symbols
                    continue;
                }

                platSyms.Add(new PlatSym(defSym.Label, defSym.Value, defSym.Tag));
            }

            return platSyms;
        }

        /// <summary>
        /// For debugging purposes, get some information about the currently loaded
        /// extension scripts.
        /// </summary>
        public string DebugGetLoadedScriptInfo() {
            StringBuilder sb = new StringBuilder();
            if (DomainMgr == null) {
                foreach (KeyValuePair<string, IPlugin> kvp in mActivePlugins) {
                    string loc = kvp.Value.GetType().Assembly.Location;
                    sb.Append("[main] ");
                    sb.Append(loc);
                    sb.Append("\r\n  ");
                    DebugGetScriptInfo(kvp.Value, sb);
                }
            } else {
                List<IPlugin> plugins = DomainMgr.PluginMgr.GetActivePlugins();
                foreach (IPlugin plugin in plugins) {
                    string loc = DomainMgr.PluginMgr.GetPluginAssemblyLocation(plugin);
                    sb.AppendFormat("[sub {0}] ", DomainMgr.Id);
                    sb.Append(loc);
                    sb.Append("\r\n  ");
                    DebugGetScriptInfo(plugin, sb);
                }
            }

            return sb.ToString();
        }
        private void DebugGetScriptInfo(IPlugin plugin, StringBuilder sb) {
            sb.Append(plugin.Identifier);
            sb.Append("\r\n");
        }
    }
}
