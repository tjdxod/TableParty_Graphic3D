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

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Oculus.Avatar2
{
    internal static class OvrPluginTracking
    {
#if !OVRPLUGIN_UNSUPPORTED_PLATFORM
#if UNITY_EDITOR || !UNITY_IOS
#if UNITY_EDITOR_OSX
        private const string LibFile = OvrAvatarPlugin.FullPluginFolderPath + "libovrplugintracking.framework/libovrplugintracking";
#else
        private const string LibFile = OvrAvatarManager.IsAndroidStandalone ? "ovrplugintracking" : "libovrplugintracking";
#endif  // UNITY_EDITOR_OSX
#else   // !UNITY_EDITOR && UNITY_IOS
        private const string LibFile = "__Internal";
#endif  // !UNITY_EDITOR && UNITY_IOS

        [StructLayout(LayoutKind.Sequential)]
        private ref struct ovrpTrackingInitializationData
        {
            public ovrpTrackingInitializationData(CAPI.ovrAvatar2Vector3f clientSpaceRightAxis,
                CAPI.ovrAvatar2Vector3f clientSpaceUpAxis, CAPI.ovrAvatar2Vector3f clientSpaceForwardAxis,
                CAPI.LoggingViewDelegate loggingDelegate, IntPtr loggingContext)
            {
                this.clientSpaceRightAxis = clientSpaceRightAxis;
                this.clientSpaceUpAxis = clientSpaceUpAxis;
                this.clientSpaceForwardAxis = clientSpaceForwardAxis;
                this.loggingDelegate = loggingDelegate;
                this.loggingContext = loggingContext;
            }

            private readonly CAPI.ovrAvatar2Vector3f clientSpaceRightAxis;
            private readonly CAPI.ovrAvatar2Vector3f clientSpaceUpAxis;
            private readonly CAPI.ovrAvatar2Vector3f clientSpaceForwardAxis;
            private readonly CAPI.LoggingViewDelegate loggingDelegate;
            private readonly IntPtr loggingContext;
        }

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_Initialize(in ovrpTrackingInitializationData initData);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ovrpTracking_Shutdown();


        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateInputTrackingContext(
            out CAPI.ovrAvatar2InputTrackingContext outContext);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrpTracking_CreateInputTrackingContext")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateInputTrackingContextNative(
            out CAPI.ovrAvatar2InputTrackingContextNative outContext);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateFaceTrackingContext(out CAPI.ovrAvatar2FacePoseProvider outContext);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrpTracking_CreateFaceTrackingContext")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateFaceTrackingContextNative(out CAPI.ovrAvatar2FacePoseProviderNative outContext);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateEyeTrackingContext(out CAPI.ovrAvatar2EyePoseProvider outContext);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrpTracking_CreateEyeTrackingContext")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateEyeTrackingContextNative(out CAPI.ovrAvatar2EyePoseProviderNative outContext);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateHandTrackingContext(
            out CAPI.ovrAvatar2HandTrackingDataContext outContext);

        [DllImport(LibFile, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrpTracking_CreateHandTrackingContext")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool ovrpTracking_CreateHandTrackingContextNative(
            out CAPI.ovrAvatar2HandTrackingDataContextNative outContext);


        public static bool Initialize(CAPI.LoggingViewDelegate callback, IntPtr logContext)
        {
            try
            {
                var initData = new ovrpTrackingInitializationData(Vector3.right, Vector3.up, Vector3.forward, callback,
                    logContext);
                return ovrpTracking_Initialize(in initData);
            }
            catch (DllNotFoundException)
            {
                OvrAvatarLog.LogWarning($"Lib {LibFile} not found");
                return false;
            }
        }

        public static void Shutdown()
        {
            try
            {
                ovrpTracking_Shutdown();
            }
            catch (DllNotFoundException)
            {

            }
        }


        private static CAPI.ovrAvatar2FacePoseProvider? CreateInternalFaceTrackingContext()
        {
            if (ovrpTracking_CreateFaceTrackingContext(out var context))
            {
                return context;
            }

            return null;
        }

        private static CAPI.ovrAvatar2FacePoseProviderNative? CreateInternalFaceTrackingContextNative()
        {
            if (ovrpTracking_CreateFaceTrackingContextNative(out var context))
            {
                return context;
            }

            return null;
        }

        private static CAPI.ovrAvatar2EyePoseProvider? CreateInternalEyeTrackingContext()
        {
            if (ovrpTracking_CreateEyeTrackingContext(out var context))
            {
                return context;
            }

            return null;
        }

        private static CAPI.ovrAvatar2EyePoseProviderNative? CreateInternalEyeTrackingContextNative()
        {
            if (ovrpTracking_CreateEyeTrackingContextNative(out var context))
            {
                return context;
            }

            return null;
        }

        private static CAPI.ovrAvatar2InputTrackingContext? CreateInputTrackingContext()
        {
            if (ovrpTracking_CreateInputTrackingContext(out var context))
            {
                return context;
            }

            return null;
        }

        private static CAPI.ovrAvatar2InputTrackingContextNative? CreateInputTrackingContextNative()
        {
            if (ovrpTracking_CreateInputTrackingContextNative(out var context))
            {
                return context;
            }

            return null;
        }

        public static OvrAvatarInputTrackingProviderBase CreateInputTrackingProvider()
        {
            var context = CreateInputTrackingContext();
            var nativeContext = CreateInputTrackingContextNative();
            return context.HasValue && nativeContext.HasValue
                ? new OvrPluginInputTrackingProvider(context.Value, nativeContext.Value)
                : null;
        }

        private static CAPI.ovrAvatar2HandTrackingDataContext? CreateHandTrackingContext()
        {
            if (ovrpTracking_CreateHandTrackingContext(out var context))
            {
                return context;
            }

            return null;
        }

        private static CAPI.ovrAvatar2HandTrackingDataContextNative? CreateHandTrackingContextNative()
        {
            if (ovrpTracking_CreateHandTrackingContextNative(out var context))
            {
                return context;
            }

            return null;
        }

        public static IOvrAvatarHandTrackingDelegate CreateHandTrackingDelegate()
        {
            var context = CreateHandTrackingContext();
            var native = CreateHandTrackingContextNative();
            return context.HasValue && native.HasValue ? new HandTrackingDelegate(context.Value, native.Value) : null;
        }

        public static OvrAvatarHandTrackingPoseProviderBase CreateHandTrackingProvider()
        {
            var context = CreateHandTrackingContext();
            var nativeContext = CreateHandTrackingContextNative();
            return context.HasValue && nativeContext.HasValue ? new OvrPluginHandTrackingProvider(context.Value, nativeContext.Value) : null;
        }

        public static OvrAvatarFacePoseProviderBase CreateFaceTrackingContext()
        {
            var context = CreateInternalFaceTrackingContext();
            var nativeContext = CreateInternalFaceTrackingContextNative();
            return context.HasValue && nativeContext.HasValue ? new OvrPluginFaceTrackingProvider(context.Value, nativeContext.Value) : null;
        }

        public static OvrAvatarEyePoseProviderBase CreateEyeTrackingContext()
        {
            var context = CreateInternalEyeTrackingContext();
            var nativeContext = CreateInternalEyeTrackingContextNative();
            return context.HasValue && nativeContext.HasValue ? new OvrPluginEyeTrackingProvider(context.Value, nativeContext.Value) : null;
        }

        private class HandTrackingDelegate : IOvrAvatarHandTrackingDelegate, IOvrAvatarNativeHandDelegate
        {
            private CAPI.ovrAvatar2HandTrackingDataContext _context;
            public CAPI.ovrAvatar2HandTrackingDataContextNative NativeContext { get; }

            public HandTrackingDelegate(CAPI.ovrAvatar2HandTrackingDataContext context, CAPI.ovrAvatar2HandTrackingDataContextNative native)
            {
                _context = context;
                NativeContext = native;
            }

            public bool GetHandData(OvrAvatarTrackingHandsState handData)
            {
                if (_context.handTrackingCallback(out var native, _context.context))
                {
                    handData.FromNative(ref native);
                    return true;
                }

                return false;
            }
        }

        private sealed class OvrPluginInputTrackingProvider : OvrAvatarInputTrackingProviderBase, IOvrAvatarNativeInputDelegate
        {
            private CAPI.ovrAvatar2InputTrackingContext _context;
            public CAPI.ovrAvatar2InputTrackingContextNative NativeContext { get; }

            public OvrPluginInputTrackingProvider(CAPI.ovrAvatar2InputTrackingContext context, CAPI.ovrAvatar2InputTrackingContextNative native)
            {
                _context = context;
                NativeContext = native;
            }

            public override bool GetInputTrackingState(out OvrAvatarInputTrackingState inputTrackingState)
            {
                inputTrackingState = default;
                if (_context.inputTrackingCallback(out var native, _context.context))
                {
                    inputTrackingState.FromNative(ref native);
                    return true;
                }

                return false;
            }
        }

        private sealed class OvrPluginHandTrackingProvider : OvrAvatarHandTrackingPoseProviderBase, IOvrAvatarNativeHandDelegate
        {
            private CAPI.ovrAvatar2HandTrackingDataContext _context;
            public CAPI.ovrAvatar2HandTrackingDataContextNative NativeContext { get; }

            public OvrPluginHandTrackingProvider(CAPI.ovrAvatar2HandTrackingDataContext context, CAPI.ovrAvatar2HandTrackingDataContextNative native)
            {
                _context = context;
                NativeContext = native;
            }

            public override bool GetHandData(OvrAvatarTrackingHandsState handData)
            {
                if (_context.handTrackingCallback(out var native, _context.context))
                {
                    handData.FromNative(ref native);
                    return true;
                }

                return false;
            }
        }


        private sealed class OvrPluginFaceTrackingProvider : OvrAvatarFacePoseProviderBase, IOvrAvatarNativeFacePose
        {
            private readonly CAPI.ovrAvatar2FacePoseProvider _context;
            private readonly CAPI.ovrAvatar2FacePoseProviderNative _nativeContext;

            CAPI.ovrAvatar2FacePoseProviderNative IOvrAvatarNativeFacePose.NativeProvider => _nativeContext;

            public OvrPluginFaceTrackingProvider(CAPI.ovrAvatar2FacePoseProvider context, CAPI.ovrAvatar2FacePoseProviderNative nativeContext)
            {
                _context = context;
                _nativeContext = nativeContext;
            }

            protected override bool GetFacePose(OvrAvatarFacePose faceState)
            {
                if (_context.facePoseCallback != null &&
                    _context.facePoseCallback(out var nativeFaceState, _context.provider))
                {
                    faceState.FromNative(in nativeFaceState);
                    return true;
                }
                return false;
            }
        }

        private sealed class OvrPluginEyeTrackingProvider : OvrAvatarEyePoseProviderBase, IOvrAvatarNativeEyePose
        {
            private readonly CAPI.ovrAvatar2EyePoseProvider _context;
            private readonly CAPI.ovrAvatar2EyePoseProviderNative _nativeContext;

            CAPI.ovrAvatar2EyePoseProviderNative IOvrAvatarNativeEyePose.NativeProvider => _nativeContext;

            public OvrPluginEyeTrackingProvider(CAPI.ovrAvatar2EyePoseProvider context, CAPI.ovrAvatar2EyePoseProviderNative nativeContext)
            {
                _context = context;
                _nativeContext = nativeContext;
            }

            protected override bool GetEyePose(OvrAvatarEyesPose eyeState)
            {
                if (_context.eyePoseCallback != null &&
                    _context.eyePoseCallback(out var nativeEyeState, _context.provider))
                {
                    eyeState.FromNative(in nativeEyeState);
                    return true;
                }
                return false;
            }
        }
#endif
    }
}
