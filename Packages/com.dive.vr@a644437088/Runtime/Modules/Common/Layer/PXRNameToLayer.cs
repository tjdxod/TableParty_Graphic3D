using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// VRModule에서 사용되는 레이어의 int값 클래스
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PXRNameToLayer
    {
        #region Public Fields

        public static readonly int DefaultLayer = LayerMask.NameToLayer("Default");
        public static readonly int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int Water = LayerMask.NameToLayer("Water");
        public static readonly int UILayer = LayerMask.NameToLayer("UI");
        public static readonly int Floor = LayerMask.NameToLayer("Floor");

        public static readonly int IgnorePointerCollider = LayerMask.NameToLayer("Ignore Pointer Collider");
        public static readonly int AvatarBody = LayerMask.NameToLayer("AvatarBody");

        public static readonly int IgnoreMirror = LayerMask.NameToLayer("Ignore Mirror");
        public static readonly int IgnoreAvatar = LayerMask.NameToLayer("Ignore Avatar");
        public static readonly int PhotoAvatar = LayerMask.NameToLayer("Photo Avatar");

        public static readonly int TeleportSpace = LayerMask.NameToLayer("TeleportSpace");
        public static readonly int Interactable = LayerMask.NameToLayer("Interactable");
        public static readonly int Directable = LayerMask.NameToLayer("Directable");

        public static readonly int AvatarArea = LayerMask.NameToLayer("AvatarArea");
        public static readonly int AvatarItem = LayerMask.NameToLayer("AvatarItem");
        
        #endregion
    }
}