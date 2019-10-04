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
using System.Linq;
using System.Reflection;
using System.Threading;

namespace PluginCommon {
    /// <summary>
    /// Manages loaded plugins, in the "remote" AppDomain.
    /// </summary>
    public sealed class PluginManager : MarshalByRefObject {
        /// <summary>
        /// Collection of instances of active plugins, keyed by script identifier.  Other
        /// plugin assemblies may be present in the AppDomain, but have not been identified
        /// by the application as being of interest.
        /// </summary>
        private Dictionary<string, IPlugin> mActivePlugins = new Dictionary<string, IPlugin>();

        /// <summary>
        /// Reference to file data.
        /// </summary>
        private byte[] mFileData;


        /// <summary>
        /// Constructor, invoked from CreateInstanceAndUnwrap().
        /// </summary>
        public PluginManager() {
            Debug.WriteLine("PluginManager ctor (id=" + AppDomain.CurrentDomain.Id + ")");

            // Seems to require [SecurityCritical]
            //Type lsc = Type.GetType("System.Runtime.Remoting.Lifetime.LifetimeServices");
            //PropertyInfo prop = lsc.GetProperty("LeaseTime");
            //prop.SetValue(null, TimeSpan.FromSeconds(30));
        }

        ~PluginManager() {
            Debug.WriteLine("~PluginManager (id=" + AppDomain.CurrentDomain.Id + ")");
        }

        /// <summary>
        /// Sets the file data to use for all plugins.
        /// 
        /// The file data argument will be an AppDomain-local copy of the data, made by the
        /// argument marshalling code.  So plugins can scribble all over it without trashing
        /// the original.  We want to store it in PluginManager so we don't make a new copy
        /// for each individual plugin.
        /// </summary>
        /// <param name="fileData">65xx code and data.</param>
        public void SetFileData(byte[] fileData) {
            mFileData = fileData;
        }

        /// <summary>
        /// Tests simple round-trip communication.  This may be called from an arbitrary thread.
        /// </summary>
        public int Ping(int val) {
            Debug.WriteLine("PluginManager Ping tid=" + Thread.CurrentThread.ManagedThreadId +
                " (id=" + AppDomain.CurrentDomain.Id + "): " + val);
            return val + 1;
        }

        /// <summary>
        /// Creates a plugin instance from a compiled assembly.  Pass in the script identifier
        /// for future lookups.  If the plugin has already been instantiated, that object
        /// will be returned.
        /// </summary>
        /// <param name="dllPath">Full path to compiled assembly.</param>
        /// <param name="scriptIdent">Identifier to use in e.g. GetPlugin().</param>
        /// <returns>Reference to plugin instance.</returns>
        public IPlugin LoadPlugin(string dllPath, string scriptIdent) {
            if (mActivePlugins.TryGetValue(dllPath, out IPlugin ip)) {
                Debug.WriteLine("PM: returning cached plugin for " + dllPath);
                return ip;
            }

            Assembly asm = Assembly.LoadFile(dllPath);

            foreach (Type type in asm.GetExportedTypes()) {
                // Using a System.Linq extension method.
                if (type.IsClass && !type.IsAbstract &&
                    type.GetInterfaces().Contains(typeof(IPlugin))) {

                    ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                    IPlugin iplugin = (IPlugin)ctor.Invoke(null);
                    Debug.WriteLine("PM created instance: " + iplugin);
                    mActivePlugins.Add(scriptIdent, iplugin);
                    return iplugin;
                }
            }
            throw new Exception("No IPlugin class found in " + dllPath);
        }

        /// <summary>
        /// Gets an instance of a previously-loaded plugin.
        /// </summary>
        /// <param name="scriptIdent">Script identifier that was passed to LoadPlugin().</param>
        /// <returns>Reference to instance of plugin.</returns>
        public IPlugin GetPlugin(string scriptIdent) {
            if (mActivePlugins.TryGetValue(scriptIdent, out IPlugin plugin)) {
                return plugin;
            }
            return null;
        }

        /// <summary>
        /// Returns a string with the assembly's location.
        /// </summary>
        public string GetPluginAssemblyLocation(IPlugin plugin) {
            return plugin.GetType().Assembly.Location;
        }

        /// <summary>
        /// Generates a list of references to instances of active plugins.
        /// </summary>
        /// <returns>Newly-created list of plugin references.</returns>
        public List<IPlugin> GetActivePlugins() {
            List<IPlugin> list = new List<IPlugin>(mActivePlugins.Count);
            foreach (KeyValuePair<string, IPlugin> kvp in mActivePlugins) {
                list.Add(kvp.Value);
            }
            Debug.WriteLine("PluginManager: returning " + list.Count + " plugins (id=" +
                AppDomain.CurrentDomain.Id + ")");
            return list;
        }

        /// <summary>
        /// Clears the list of loaded plugins.  This does not unload the assemblies from
        /// the AppDomain.
        /// </summary>
        public void ClearPluginList() {
            mActivePlugins.Clear();
        }

        /// <summary>
        /// Invokes the Prepare() method on all active plugins.
        /// </summary>
        /// <param name="appRef">Reference to host object providing app services.</param>
        public void PreparePlugins(IApplication appRef, List<PlSymbol> plSyms) {
            foreach (KeyValuePair<string, IPlugin> kvp in mActivePlugins) {
                kvp.Value.Prepare(appRef, mFileData, plSyms);
            }
        }

#if false
        /// <summary>
        /// DEBUG ONLY: establish a fast lease timeout.  Normally the lease
        /// is five minutes; this reduces it to a few seconds.  (The actual time is
        /// also affected by LifetimeServices.LeaseManagerPollTime, which defaults
        /// to 10 seconds.)
        /// 
        /// Unfortunately this must be tagged [SecurityCritical] to match the method being
        /// overridden, but in a partially-trusted sandbox that's not allowed.  You have
        /// to relax security entirely for this to work.
        /// </summary>
        //[SecurityPermissionAttribute(SecurityAction.Demand,
        //        Flags = SecurityPermissionFlag.Infrastructure)]
        [System.Security.SecurityCritical]
        public override object InitializeLifetimeService() {
            object lease = base.InitializeLifetimeService();

            // netstandard2.0 doesn't have System.Runtime.Remoting.Lifetime, so use reflection
            PropertyInfo leaseState = lease.GetType().GetProperty("CurrentState");
            PropertyInfo initialLeaseTime = lease.GetType().GetProperty("InitialLeaseTime");
            PropertyInfo sponsorshipTimeout = lease.GetType().GetProperty("SponsorshipTimeout");
            PropertyInfo renewOnCallTime = lease.GetType().GetProperty("RenewOnCallTime");

            Console.WriteLine("Default lease: ini=" +
                initialLeaseTime.GetValue(lease) + " spon=" +
                sponsorshipTimeout.GetValue(lease) + " renOC=" +
                renewOnCallTime.GetValue(lease));

            if ((int)leaseState.GetValue(lease) == 1 /*LeaseState.Initial*/) {
                // Initial lease duration.
                initialLeaseTime.SetValue(lease, TimeSpan.FromSeconds(8));

                // How long we will wait for the sponsor to respond
                // with a lease renewal time.
                sponsorshipTimeout.SetValue(lease, TimeSpan.FromSeconds(5));

                // Each call to the remote object extends the lease so that
                // it has at least this much time left.
                renewOnCallTime.SetValue(lease, TimeSpan.FromSeconds(2));
            }
            return lease;
        }
#endif
    }
}
