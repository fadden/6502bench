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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Security.Permissions;
using System.Timers;
using PluginCommon;

namespace SourceGen.Sandbox {
    /// <summary>
    /// This is a host-side object that manages the plugin AppDomain.
    /// </summary>
    //[SecurityPermission(SecurityAction.LinkDemand, ControlAppDomain = true, Infrastructure = true)]
    public class DomainManager : IDisposable {
        /// <summary>
        /// For IDisposable.
        /// </summary>
        private bool mDisposed = false;

        /// <summary>
        /// AppDomain handle.
        /// </summary>
        private AppDomain mAppDomain;

        /// <summary>
        /// Reference to the remote PluginManager object.
        /// </summary>
        private Sponsor<PluginManager> mPluginManager;

        /// <summary>
        /// Hack to keep the sandbox from disappearing.
        /// </summary>
        private Timer mKeepAliveTimer;

        /// <summary>
        /// Access the remote PluginManager object.
        /// </summary>
        public PluginManager PluginMgr {
            get {
                //Debug.Assert(mPluginManager.CheckLease());
                return mPluginManager.Instance;
            }
        }

        /// <summary>
        /// App domain ID, or -1 if not available.
        /// </summary>
        public int Id { get { return mAppDomain != null ? mAppDomain.Id : -1; } }


        public DomainManager(bool useKeepAlive) {
            // Sometimes the sandbox AppDomain can't call back into the main AppDomain to
            // get a lease renewal, and the PluginManager object gets collected.  See
            // https://stackoverflow.com/q/52230527/294248 for details.
            //
            // The idea is to keep tickling renew-on-call, so that the plugin side never
            // has to request renewal.  This is ugly but seems to work.
            //
            // The timer event runs on a pool thread, and calls across domains seem to stay
            // on the same thread, so the remote Ping() method must be prepared to be called
            // on an arbitrary thread.
            if (useKeepAlive) {
                Debug.WriteLine("Setting keep-alive timer...");
                mKeepAliveTimer = new Timer(60 * 1000);
                mKeepAliveTimer.Elapsed += (source, e) => {
                    // The Timer docs say that Elapsed events can occur after Stop(), because
                    // the signal to raise Elapsed is queued on a thread pool thread.  Instead
                    // of being careful we just wrap it in try/catch, since nothing bad happens
                    // if this fails.
                    try {
                        int result = mPluginManager.Instance.Ping(1000);
                        Debug.WriteLine("KeepAlive tid=" +
                            System.Threading.Thread.CurrentThread.ManagedThreadId +
                            " result=" + result);
                    } catch (Exception ex) {
                        Debug.WriteLine("Keep-alive timer failed: " + ex.Message);
                    }
                };
                mKeepAliveTimer.AutoReset = true;
                mKeepAliveTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Creates a new AppDomain.  If our plugin is just executing
        /// pre-compiled code we can lock the permissions down, but if
        /// it needs to dynamically compile code we need to open things up.
        /// </summary>
        /// <param name="appDomainName">The "friendly" name.</param>
        /// <param name="appBaseBath">Directory to use for ApplicationBase.</param>
        public void CreateDomain(string appDomainName, string appBaseBath) {
            // This doesn't seem to affect Sponsor.  Doing this over in the PluginManager
            // does have the desired effect, but requires unrestricted security.
            //LifetimeServices.LeaseTime = TimeSpan.FromSeconds(5);
            //LifetimeServices.LeaseManagerPollTime = TimeSpan.FromSeconds(3);
            //LifetimeServices.RenewOnCallTime = TimeSpan.FromSeconds(2);
            //LifetimeServices.SponsorshipTimeout = TimeSpan.FromSeconds(1);

            if (mAppDomain != null) {
                throw new Exception("Domain already created");
            }

            PermissionSet permSet;
            // Start with everything disabled.
            permSet = new PermissionSet(PermissionState.None);
            //permSet = new PermissionSet(PermissionState.Unrestricted);

            // Allow code execution.
            permSet.AddPermission(new SecurityPermission(
                SecurityPermissionFlag.Execution));

            // This appears to be necessary to allow the lease renewal to work.  Without
            // this the lease silently fails to renew.
            permSet.AddPermission(new SecurityPermission(
                SecurityPermissionFlag.Infrastructure));

            // Allow changes to Remoting stuff.  Without this, we can't
            // register our ISponsor.
            permSet.AddPermission(new SecurityPermission(
                SecurityPermissionFlag.RemotingConfiguration));

            // Allow read-only file access, but only in the plugin directory.
            // This is necessary to allow PluginLoader to load the assembly.
            FileIOPermission fp = new FileIOPermission(
                FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery,
                appBaseBath);
            permSet.AddPermission(fp);

            // TODO(maybe): it looks like this would allow us to mark the PluginCommon dll as
            // trusted, so we wouldn't have to give the above permissions to everything.
            // That seems to require a cryptographic pair and some other voodoo.
            //StrongName fullTrustAssembly =
            //    typeof(PluginManager).Assembly.Evidence.GetHostEvidence<StrongName>();

            // Configure the AppDomain.  Setting the ApplicationBase directory away from
            // the main app location is apparently very important, as it mitigates the
            // risk of certain exploits from untrusted plugin code.
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ApplicationBase = appBaseBath;

            // Create the AppDomain.
            mAppDomain = AppDomain.CreateDomain(appDomainName, null, adSetup, permSet);

            Debug.WriteLine("Created AppDomain '" + appDomainName + "', id=" + mAppDomain.Id);
            //Debug.WriteLine("Loading '" + typeof(PluginManager).Assembly.FullName + "' / '" +
            //    typeof(PluginManager).FullName + "'");

            // Create a PluginManager in the remote AppDomain.  The local
            // object is actually a proxy.
            PluginManager pm = (PluginManager)mAppDomain.CreateInstanceAndUnwrap(
                typeof(PluginManager).Assembly.FullName,
                typeof(PluginManager).FullName);

            // Wrap it so it doesn't disappear on us.
            mPluginManager = new Sponsor<PluginManager>(pm);

            Debug.WriteLine("IsTransparentProxy: " +
                System.Runtime.Remoting.RemotingServices.IsTransparentProxy(pm));
        }

        /// <summary>
        /// Destroy the AppDomain.
        /// </summary>
        private void DestroyDomain(bool disposing) {
            Debug.WriteLine("Unloading AppDomain '" + mAppDomain.FriendlyName +
                "', id=" + mAppDomain.Id + ", disposing=" + disposing);
            if (mKeepAliveTimer != null) {
                mKeepAliveTimer.Stop();
                mKeepAliveTimer.Dispose();
                mKeepAliveTimer = null;
            }
            if (mPluginManager != null) {
                mPluginManager.Dispose();
                mPluginManager = null;
            }
            if (mAppDomain != null) {
                // We can't simply invoke AppDomain.Unload() from a finalizer.  The unload is
                // handled by a thread that won't run at the same time as the finalizer thread,
                // so if we got here through finalization we will deadlock.  Fortunately the
                // runtime sees the situation and throws an exception out of Unload().
                //
                // If we don't have a finalizer, and we forget to make an explicit cleanup
                // call, the AppDomain will stick around and keep the DLL files locked, which
                // could be annoying if the user is trying to iterate on extension script
                // development.
                //
                // So we use a workaround from https://stackoverflow.com/q/4064749/294248
                // and invoke it asynchronously.
                if (disposing) {
                    AppDomain.Unload(mAppDomain);
                } else {
                    new Action<AppDomain>(AppDomain.Unload).BeginInvoke(mAppDomain, null, null);
                }
                mAppDomain = null;
            }
        }

        /// <summary>
        /// Finalizer.  Required for IDisposable.
        /// </summary>
        ~DomainManager() {
            Debug.WriteLine("WARNING: DomainManager finalizer running (id=" +
                (mAppDomain != null ? mAppDomain.Id.ToString() : "--") + ")");
            Dispose(false);
        }

        /// <summary>
        /// Generic IDisposable implementation.
        /// </summary>
        public void Dispose() {
            // Dispose of unmanaged resources (i.e. the AppDomain).
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destroys the AppDomain, if one was created.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
        protected virtual void Dispose(bool disposing) {
            if (mDisposed) {
                return;
            }

            if (disposing) {
                // Free *managed* objects here.  This is mostly an
                // optimization, as such things will be disposed of
                // eventually by the GC.
            }

            // Free unmanaged objects (i.e. the AppDomain).
            if (mAppDomain != null) {
                DestroyDomain(disposing);
            }

            mDisposed = true;
        }
    }
}
