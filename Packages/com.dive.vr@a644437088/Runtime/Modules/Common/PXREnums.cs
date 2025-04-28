using System;

namespace Dive.VRModule
{
    public enum SupportedPlatform
    {
        None = -1,
        Meta = 1,
        Pico = 2,
        Qiyu = 3,
        Steam = 4,
        PlayStation = 5
    }
    
    /// <summary>
    /// Character Controller 상태 enum
    /// </summary>
    public enum CharacterState
    {
        None,
        Default,
        Movement,
        Teleport
    }
    
    /// <summary>
    /// 손 방향 enum
    /// </summary>
    public enum HandSide { Left, Right, Unknown }
    
    /// <summary>
    /// 컨트롤러 버튼 enum
    /// </summary>
    public enum Buttons { Trigger, Grip, PrimaryAxis, Primary, SecondaryAxis, Secondary, Menu, LeftMouse, RightMouse, Default }
    
    /// <summary>
    /// Interact 상호작용 종류
    /// </summary>
    public enum InteractMode {None, Finger, Palm}
    
    public enum DirectableState { Enter, Stay, Exit }
    
    /// <summary>
    /// 손가락 enum
    /// </summary>
    public enum FingerType { None, Thumb, Index, Middle, Ring, Pinky, All }

    /// <summary>
    /// 손바닥 enum
    /// </summary>
    public enum PalmType { None, Palm, BackOfHand }
    
    /// <summary>
    /// Directable 상호작용 종류
    /// </summary>
    public enum DirectableType { None, CubicButton, Lever, Pump, Socket, Valve, Attachable, Reel, DummyButton }
    
    /// <summary>
    /// Axis 컨트롤러 enum
    /// </summary>
    public enum ControllerAxis { Primary, Secondary }
    
    /// <summary>
    /// 컨트롤러 Interact 상태 enum
    /// </summary>
    public enum HandInteractState { None, Teleporting, Grabbing, Clicking }
    
    /// <summary>
    /// 
    /// </summary>
    public enum TransferState { None, One, Both }
    
    /// <summary>
    /// 컨트롤러 Grab 상태 enum
    /// </summary>
    public enum GrabState { None, Collider, Distance }
    
    public enum ClickState {None, Click}
    
    public enum EnableGrabState {None, Collider, Distance, Both}
    
    public enum AttachGrabbableState { None, Attached, AttachedWithPose}
    
    public enum OverrideTransformState { None, Position, Rotation, Both}
    
    public enum OverrideParentState { Origin, Null, Other }
    
    /// <summary>
    /// 컨트롤러 라인렌더러 활성화 상태
    /// </summary>
    public enum LineRendererState { Always, OnHover, None }
    
    /// <summary>
    /// Offset 변경을 지원하는 Device 종류
    /// </summary>
    public enum SupportOffsetDevice
    {
        Unknown,
        Quest2,
        QuestPro,
        Quest3,
        MixedReality,
        Vive,
        Index,
        Pico,
        OpenXR
    }

    public enum OpenXRRuntimeType
    {
        None,
        SteamVR,
        Oculus,
        Other
    }
    
    public enum LeverType
    {
        None,
        TwoDirection, // On, Off
        FourDirection // 인 게임 패드?
    }
    
    public enum LeverState
    {
        On,
        Off
    }
}
