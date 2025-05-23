/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


#nullable disable

using AOT;
using System;

/// @file OvrAvatarLipSyncContextBase.cs

namespace Oculus.Avatar2
{
    /**
     * Base class for C# code to drive lipsync data for avatar entites.
     * @see OvrAvatarEntity.SetLipSync
     */
    public abstract class OvrAvatarLipSyncContextBase : OvrAvatarCallbackContextBase
    {
        // Cache the managed representation to reduce GC allocations
        private readonly OvrAvatarLipSyncState _lipsyncState = new OvrAvatarLipSyncState();
        internal CAPI.ovrAvatar2LipSyncContext DataContext { get; }

        protected OvrAvatarLipSyncContextBase()
        {
            var dataContext = new CAPI.ovrAvatar2LipSyncContext();
            dataContext.context = new IntPtr(id);
            dataContext.lipSyncCallback = LipSyncCallback;

            DataContext = dataContext;
        }

        /**
         * Gets the lip sync state from the native lipsync implementation.
         * The lip sync state contains the weights for the visemes to
         * make the lip expression.
         * Lip sync implementations must override this function to
         * convert the native lip sync state into a form usable by Unity.
         * @param lipsyncState  where to store the generated viseme weights.
         * @see OvrAvatarLipSyncState
         */
        protected abstract bool GetLipSyncState(OvrAvatarLipSyncState lipsyncState);

        [MonoPInvokeCallback(typeof(CAPI.LipSyncCallback))]
        private static bool LipSyncCallback(out CAPI.ovrAvatar2LipSyncState lipsyncState, IntPtr userContext)
        {
            try
            {
                var context = GetInstance<OvrAvatarLipSyncContextBase>(userContext);
                if (context != null)
                {
                    if (context.GetLipSyncState(context._lipsyncState))
                    {
                        lipsyncState = context._lipsyncState.ToNative();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogError(e.ToString());
            }

            lipsyncState = new CAPI.ovrAvatar2LipSyncState();
            return false;
        }

        public OvrAvatarLipSyncState DebugQueryLipSyncState()
        {
            if (GetLipSyncState(_lipsyncState))
            {
                return _lipsyncState;
            }
            return null;
        }
    }
}
