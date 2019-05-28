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
using System.Diagnostics;
using System.Runtime.Remoting.Lifetime;
using System.Security.Permissions;

namespace SourceGenWPF.Sandbox {
    /// <summary>
    /// This wraps a MarshalByRefObject instance with a "sponsor".  This
    /// is necessary because objects created by the host in the plugin
    /// AppDomain aren't strongly referenced across the boundary (the two
    /// AppDomains have independent garbage collection).  Because the plugin
    /// AppDomain can't know when the host AppDomain discards its objects,
    /// it will discard remote-proxied objects on its side after a period of disuse.
    ///
    /// The ISponsor/ILease mechanism provides a way for the host-side object
    /// to define the lifespan of the plugin-side objects.  The object
    /// manager in the plugin AppDomain will invoke Renewal() back in the host-side
    /// AppDomain.
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    class Sponsor<T> : MarshalByRefObject, ISponsor, IDisposable where T : MarshalByRefObject {

        /// <summary>
        /// The object we've wrapped.
        /// </summary>
        private T mObj;

        /// <summary>
        /// For IDisposable.
        /// </summary>
        private bool mDisposed = false;

        // For debugging, track the last renewal time.
        private DateTime mLastRenewal = DateTime.Now;


        public T Instance {
            get {
                if (mDisposed) {
                    throw new ObjectDisposedException("Sponsor was disposed");
                } else {
                    return mObj;
                }
            }
        }

        public Sponsor(T obj) {
            mObj = obj;

            // Get the lifetime service lease from the MarshalByRefObject,
            // and register ourselves as a sponsor.
            ILease lease = (ILease)obj.GetLifetimeService();
            lease.Register(this);

            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "|Sponsor created; initLt=" +
                lease.InitialLeaseTime + " renOC=" + lease.RenewOnCallTime +
                " spon=" + lease.SponsorshipTimeout);
        }

        public bool CheckLease() {
            try {
                ILease lease = (ILease)mObj.GetLifetimeService();
                if (lease.CurrentState != LeaseState.Active) {
                    Debug.WriteLine("WARNING: lease has expired for " + mObj);
                    return false;
                }
            } catch (System.Runtime.Remoting.RemotingException ex) {
                Debug.WriteLine("WARNING: remote object gone: " + ex.Message);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Extends the lease time for the wrapped object.  This is called
        /// from the plugin AppDomain, but executes on the host AppDomain.
        /// </summary>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
                Flags = SecurityPermissionFlag.Infrastructure)]
        TimeSpan ISponsor.Renewal(ILease lease) {
            DateTime now = DateTime.Now;
            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "|Lease renewal for " + mObj +
                ", last renewed " + (now - mLastRenewal) + " sec ago; renewing for " +
                lease.RenewOnCallTime + " (host id=" + AppDomain.CurrentDomain.Id + ")");
            mLastRenewal = now;

            if (mDisposed) {
                // Shouldn't happen -- we should be unregistered -- but I
                // don't know if multiple threads are involved.
                Debug.WriteLine("WARNING: attempted to renew a disposed Sponsor");
                return TimeSpan.Zero;
            } else {
                // Use the lease's RenewOnCallTime.
                return lease.RenewOnCallTime;
            }
        }

        /// <summary>
        /// Finalizer.  Required for IDisposable.
        /// </summary>
        ~Sponsor() {
            Dispose(false);
        }

        /// <summary>
        /// Generic IDisposable implementation.
        /// </summary>
        public void Dispose() {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destroys the Sponsor, if one was created.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
        protected virtual void Dispose(bool disposing) {
            if (mDisposed) {
                return;
            }
            Debug.WriteLine("Sponsor.Dispose(disposing=" + disposing + ")");

            // If this is a managed object, call its Dispose method.
            if (disposing) {
                if (mObj is IDisposable) {
                    ((IDisposable)mObj).Dispose();
                }
            }

            // Remove ourselves from the lifetime service.
            // NOTE: if you see this blowing up at app shutdown, it's because you didn't
            //   call Dispose() on the DomainManager.
            object leaseObj;
            try {
                leaseObj = mObj.GetLifetimeService();
            } catch (Exception ex) {
                // This seems to happen when we shut down without having disposed of the
                // AppDomain, probably when a Sponsor's finalizer runs before the
                // DomainManager's finalizer.  Sometimes it also happens when you seem to
                // be doing everything right, though this seems to correspond with a lack
                // of lease renewal messages (i.e. something is really wrong as the other end).
                //
                // I think failures here can be ignored, since it's just failure to clean up
                // something that doesn't exist.
                //
                // Sometimes it's:
                //  RemotingException: Object '---' has been disconnected or does not exist at the server.
                Debug.WriteLine("WARNING: GetLifetimeService failed: " + ex.Message);
                leaseObj = null;
            }
            if (leaseObj is ILease) {
                ILease lease = (ILease)leaseObj;
                try {
                    lease.Unregister(this);
                } catch (InvalidOperationException ex) {
                    // TODO: not expected -- why did this start happening?
                    Debug.WriteLine("WARNING: lease.Unregister threw " + ex);
                }
            }

            mDisposed = true;
        }
    }
}
