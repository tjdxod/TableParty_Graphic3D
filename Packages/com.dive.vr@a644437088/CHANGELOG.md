# Changelog

## [2.7.36] - 2025.04.21
- 머테리얼 및 텍스쳐 GUID 수정

## [2.7.35] - 2025.04.08
- `PXRGrabber`클래스의 `FindNearestGrabbable` 함수 조건 및 매개 변수 수정

## [2.7.34] - 2025.04.08
- `PXRInteractableBaseOutline` 클래스에 `Only Color` 기능 추가

## [2.7.33] - 2025.04.08
- `Lux URP Fast Outline Double Pass` 셰이더 `MultiPass `예외 처리 추가

## [2.7.32] - 2025.04.08
- `LuxOutline` 클래스 `Odin Inspector` 에셋 `Define Symbol` 조건 추가

## [2.7.31] - 2025.04.08
- `LuxOutline` 클래스 스케일 모드 추가

## [2.7.30] - 2025.04.07
- `PXREyeHeightChanger` 클래스 높이 제한 변수 추가

## [2.7.29] - 2025.04.04
- 높이 조절 기능 롤백

## [2.7.28] - 2025.04.03
- `PXRRig_KCC` 프리펩에 손목 모델 추가가
- `PXRHandAnimationController`클래스에 손목 모델 렌더러를 관리하는 `HandSleeveRenderer` 변수 할당
- `PXRHandAnimationController`클래스의 손목 모델의 메쉬를 관리하는 `HandSleeveMeshFilter` 변수 할당

## [2.7.27] - 2025.04.03
- `PXRRig_KCC` 프리펩에 손목 모델 추가가
- `PXRHandAnimationController`클래스에 손목 모델 렌더러를 관리하는 `HandSleeveRenderer` 변수 추가
- `PXRHandAnimationController`클래스의 손목 모델의 메쉬를 관리하는 `HandSleeveMeshFilter` 변수 추가
- `PXRRig` 클래스에 모든 손목 모델의 레이어를 변경하는 `SetLayerAllHandSleeve` 함수 추가
- `PXRRig` 클래스에 특정 손목 모델의 레이어를 변경하는 `SetLayerHandSleeve` 함수 추가
- `PXRRig` 클래스에 모든 손 모델의 Material을 변경하는 `SetHandRenderer` 함수 추가
- `PXRRig` 클래스에 모든 손목 모델의 Material을 변경하는 `SetHandSleeveRenderer` 함수 추가

## [2.7.26] - 2025.04.01
- `PXRLocomotionController`클래스의 `MoveToFixedDestination` 함수 수정

## [2.7.25] - 2025.03.31
- `PXRTeleportSpaceBase` 클래스에 미사용 기능 주석
- `PXREyeHeightChanger` 클래스 미사용을 위해 `IsEnableAB` 변수 수정

## [2.7.24] - 2025.03.29
- `PXRPlayerController` 클래스의 `ForceChangeEyeHeight` 함수 오버로딩 추가

## [2.7.23] - 2025.03.05
- `PXRBaseController` 클래스의 `UpdateTrackingInput` 함수에 플랫폼 별 `Define Symbol` 조건 추가

## [2.7.22] - 2025.03.04
- `PXRBaseController` 클래스에 `ApplyControllerState` 함수 override 추가
- `PXRBaseController` 클래스의 `UpdateTrackingInput` 함수에 `inputTrackingState` 조건 수정

## [2.7.21] - 2025.02.27
- `PXRSnapTurn` 클래스에 잡은 상태에서 회전이 가능한지 관리하는 `canGrabTurn` 변수 추가

## [2.7.20] - 2025.02.05
- `PXRGrabber` 클래스의 `FindNearestGrabbable` 함수에 `TransferState.One` 조건 추가

## [2.7.19] - 2025.02.03
- `PXRPullaableLever` 클래스의 `ReturnToOrigin` 함수에 `useEvent` 매개변수 추가
- `PXRPullaableLever` 클래스에 `ratio`에 따른 위치 값 적용 수정

## [2.7.18] - 2025.01.23
- `PXRRig` 클래스의 `RenderHand` 함수를 `SetLayerAllHand`로 변경
- `PXRRig` 클래스에 특정 손 모델의 레이어를 변경하는 `SetLayerHand` 함수 추가

## [2.7.17] - 2025.01.23
- `PXRRig` 클래스를 `partial` 로 수정
- `PXRRig` 클래스에 손 모델 렌더러 활성화를 관리하는 `RenderHand`함수 추가

## [2.7.16] - 2025.01.22
- `PXRPullaableLever` 클래스에 불필요한 using문 제거

## [2.7.15] - 2025.01.22
- `PXRPullaableLever` 클래스에 `ForceStateChange` 함수 추가 및 기능 수정
- `PXRLeverGrabbable` 클래스의 `OnGrabbedEvent` 함수 수정
- `PXREnum`에 레버 기능을 위한 enum 추가

## [2.7.14] - 2025.01.14
- `PXRGrabbable` 클래스의 `PXRPose`와 `HandPose`를 강제로 변경하는 `ForceSetPXRPose`, `ForceSetHandPose` 함수 추가

## [2.7.13] - 2025.01.06
- `PXRRigMovementBase` 클래스의 `ResetPosition`,`ResetRotation` 변수 `abstract` 키워드 제거
- `PXRRigMovementBase` 클래스의 `Update` 함수 수정

## [2.7.12] - 2024.12.30
- `OpenXRRuntimeType` 열거형 수정
- `PXRBaseController` 클래스의 `OnDeviceConnected` 함수 수정

## [2.7.11] - 2024.12.30
- `OpenXRRuntimeType` 열거형 추가
- `PXRBaseController` 클래스의 `OnDeviceConnected` 함수에 `Define Symbol` 조건 추가

## [2.7.10] - 2024.12.30
- `SupportOffsetDevice` 열거형 수정
- `PXRBaseController` 클래스의 `OnDeviceConnected` 함수 수정

## [2.7.9] - 2024.12.26
- `PXRBaseControllerScriptableObject` 클래스의 프리펩 추가

## [2.7.8] - 2024.12.26
- `PXRBaseControllerScriptableObject` 클래스의 `positionOffset`, `rotationOffset` 변수 추가

## [2.7.7] - 2024.12.16
- `PXRGrabber` 클래스에 `DeletePoser` 함수 추가

## [2.7.6] - 2024.12.16
- `PXRGrabber` 클래스의 `Update` 함수에 `Define Symbol` 조건 추가

## [2.7.5] - 2024.12.16
- `PXRRig` 클래스의 `isHandTrackingMode` 변수 추가

## [2.7.4] - 2024.12.16
- `PXRGrabber` 클래스의 pxrPosers 변수 수정

## [2.7.3] - 2024.12.09
- `PXRPressableButton` 클래스의 디버그 코드 제거

## [2.7.2] - 2024.12.09
- `TurnableValve` 기능 폴더 위치 변경

## [2.7.1] - 2024.12.09
- `IDirectable` 인터페이스에 `ForcePress` 함수 추가
- `IDirectable` 인터페이스의 `ForceRelease` 함수에 이벤트 실행 유무를 나타내는 `useEvent` 변수 추가

## [2.7.0] - 2024.12.03
- `PXRMetaAvatarEntityBase` 클래스에 `isPhotoAvatar` 변수 추가
- `PXRLayer` 클래스에 `PhotoAvatar` 레이어 추가
- `PXRNameToLayer` 클래스에 `PhotoAvatar` 레이어 추가

## [2.6.13] - 2024.10.29
- `PXRInputModule` 드래그 조작 `Pico` 플랫폼용 `Define Symbol` 조건 추가

## [2.6.12] - 2024.10.28
- `PXRInputModule` 클래스 드래그 UI 패드조작 추가

## [2.6.11] - 2024.10.18
- `PXRInputModuleCanvasAdder` 클래스에 예외처리 추가

## [2.6.10] - 2024.09.10
- `PXRBaseController` 클래스에 `Steam` 플랫폼에서 기기별 `positionOffset`, `rotationOffset` 변경

## [2.6.9] - 2024.09.03
- `Native` 아바타 셰이더 롤백

## [2.6.8] - 2024.09.03
- `Native` 아바타 셰이더 변경

## [2.6.7] - 2024.09.02
- `PXRLayer` 클래스에 `AvatarArea` 변수 추가
- `PXRNameToLayer` 클래스에 `AvatarArea` 변수 추가

## [2.6.6] - 2024.08.29
- `PXRInputBridge` 클래스에 `IsTrackingHMD` 변수 추가

## [2.6.5] - 2024.08.26
- `PXRGrabbable` 클래스의 `throwForce` 변수에 `Define Symbol` 조건 추가

## [2.6.4] - 2024.08.21
- `Vive` 디바이스를 위한 InputSystem 변경

## [2.6.3] - 2024.08.19
- `PXRRecenter` 클래스 롤백 및 `PXR InputSystem`에 `VIVE`용 예외처리 추가 

## [2.6.2] - 2024.08.19
- `PXRRecenter` 클래스에 이벤트 추가

## [2.6.1] - 2024.08.19
- `PXRRecenter` 클래스에 `Define Symbol` 조건 추가

## [2.6.0] - 2024.08.19
- `PXRRecenter` 클래스 추가

## [2.5.6] - 2024.08.16
- `Vive` 디바이스를 위한 InputSystem 변경

## [2.5.5] - 2024.07.23
- `PXRPlayerController` 클래스에 현재 상태에 대한 플래그 변수 추가  
  ┣ `IsBothSnapTurnActive` : 양손 스냅턴 회전 활성화 여부  
  ┣ `IsBothPointerActive` : 양손 포인터 활성화 여부  
  ┣ `IsBothTeleporterActive` : 양손 텔레포트 활성화 여부  
  ┣ `IsBothGrabberActive` : 양손 그랩 활성화 여부  
  ┣ `IsHeightChangeActive` : 높이 조절 활성화 여부  
  ┗ `IsMovementActive` : 이동 활성화 여부
- `PXRTeleporter` 클래스의 `ForceTeleport` 함수가 텔레포트 이벤트를 수행하며 이동하도록 수정


## [2.5.4] - 2024.07.04
- `PXRPlayerController` 클래스의 `standardHeight` 변수를 변경하는 `SetStandardHeight` 함수 추가
- `PXREyeHeightChanger` 클래스의 `standardHeight` 변수를 초기화하는 `ResetStandardHeight` 함수 추가
- 

## [2.5.3] - 2024.05.16
- `PXRGrabbable` 클래스의 `OverrideLocalPosition`, `OverrideLocalRotation` 프로퍼티 `define symbol` 조건 수정
- `PXRTeleportSpaceBase` 클래스의 `ActiveSpace` 함수 조건 추가

## [2.5.2] - 2024.05.14
- `PXRGrabbable` 클래스의 `OverrideLocalPosition`, `OverrideLocalRotation` 프로퍼티 `define symbol` 조건 수정

## [2.5.1] - 2024.05.10
- Teleport Point의 MeshCollider를 이용하기 위한 FBX 모델 추가

## [2.5.0] - 2024.05.10
- 패키지 displayName 변경 (`VRModule` -> `Dive VR SDK`)
- 패키지 name 변경 (`com.dive.vrmodule` -> `com.dive.vr`)
- 패키지 description 변경
- `PXRNameToLayer`클래스에 `Floor` 변수 추가

## [2.4.4] - 2024.05.02
- `PXRGrabber` 클래스의 `CheckColliderGrab` 함수 조건 추가

## [2.4.3] - 2024.05.01
- `PXRCliclable` 클래스에 `OnDisable` 함수 추가 및 `ForceRelease` 실행

## [2.4.2] - 2024.04.26
- 이동 중에 `CharacterState`가 변경 된 경우 계속 이동되는 현상 수정

## [2.4.1] - 2024.04.26
- 포인터 활성화를 UI-버튼에만 적용에서 UI 전체 적용으로 변경

## [2.4.0] - 2024.04.25
- 버전 업데이트

## [2.3.31] - 2024.04.25
- Rig 이동 시 Pointer의 LineRenderer 렌더 수정

## [2.3.30] - 2024.04.25
- `PXRRigMovementBase` 클래스의 `ResetRotation` 변수명 및 자료형 변경

## [2.3.29] - 2024.04.25
- `PXRRigMovementBase` 클래스에 abstract `ResetPosition`, `ResetRotation` 프로퍼티 추가
- `PXRRigMovementBase` 클래스에 `SetResetPosition`, `SetResetRotation` 함수 추가

## [2.3.28] - 2024.04.24
- `PXRRigMovementBase` 클래스의 `SetInputs` 함수 조건 추가

## [2.3.27] - 2024.04.24
- `PXRRigMovementBase` 클래스의 `SetInputs` 함수 조건 변경

## [2.3.26] - 2024.04.24
- `PXRPlayerController` 클래스에 이동 시 호출되는 `AfterMovementEvent` 이벤트 추가

## [2.3.25] - 2024.04.22
- `PXRGrabbable` 클래스에 `preFramePosition`, `perFrameRotation` 변수 갱신

## [2.3.24] - 2024.04.22
- `PXRGrabbable` 클래스에 `PreFramePosition`, `PreFrameRotation` 프로퍼티 롤백
- `PXRGrabbable` 클래스에 `Released`가 되고난 후 일정 `velocity`보다 값이 작아진 경우 `CurrentVelocity`가 초기화 되도록 수정

## [2.3.23] - 2024.04.22
- `PXRGrabbable` 클래스에 `PreFramePosition`, `PreFrameRotation` 프로퍼티 추가
- `PXRGrabbable` 클래스에 `GetDistanceAtFrame`, `GetAngleAtFrame`, `GetDirectionAtFrame` 함수 추가

## [2.3.22] - 2024.04.22
- `PXRNameToLayer` 클래스에 `AvatarItem` 변수 추가

## [2.3.21] - 2024.04.22
- `PXREyeHeightChanger` 클래스의 `EyeMoveTime` 프로퍼티를 `SerializeField` 속성으로 변경

## [2.3.20] - 2024.04.19
- `PXRPlayerController` 클래스에 `StandardHeight` 프로퍼티 추가

## [2.3.19] - 2024.04.17
- `PXRPlayerController` 클래스의 `ForceChangeEyeHeight` 함수 `Define Symbol` 조건 제거

## [2.3.18] - 2024.04.15
- `PXRTeleportRay` 클래스의 `ProjectForward`, `ProjectDown` 함수 수정

## [2.3.17] - 2024.04.09
- `PXRClickerController` 클래스에 `IsClicking` 프로퍼티 추가
- `PXRRigMovementMeta` 클래스의 이름을 `PXRRigMovementCommon`로 변경
- `PXRRigMovementCommon` 클래스의 `HandleCharacterXRInput` 함수 조건 추가

## [2.3.16] - 2024.04.09
- `PXRInputBridge` 클래스에 진동 세기를 조절하는 함수 추가

## [2.3.15] - 2024.04.08
- `PXRTeleportSpaceBase` 클래스의 `InactiveSpace` 함수 조건 추가
- `PXRInteractableBaseEvent` 클래스의 이벤트명 변경

## [2.3.14] - 2024.04.05
- `PXRGrabbable` 클래스에 각 플랫폼별 잡는 위치 및 방향을 변경하는 `Define Symbol` 조건 추가

## [2.3.13] - 2024.04.05
- `PXRMovableUI` 클래스의 `localPosition`을 `position` 으로 변경

## [2.3.12] - 2024.04.02
- `PXRPointerBase` 클래스의 `PointerEventData` 프로퍼티 예외처리 추가
- `PXRPoserCreator` 클래스에 `Define Symbol` 조건 추가

## [2.3.11] - 2024.04.01
- `PXRInputModule` 클래스의 드래그 이벤트 조건 변경
- `PXRPointerEventDataExtensions` 의 주석 제거

## [2.3.10] - 2024.04.01
- dependency 제거

## [2.3.9] - 2024.04.01
- 오른쪽 컨트롤러로 UI 클릭 안되던 사항 수정
- `PXRMovableUI` 클래스의 빌보드 기능 수정
- `PXRInteractableBase` 클래스의 `PointerDown` 이벤트 수정
- Unity 내장 `PointerEventData` 클래스의 확장 메소드 추가

## [2.3.8] - 2024.03.29
- `PXRMovableUI` 클래스의 `OnDestroy`에 예외처리 추가

## [2.3.7] - 2024.03.28
- `PXRPoser` 클래스에 조인트에 접근할 수 있는 프로퍼티 추가

## [2.3.6] - 2024.03.28
- `PXRGrabber` 클래스의 `ForcePose` 기능 비활성화

## [2.3.5] - 2024.03.28
- `PXRPoserCreator` 클래스의 관절 갯수 추가

## [2.3.4] - 2024.03.27
- `PXRGrabber` 클래스의 `ForcePose` 함수 조건 변경

## [2.3.3] - 2024.03.27
- `PXRPoseDefinition` 클래스 `Serializable` 추가

## [2.3.2] - 2024.03.27
- `PXRPoser` 클래스의 변수의 접근제한자 변경

## [2.3.1] - 2024.03.27
- `HandPose`에 불필요한 코드 제거

## [2.3.0] - 2024.03.27
- `Pico`, `Meta` 플랫폼을 위한 `HandPose`용 클래스 추가

## [2.2.5] - 2024.03.25
- `PXRBaseController` 클래스에 `Transform` 프로퍼티 추가
- `PXRMovableUI` 클래스가 잡힌채 제거된 경우 `PXRClicker` 클래스가 강제로 `Release` 되도록 수정

## [2.2.4] - 2024.03.21
- `PXRMovableStaticUI` 클래스 기능을 사용하는 경우 고정된 방향을 바라보도록 수정

## [2.2.3] - 2024.03.21
- `PXRMovableStaticUI` 클래스의 변수 및 `OnValidate` 함수 수정
- `PXRMovableUI` 클래스의 변수 및 `OnValidate` 함수 수정

## [2.2.2] - 2024.03.21
- `PXRMovableUI` 클래스에 `Define Symbol` 조건 추가
- 불필요한 using 문 제거

## [2.2.1] - 2024.03.21
- `PXRClickable` 클래스 롤백 및 기능 수정

## [2.2.0] - 2024.03.21
- `PXRClickable` 클래스 제거
- `PXRClicker` 클래스 및 프리펩 추가
- 트리거를 이용하여 움직이는게 가능한 `PXRMovableUI` 프리렙 및 클래스 추가

## [2.1.14] - 2024.03.18
- `AutoPoser` 기능 최적화

## [2.1.13] - 2024.03.18
- `PXRTeleportSpaceBase` 클래스의 `OnEnterPlayer`와 `OnExitPlayer`함수의 호출 순서를 변경
- `PXRTeleportPoint` 클래스의 `OnEnterPlayer`, `OnExitPlayer` 함수 조건 추가

## [2.1.12] - 2024.03.18
- `PXRRig` 클래스에 `RigMovement` 변수 추가
- `PXRTeleporter` 클래스의 이벤트 실행 복구

## [2.1.11] - 2-24.03.18
- `PXRRig` 클래스에 현재 입장한 텔레포트 영역을 반환하는 `OwnerArea` 변수 추가
- `PXRPlayerController` 클래스에 `PXRRig`의 `OwnerArea`를 변경할 수 있는 `SetOwnerPoint` 함수 추가
- `PXRTeleporter` 클래스의 텔레포트 포인터 조건 변경

## [2.1.10] - 2024.03.15
- `PXRRigMovementBase` 클래스의 `Debug.DrawRay` 디버그 제거
- `PXRTeleporter` 클래스의 `maxTeleportRayLength` 변수 기본값 변경
- `PXRTeleporter` 클래스에 파티클 활성화 조건 추가
- `PXRTeleportMarker` 클래스에 예외처리 추가

## [2.1.9] - 2024.03.15
- `PXRRig` 클래스의 `UseMovement` 프로퍼티 제거 및 프로퍼티 제거에 따른 관련 클래스들 수정
- `PXRPlayerController` 클래스에 `UseMovement` 프로퍼티 추가
- `PXRPlayerController` 클래스에 `UseMovement`의 활성화를 관리하는 함수 추가

## [2.1.8] - 2024.03.14
- `HandPoser` 클래스의 `UpdateJoint`의 Quaternion Lerp의 t값을 Clamp로 값 제한

## [2.1.7] - 2024.03.13
- `PXRRigMovementBase` 클래스의 `Awake` 함수 `virtual`로 변경
- `PXRRigMovementBaseEvent` 클래스 조건 추가

## [2.1.6] - 2024.03.11
- `PXRPressableButton` 클래스의 변수 접근 제한자 변경

## [2.1.5] - 2024.03.06
- `[2.1.4]` 버전 롤백 및 조건 추가

## [2.1.4] - 2024.03.05
- `PXRTeleportPoint` 클래스의 `Coroutine`을 `Task`로 변경

## [2.1.3] - 2024.02.27
- `PXRTeleportSpaceBase` 클래스의 `IsEnteredPlayer` 변수 조건 추가

## [2.1.2] - 2024.02.27
- `PXRTeleporterRay` 클래스의 `ExecuteRay` 함수 수정 및 조건 추가
- `PXRTeleportSpaceBase` 클래스의 `spaceCollider` 변수의 타입 변경 (`MeshCollider` -> `Collider`)

## [2.1.1] - 2024.02.27
- `PXRTeleportSpaceBase` 클래스의 `IsEnteredPlayer` 접근 제한자 변경

## [2.1.0] - 2024.02.27
- 텔레포트 기능 이벤트 변경

## [2.0.33] - 2024.02.22
- 이미 잡고 있는 `Grabbable`를 반대쪽 손으로 `Grabbable`를 잡은 경우 발생하는 이벤트 `ChangeGrabbedHandEvent` 추가

## [2.0.32] - 2024.02.20
- `PXREyeHeightController` 클래스의 `ForceChangeEyeHeight` 함수 `Define Symbol` 조건 수정

## [2.0.31] - 2024.02.19
- `[2.0.30]` 버전 롤백

## [2.0.30] - 2024.02.19
- `PXRMetaAvatarControlDelegate` 클래스의 `UpdateControllerInput`함수 조건 추가

## [2.0.29] - 2024.02.19
- `PXRRig` 클래스에 Kinematic Character Controller 변수 추가
- `PXRLocomotionController` 클래스의 `MoveToFixedDestination`함수 수정
- `PXREyeHeightController` 클래스에 `Define Symbol` 조건 추가

## [2.0.28] - 2024.02.19
- `PXR Rig` 클래스에 `Define Symbol` 제거
- `PXRGrabbable` 클래스의 변수 주석 변경

## [2.0.27] - 2024.02.14
- `PXR Rig` 클래스에 `Define Symbol` 조건 변경

## [2.0.26] - 2024.02.14
- `PXR Rig` 클래스에 `Meta Avatar`를 위한 변수 및 `Define Symbol` 조건 추가

## [2.0.25] - 2024.02.14
- `QuickSlot` 기능 모두 제거

## [2.0.24] - 2024.02.07
- `PXRScreenFade` 클래스에 변수를 초기화하는 `ResetFadeValues` 함수 추가

## [2.0.23] - 2024.02.07
- `PXRInteractableBase` 클래스의 `GetCanInteractColor`, `GetCanNotInteractColor` 예외처리 추가

## [2.0.22] - 2024.01.31
- `PXRInputBridge` 클래스의 `GetXRController` 함수에 예외처리 추가

## [2.0.21] - 2024.01.30
- `PXRInputBridge` 클래스의 `GetXRController` 함수 변경

## [2.0.20] - 2024.01.29
- `PXRGrabbable` 클래스의 `ChangeLocalPosition`에 조건 추가
- `PXRGrabber` 클래스의 `IsNeedChangeTweezerPosition` 프로퍼티 추가 및 `GetGrabberPosition`, `GetGrabberTweezerPosition` 함수 public로 변경

## [2.0.19] - 2024.01.26
- `PXRGrabber` 클래스에 `Define Symbol` 조건 추가

## [2.0.18] - 2024.01.26
- `PXREyeHeightChanger` 클래스의 `IsEnableAB` 조건 변경
- `PXREyeHeightChanger` 클래스에 즉시 특정 위치로 이동하는 `ChangeEyeTargetHeight` 함수 추가
- `PXRGrabber` 클래스의 위치를 바꾸고 초기화하는 기능 추가

## [2.0.17] - 2024.01.25
- `Kinematic Character Controller`에 빌드를 위한 `UNITY_EDITOR Define Symbol` 추가

## [2.0.16] - 2024.01.19
- `PXRRig` 클래스에 패드 이동 가능 유무를 설정할수 있는 `useMovement` 변수 추가 (접근 시에는 `PXRRig.UseMovement`로 접근)
- `Kinematic Character Controller`의 상태를 나타내는 `CharacterState` enum 추가
- `PXRBaseController` 클래스에 `positionOffset`와 `rotationOffset` 변수의 `ReadOnly Attribute` 제거
- `PXRGrabbable` 클래스의 `OverrideTransformState` 를 비교하는 조건문에 조건 추가
- `PXRGrabbable` 클래스의 `IsMoveGrabbable` 프로퍼티의 조건 변경
- 자연스러운 캐릭터 컨트롤을 위한 `Kinematic Character Controller` ThirdParty 추가
- 패드 이동을 위한 `PXRRigMovementBase` 클래스 추가  
  ┗ `PXRRig_KCC` 프리펩 추가
- Oculus 디바이스 용 `PXRRigMovementOculus` 클래스 추가  
  ┗ `PXRRigMovementBase` 클래스 상속
- 패드 이동 지원 시 텔레포트 및 회전이 제한되도록 `PXRSnapTurn`, `PXRTeleporter` 클래스에 조건 추가
- `PXRQuickSlot` 클래스에 조건 추가
- `Billboard` 클래스에 조건 추가

## [2.0.15] - 2024.01.12
- `PXRInteractAddona` 클래스의 `intensifyInteracts` 변수 타입 변경 (`IIntensifyInteract[]` -> `PXRIntensifyInteractBase[]`)
- `PXRGrabbable` 클래스의 `[RequireComponent(typeof(Rigidbody))]` 제거  
  ┗ 필요없는 상황이 존재하므로, 무조건적으로 넣는 방식에서 변경
- 패키지명 변경 (`com.plugvr.vrmodule` -> `com.dive.vrmodule`)

## [2.0.14] - 2024.01.11
- `PXRAdditionalPressableButton` 클래스에 `Odin Inspector`로 대체되도록 위한 `Define Symbol` 추가
- `PXRAdditionalPressableButton` 클래스 변수에 `LabelText`로 설명 추가

## [2.0.13] - 2024.01.11
- 작은 버튼을 위한 `AdditionalPressableButton` 클래스 추가
- 작은 버튼을 위한 `IAdditionalPressableButton` 인터페이스 추가  
   
- `IDirectable` 인터페이스에 `ForceRelease` 함수 추가  
   
- `PXRDirectableBase` 클래스를 `abstract`로 변경
- `PXRDirectableBase`에 클래스에 `abstract` 함수 `ForceRelease` 추가  
   
- `PXRPressableButton` 클래스에 `ForceRelease` 함수 오버라이드
- `PXRPressableButton` 클래스에서 강제로 `Release`시에 `ratio`값이 1로 변경되도록 수정  
   
- `PXRPressableButtonChild`클래스에서 부모의 인터페이스에 접근할 수 있도록 `GetAdditionalPressableButton` 함수 추가
- `PXRPressableButtonChild`클래스에 `ForceRelease` 함수 추가  
   
- `PXRPullableLever` 클래스에 `ForceRelease` 함수 오버라이드
- `PXRPullablePunp` 클래스에 `ForceRelease` 함수 오버라이드
- `PXRTurnableValve` 클래스에 `ForceRelease` 함수 오버라이드  
   
- `IIntensifyInteract` 인터페이스에 `GetIntensifyInteractBase` 함수 추가
- `PXRIntensifyInteractBase` 클래스에 닿았던 `directable`를 `Release`하는 `ForceAllRelease` 함수 추가
- `PXRIntensifyInteractBase` 클래스에 `GetIntensifyInteractBase` 함수 추가  
   
- `PXRInteractAddon` 클래스의 `handSide` 변수 직렬화
- `PXRInteractAddon` 클래스에 그랩인 상태에서 닿았던 `directable`를 초기화하는 조건 추가  
   
- `PXRGrabbable` 클래스에 `isKinematic` 로그 방지를 위한 조건 추가
- `PXRGrabber` 클래스에 작은 버튼이 있는 경우 강제로 `HandPose`를 변경할 수 있는 `ForcePose` 함수 추가
- `PXRGrabber` 클래스에 주변에 작은 버튼을 감지하는 `FindNearestPressableButton` 함수 추가  
   
- `Grabber` 프리펩 수정  
   
- `Pointing`, `Gun`, `Grab` Pose 데이터 추가 및 기존 Pose 이름 변경

## [2.0.12] - 2024.01.10
- `PXRPressableButton` 클래스에 `useFixedPress`변수 및 조건 추가
- `PXRPressableButton` 클래스의 `ratio`값이 `0 ~ 1` 사이로 고정되도록 수정
- `PXRPressableButton` 클래스의 샘플 코드 제거

## [2.0.11] - 2024.01.10
- `Lux URP Essentials` 에셋의 버전 업그레이드로 인한 `Lux Outline` 클래스 변수명 변경 적용 (`_Color` -> `_BaseColor`)
- `TeleportSpace` 프리펩 `Mesh Collider` 설정 변경 (`None` -> `Convex`, `None Trigger` -> `Is Trigger`)

## [2.0.10] - 2024.01.10
- `TeleportSpace_Area`, `TeleportSpace_OnePoint`, `TeleportSpace_TogglePoint` 프리펩 추가
- `TeleportSpace`에서 고정 방향 및 위치 활성화 시 정상적으로 변경되도록 수정

## [2.0.9] - 2024.01.10
- `Fader`, `Teleporter`, `PXRRig` 프리펩 인스펙터 변수 할당

## [2.0.8] - 2024.01.09
- `Attribute`의 `Label`을 `Odin Inspector`와 이름이 같게 `LabelText`로 변경

## [2.0.7] - 2024.01.09
- `Attribute`가 사용된 `Interact` 클래스에 누락된 `Define Symbol` 추가

## [2.0.6] - 2024.01.09
- `Attribute`가 사용된 `Interact` 클래스에 `Define Symbol` 조건 추가

## [2.0.5] - 2024.01.09
- `Attribute` 관련 클래스가 `Odin Inspector`가 없는 경우에만 사용되도록 `Define Symbol` 조건 추가

## [2.0.4] - 2024.01.09
- 중복 `GUID` 파일 변경

## [2.0.3] - 2024.01.09
- `PXRInteractableBase` 클래스의 아웃라인 색상을 변경할 수 있는 함수 추가

## [2.0.2] - 2024.01.09
- `PXRTeleportSpaceBase` 클래스의 `CanVisible` 변수 값을 변경할 수 있는 함수 추가

## [2.0.1] - 2024.01.09
- ThirdParty 에셋 추가 (`Easy Buttons`)

## [2.0.0] - 2024.01.08
- `NameSpace` 변경 (`PlugVR -> Dive`)  
   
- 미사용 레이어 제거 및 용도가 비슷한 레이어 병합  
                                   ┗ 제거된 레이어 : `DrawerInteractable, PrivateInteractable, Particles, MixedLighting, TeleportArea,`   
                     `TeleportPoint, IgnoreTeleport`  
 
- `Inspector Attribute` 수정  
  ┣ 기존에 사용하던 `NaughtyAttributes` 제거 (`Odin Inspector`와 충돌)  
  ┗ `Label, Readonly, ShowIf / HideIf, Button` 추가  
   
- `PXRRig` 수정  
  ┣ 하위에 있는 `PXRPlayerController`의 변수에 대해 접근 가능  
  ┣ `PXRPlayerController` 클래스는 `partial`로 나누기  
  ┃ ┣ `Teleporter` : `PXRLocomotionController`  
  ┃ ┣ `SnapTurn` : `PXRLocomotionController`  
  ┃ ┣ `QuickSlot` : `PXRQuickSlotController`  
  ┃ ┣ `Grabber` : `PXRGrabberController`  
  ┃ ┣ `Animation Controller` : `PXRAnimationController`  
  ┃ ┣ `VRPointer` : `PXRVRPointerController`  
  ┃ ┗ `Height` : `PXREyeHeightController`  
  ┗ `PXRPlayerController`에서 불필요한 코드 제거  
   ┣- `ActivateAllControllerModel` 함수 제거  
   ┣- `DeactivateAllControllerModel` 함수 제거  
   ┗- `CoroutineCheckRecenterEvent` 함수 제거  
   
- `Domain reload`에 따른 코드 추가 및 수정  
  ┗ `static` 키워드 대신 `StaticVar`로 대체  
   
- `InputBridge`에서 키 피드백에 대해 접근하기 쉽도록 변경  
   
- `ForcedGrab` 추가  
  ┗ `PXRGrabber` 클래스에 `ForceGrab` 함수 추가  
   
- Teleport 코드 수정  
  ┣ 포인트 진입 시 효과 변경  
  ┣ 코드 전체 정리 (불필요한 부분 제거)    
  ┣ 텔레포트 콜라이더 계산방식 변경  
  ┗ 사용하는 레이어 갯수 감소  
     
- `Grabbable`의 위치 및 변수 초기화 이벤트 추가  
  ┣ `PXRGrabbable`에 `ReturnToOriginEvent` 추가  
  ┗ 복귀 함수를 `virtual`로 설정하여 `override` 가능  
       
- 상호작용 기능 추가  
  ┣ `PXRInteractAddon` : 손으로 하는 추가적인 동작의 상호작용을 관리하는 클래스    
  ┃ ┣ `PXRFingerInteract` : 손가락 상호작용  
  ┃ ┗ `PXRPalmInteract` : 손바닥 상호작용  
  ┣ `PXRDirectableBase` : 심화 동작의 상호작용이 되는 오브젝트들의 Base 클래스    
  ┃ ┗ `IDirectable` : 상세한 구분을 위해 인터페이스 추가  
  ┣ `PXRPressableButton` : 누를수 있는 3D 버튼에 해당하는 상호작용 클래스 - `PXRDirectableBase` 상속  
  ┃ ┗ 손가락으로 누르거나 손바닥으로 누르는게 가능  
  ┣ `PXRPullableLever` : 당길 수 있는 레버에 해당하는 상호작용 클래스 - `PXRDirectableBase` 상속   
  ┃ ┣ 특정 각도로 제한하여 당길 수 있고, 당겨진 정도를 체크 가능  
  ┃ ┗ 당기는 방향은 제한 X - 모든 방향으로 설치 가능  
  ┣ `PXRPullablePump` : 위 아래로 잡고 움직일 수 있는 펌프 상호작용 클래스 - `PXRDirectableBase` 상속    
  ┃ ┣ 최대, 최소 높이를 제한할 수 있으며, 현재 당겨진 정도를 체크 가능  
  ┃ ┗ 당기는 방향은 제한 X - 모든 방향으로 설치 가능    
  ┗ `PXRPullablePump` : 위 아래로 잡고 움직일 수 있는 펌프 상호작용 클래스 - `PXRDirectableBase` 상속    
    ┣ 회전되는 최대 바퀴수를 제한할 수 있으며, 현재 각도가 몇도가 돌아갔는지 확인 가능  
    ┗ 설치 방향은 제한 X - 모든 방향으로 설치 가능  
        
- `Sample` 폴더 제거 후 패키지 포함

## [1.2.1] - 2023.08.10
- `QuickSlot` 버그 수정

## [1.2.0] - 2023.08.09
- `QuickSlot` 기능 추가
- `QuickSlot` 샘플 추가

## [1.1.32] - 2023.08.03
- `PXRBaseController`의 연결된 기기 Debug를 보기위해 직렬화 

## [1.1.31] - 2023.08.03
- `PXRTeleportAreaBase`의 `OnEnterPlayer` 이벤트 시 `ForceHeight` 조건 추가

## [1.1.30] - 2023.08.03
- `PXRPlayerController` 플레이어의 카메라를 지정된 높이로 강제 변경시키는 기능 추가
- `PXRRigHeadRotator` Euler를 매개 변수로 하는 `ClampRotation` 함수 추가

## [1.1.29] - 2023.08.02
- `PXREnum` `SupportOffsetDevice`의 `enum` 수정

## [1.1.28] - 2023.08.02
- `PXRBaseController`에 `Key` 추가 및 초기화 제거

## [1.1.27] - 2023.08.01
- `PXREnum`의 `SupportOffsetDevice`에 `enum` 추가
- `PXRBaseController`에 `SetOffsetPosition` 함수 추가

## [1.1.26] - 2023.07.31
- `PXRBaseController` 기기 구분 조건 추가

## [1.1.25] - 2023.07.31
- `PXRBaseController`의 `Haptic` 기능 통합
- `PXRBaseController` 기기별 Offset 지정
- `PXR Module InputAction` 기기 추가

## [1.1.24] - 2023.06.13
- `PXRDirectInteractor` 딜레이 활성화 기능 추가
- `PXRDirectInteractableBase` 딜레이 활성화 기능 추가

## [1.1.23] - 2023.06.12
- `Mixed Reality`의 `Input System` 변경

## [1.1.22] - 2023.05.10
- `VRModule Sample` 경로 수정

## [1.1.21] - 2023.05.10
- `VRModule Sample` 리소스 업데이트

## [1.1.20] - 2023.05.09
- `PXRInteractableBase`의 `OnValidate` 함수 주석

## [1.1.19] - 2023.05.09
- `PXRPlayerControllerExtension` PC 모드인 경우 예외 처리 추가
- `PXRPointerVR` PC 모드인 경우 예외 처리 추가

## [1.1.18] - 2023.04.27
- `PXRBaseController`의 `SendHapticImpulse` 함수 추가 수정

## [1.1.17] - 2023.04.27
- `PXRBaseController`의 `SendHapticImpulse` 함수 수정

## [1.1.16] - 2023.04.27
- Pico 컨트롤러 진동 라이브러리 추가

## [1.1.15] - 2023.04.13
- `LuxOutline` 2개 이상의 메쉬 렌더러에서도 적용되도록 변경

## [1.1.14] - 2023.04.12
- `LuxOutline` 클래스 `enableRefreshRenderers` 조건 추가

## [1.1.13] - 2023.04.12
- `PXRDirectInteractor` 기능 추가

## [1.1.12] - 2023.04.12
- `PXRDirectInteractor` 클래스 수정
- `PXRDirect3DButton` 롤백

## [1.1.11] - 2023.04.07
- `PXREyeHeightChanger`의 `IsEnableAB`변수를 `static`로 변경

## [1.1.10] - 2023.04.07
- `PointerGrab` 물체 감지 `PXRGrabbableChild` 추가

## [1.1.9] - 2023.04.07
- `PointerGrab` 아이콘 표시 기능 버그 수정

## [1.1.8] - 2023.04.05
- `PXRGrabber`의 Show 포인터 코루틴 제거
- `PXRGrabber`의 `PointerGrab` 오프셋 값 추가
- `PXRDirectInteractor` 조건 추가

## [1.1.7] - 2023.04.04
- PXRDirect3DButton 함수 변경

## [1.1.6] - 2023.04.04
- `PXRDirect3DButton` 딜레이 추가 및 함수 이름 변경

## [1.1.5] - 2023.03.20
- `PointerGrab` 아이콘 표시 기능 On Off 추가

## [1.1.4] - 2023.03.20
- `Sample` 업데이트

## [1.1.3] - 2023.03.20
- `PointerGrab` 아이콘 표시 기능 수정

## [1.1.2] - 2023.03.20
- `PXRInputHandlerBase`의 `EnableAction`에 딜레이 롤백
- `PointerGrab`이 가능한 `Grabbable`를 가리킨 경우 아이콘 표시 기능 추가

## [1.1.1] - 2023.03.20
- `PXRInputHandlerBase`의 `EnableAction`에 딜레이 적용

## [1.1.0] - 2023.03.07
- `QuickOutline -> Lux Outline`로 교체
- `PXRInteractableBase`의 `DefineSymbol` 조건문 추가

## [1.0.29] - 2023.02.16
- `PXRInputHandlerQiyu` 클래스 추가
- `PXRInputHandlerBase` 에서 입력 `Threshold` 를 조절할 수 있도록 변수 직렬화

## [1.0.28] - 2023.01.18
- `Oculus Handler` 추가

## [1.0.27] - 2022.12.22
- 이벤트 명 변경
    - `PXRTeleportAreaBase` : `EnterPlayer -> EnterPlayerEvent` 변경
    - `PXRTeleportAreaBase` : `ExitPlayer -> ExitPlayerEvent` 변경
    - `PXRGrabber` : `Grabbed -> GrabbedEvent` 변경
    - `PXRGrabber` : `Released -> ReleasedEvent` 변경
    - `PXRGrabber` : `AfterForceRelease -> AfterForceReleaseEvent` 변경
    - `PXRClickable` : `pointerDown -> PointerdownEvent` 변경
    - `PXRClickable` : `pointerUp -> PointerupEvent` 변경
    - `PXRClickable` : `PointerStay -> PointerStayEvent` 변경
    - `PXRDirect3DButton` : `PointerClick -> PointerClickEvent` 변경
    - `PXRDirectInteractableBase` : `HoverAction -> HoverEvent` 변경
    - `PXRDirectInteractableBase` : `UnHoverAction -> UnHoverEvent` 변경
    - `PXRDirectInteractableBase` : `PressAction -> PressEvent` 변경
    - `PXREyeHeightChanger` : `BeforeChangeEyeHeight -> BeforeChangeHeightEvent` 변경
    - `PXREyeHeightChanger` : `AfterChangeEyeHeight -> AfterChangeHeightEvent` 변경
    - `PXRSnapTurn` : `ExcuteSnapTurnEvent -> ExecuteSnapTurnEvent` 변경

## [1.0.26] - 2022.12.13
- `PXRTeleporter` 클래스 `Teleport Cancel` 이벤트 추가

## [1.0.25] - 2022.12.13
- `PXRPlayerControllerExtensions`에 `BeforeSnapTurn` 함수 내용 변경

## [1.0.24] - 2022.12.03
- `PXRPointerVR` 클래스 `PXRLineRendererController Property` 수정

## [1.0.23] - 2022.12.02
- `PXRPointerVR` 클래스 `PXRPlayerController`, `PXRLineRendererController Property` 추가

## [1.0.22] - 2022.12.01
- `PXREyeHeightChanger` 클래스 `Height PlayerPrefs` 추가
- `PlayerPrefs` 제어 변수 추가

## [1.0.21] - 2022.12.01
- `PXREyeHeightChanger` 클래스 A B키를 이용한 높이 조절 제어 변수 추가

## [1.0.20] - 2022.12.01
- `PXREyeHeightChanger` 클래스 `PlayerController`, `MainCam Property` 추가

## [1.0.19] - 2022.12.01
- `PXREyeHeightChanger` 클래스 `ChangeEyeHeightImmediately` 함수 매개변수 수정

## [1.0.18] - 2022.12.01
- `PXREyeHeightChanger` 클래스 `ChangeEyeHeightImmediately` 함수 추가

## [1.0.17] - 2022.11.30
- `PXREyeHeightChanger` 클래스 `CheckCanMoveUp`, `CheckCanMoveDown` 함수 `public`로 변경

## [1.0.16] - 2022.11.29
- `PXRDirect3DButton` 클래스 비활성화 시 코루틴 실행이 되지 않도록 변경

## [1.0.15] - 2022.11.27
- `PXRInteractableBase` 클래스 `canInteract` 변수 변경

## [1.0.14] - 2022.11.23
- 레이캐스트 라인렌더러 On Off 기능 조건 추가

## [1.0.13] - 2022.11.17
-  레이캐스트 라인렌더러 On Off 기능 추가

## [1.0.12] - 2022.11.03
- `PXRScreenFade` 클래스 Fade 추가 이미지 변수 제공

## [1.0.11] - 2022.10.26
- `PXRGrabbable` 클래스 `ChangeLocalPosition`, `ChangeLocalRotation` 함수 변경

## [1.0.10] - 2022.10.26
- `Grabbable`이 다른쪽 손으로 이동이 되지 않도록 제한하는 기능 추가
- `PXRInteractableBase` 클래스 `CanInteract`의 `internal` 사용처 제거

## [1.0.9] - 2022.10.26
- 아웃라인 색상이 정상적으로 변경되지 않는 현상 수정

## [1.0.8] - 2022.10.26
- `PXRInteractableBase` 클래스 변수 수정

## [1.0.7] - 2022.10.26
- `PXRInteractableBase` 클래스 변수 수정
- `PXRGrabbable` 클래스 변수 제거

## [1.0.6] - 2022.10.25
- `PXRInteractableBase` 클래스 변수 추가
- `PXRInteractableBase` 클래스 변수 수정
- `PXRDirect3DButton` 클래스 `ClickedButton` 함수 수정
- `PXRDirectInteractor` 클래스 함수 수정
- `PXREnums`에 `TransferState enum` 추가

## [1.0.5] - 2022.10.24
- `PXRGrabbable` 클래스 함수 수정

## [1.0.4] - 2022.10.21
- `PXRBaseController` 클래스 `SendHapticImpulse` 함수 수정

## [1.0.3] - 2022.10.17
- `PXRGrabbable` HandSide 변수 수정
- `PXRDirectInteractableBase` 클래스 CanInteract 변수 추가
- `PXRDirectInteractableBase` 클래스 이름 변경
- `PXRDirect3DButton` 클래스 이름 변경
- `PXRDirectInteractor` 클래스 이름 변경

## [1.0.2] - 2022.09.28
- `PXRGrabbable` 함수 수정

## [1.0.1] - 2022.09.14
- `3D Button` 함수 추가

## [1.0.0] - 2022.04.29
- `VRModule` 업로드

