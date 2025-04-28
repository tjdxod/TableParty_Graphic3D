using System.Collections.Generic;

namespace Dive.VRModule
{
    /// <summary>
    /// 사용되는 레이어 정리
    /// </summary>
    public static class PXRLayer
    {
        #region Public Fields

        /// <summary>
        /// 레이어 인덱스와 이름
        /// </summary>
        public static readonly Dictionary<int, string> Layers = new Dictionary<int, string>()
        {
            {6, IgnorePointerCollider}, //
            {7, AvatarBody}, //
            {9, Mask}, //
            {15, IgnoreMirror}, // 아바타 커스터마이징 거울
            {16, IgnoreAvatar}, // 아바타 커스터마이징 거울
            {17, PhotoAvatar}, // 사진 아바타
            {18, PCBody}, // 
            {19, Floor}, //
            {22, TeleportSpace}, //
            {24, Interactable}, // 상호작용 관련 레이어

            {26, Directable}, // 3D 버튼
            {27, AvatarArea}, // 아바타 영역
            {28, AvatarItem}, // 아바타 관절 콜라이더

        };

        #endregion

        #region Private Fields

        private const string IgnorePointerCollider = "Ignore Pointer Collider";
        private const string AvatarBody = "AvatarBody";
        private const string Mask = "Mask";

        private const string IgnoreMirror = "Ignore Mirror";
        private const string IgnoreAvatar = "Ignore Avatar";
        private const string PhotoAvatar = "Photo Avatar";

        private const string PCBody = "PCBody";
        private const string Floor = "Floor";
        
        private const string TeleportSpace = "TeleportSpace";
        
        private const string Interactable = "Interactable";
        private const string Directable = "Directable";
        
        private const string AvatarArea = "AvatarArea";
        private const string AvatarItem = "AvatarItem";

        #endregion
    }
}