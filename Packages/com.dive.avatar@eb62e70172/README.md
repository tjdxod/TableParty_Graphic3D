# Dive.AvatarCustom

- 플랫폼별 아바타 기능 제공

## 업데이트 로그

### [2.1.2] - 2025.04.10
- `PXRMetaPresetAvatarBase` 클래스에 Stand / Sit을 세팅하는 함수 추가

### [2.1.1] - 2025.04.08
- 피코 아바타 2.1.1 업데이트
- `PXRPicoAvatarLaunchBase` 클래스 수정

### [2.1.0] - 2025.04.07
- 메타 아바타 2.0 공식 업데이트
- `PXRMetaAvatarEntityBase` 클래스 try catch 수정

### [2.0.6] - 2025.04.01
- `PXRMetaPresetAvatarBase` 클래스 `CreateEntity` 강제 호출

### [2.0.5] - 2025.03.28
- `PXRMetaAvatarEntityBase` 클래스 `StreamingAssets`에서 `Zip`로 변경

### [2.0.4] - 2025.03.28
- `PXRMetaAvatarEntityBase` 클래스 예외처리 추가

### [2.0.3] - 2025.03.28
- `PXRMetaAvatarTrackingDelegate` 클래스 수정

### [2.0.2] - 2025.03.25
- `PXRAvatarBridgeComponent` 클래스 롤백

### [2.0.1] - 2025.03.25
- `PXRAvatarBridgeComponent` 클래스 수정

### [2.0.0] - 2025.03.25
- 메타 아바타 SDK 2.0 업데이트 적용

### [1.9.3] - 2024.12.03
- `PXRMetaAvatarEntityBase` 클래스에 `isPhotoAvatar` 변수 제거 후 `OvrAvatarEntity_Loading` 에서 `photoAvatar` 상속 받기

### [1.9.2] - 2024.12.03
- `PXRMetaAvatarEntityBase` 클래스에 `isPhotoAvatar` 변수 추가

### [1.9.1] - 2024.09.03
- `Meta` 플랫폼 아바타에 투명도 조절 기능 수정

### [1.9.0] - 2024.09.03
- `Meta` 플랫폼 아바타에 투명도 조절 기능 추가

### [1.8.10] - 2024.08.30
- `PXRMetaAvatarPlatformInit` 클래스에서 `Steam` 플랫폼 전용을 다른 `StandAlone` 환경에서도 호환되도록 변경
- `PXRMetaAvatarEntityBase` 클래스의 `Define Symbol` 내부 조건 수정

### [1.8.9] - 2024.08.27
- `PXRMetaAvatarEntityBase` 클래스의 `Define Symbol` 내부 조건 수정

### [1.8.8] - 2024.08.27
- `PXRMetaAvatarEntityBase` 클래스의 `Define Symbol` 내부 조건 수정

### [1.8.7] - 2024.08.27
- `PXRMetaAvatarEntityBase` 클래스의 프리셋 아바타 생성 경로 수정
- `PXRMetaAvatarEntityBase` 클래스의 `Define Symbol` 조건 수정

### [1.8.6] - 2024.08.09
- `Steam` 토큰 생성 예외처리 추가

### [1.8.5] - 2024.08.09
- `Steam` 플랫폼은 `Meta` 유저가 프리셋 아바타로 보이도록 변경

### [1.8.4] - 2024.08.08
- `PXRMetaAvatarEntityBase` 클래스의 `namakaId` 변수를 `uniqueId`로 변경

### [1.8.3] - 2024.08.08
- `PXRMetaAvatarEntityBase` 클래스의 `GetFederateAccessToken` 함수 수정

### [1.8.2] - 2024.08.08
- `PXRMetaAvatarPlatformInit` 클래스의 `FederatedUser` 관련 토큰 함수 변경

### [1.8.1] - 2024.08.06
- `PXRMetaAvatarEntityBase` 클래스의 `Task`를 `UniTask`로 수정

### [1.8.0] - 2024.08.05
- `Steam` 플랫폼용 아바타 인증 기능 추가

### [1.7.2] - 2024.07.10
- `PXRPicoAvatarBase` 클래스의 `CreateAvatar` 함수 조건 수정

### [1.7.1] - 2024.07.10
- Pico Avatar SDK 업데이트에 따른 코드 수정

### [1.7.0] - 2024.05.09
- 패키지 displayName 변경 (`AvatarCustom` -> `Dive Avatar SDK`)
- 패키지 name 변경 (`com.dive.avatarcustom` -> `com.dive.avatar`)
- 패키지 description 변경

### [1.6.13] - 2024.05.07
- `PXRMetaAvatarEntityBase` 클래스의 `PresetAvatarIndex` 변수 및 `Initialize` 함수 수정

### [1.6.12] - 2024.05.01
- `IAvatar` 인터페이스에 `AddColliderCompleted` 프로퍼티 추가

### [1.6.11] - 2024.04.16
- `PXRPicoAvatarBase` 클래스의 `AudioSource` 프로퍼티 수정
- `PXRMetaAvatarEntityBase` 클래스의 `AudioSource` 프로퍼티 수정

### [1.6.10] - 2024.04.12
- `PXRPicoAvatarIKSettings` 클래스의 `SetFindXROrigin` 함수 수정

### [1.6.9] - 2024.04.11
- `PXRPicoAvatarBase` 클래스 수정

### [1.6.8] - 2024.04.11
- `PXRPicoAvatarHand` 클래스 추가
- `PXRPicoAvatarBase` 클래스에 커스텀 핸드 지원 기능 추가

### [1.6.7] - 2024.04.08
- `PXRMetaAvatarEntityBase` 클래스의 조건 추가
- `PXRPicoAvatarBase` 클래스의 조건 간소화

### [1.6.6] - 2024.04.02
- `IAvatar` 인터페이스의 이벤트 매개변수 변경

### [1.6.5] - 2024.04.02
- `IAvatar` 인터페이스에 `AudioSource` 관련 함수 추가

### [1.6.4] - 2024.03.27
- `IAvatar` 인터페이스 static 이벤트 및 함수 추가

### [1.6.3] - 2024.03.27
- `IAvatar` 인터페이스 롤백

### [1.6.2] - 2024.03.27
- `IAvatar` 인터페이스 제네릭으로 수정
- `Meta`에서 물건을 잡을 시 주먹을 쥔 포즈로 고정하는 기능 제거

### [1.6.1] - 2024.03.27
- `PXRPicoAvatarBase` 클래스 수정

### [1.6.0] - 2024.03.27
- 플랫폼 구분을 위한 아바타 인터페이스 추가
- 아바타 폴더 정리

### [1.5.2] - 2024.03.26
- `Pico` 아바타 클래스 수정 

### [1.5.1] - 2024.03.25
- `Steam` 플랫폼을 구분하는 `DIVE_PLAFTORM_STEAM` `Define Symbol` 추가

### [1.5.0] - 2024.03.25
- `Meta` 와 `Pico`를 구분하는 `Define Symbol` 추가
- `Pico` 플랫폼 아바타 생성 기능 추가

### [1.4.5] - 2024.03.21
- `PXRMetaAvatarEntityBase` 클래스에 로컬 3인칭 아바타를 위한 `SetThirdPersonOwnerAvatar` 함수 및 `useNetwork` 변수 추가

### [1.4.4] - 2024.03.11
- `PXRMetaAvatarEntityBase` 클래스 `Task`를 다시 `Coroutine`로 롤백

### [1.4.3] - 2024.03.11
- `Meta Avatar SDK` 프리셋 아바타에 콜라이더 추가
- `PXRMetaPresetAvatarBase` 클래스의 `AddCollider` 함수 추가

### [1.4.2] - 2024.03.06
- `PXRMetaPresetAvatarBase` 클래스의 `AssetData` 구조체 접근 제한자 변경

### [1.4.1] - 2024.03.05
- 프리셋 아바타를 생성하는 `PXRMetaPresetAvatarBase` 클래스 추가
- 프리셋 아바타를 생성하는 `PXRMetaPresetAvatar` 프리펩 추가
- 프리셋 아바타의 자세를 기본 자세로 변경하는 `Byte Array` 추가

### [1.4.0] - 2024.03.04
- `Meta Avatar SDK` 업데이트에 따른 코드 수정

### [1.3.9] - 2024.02.21
- `PXRMetaAvatarEntityBase` 클래스의 `PollForAvatarChangeAsync` 함수 조건 추가

### [1.3.8] - 2024.02.19
- `PXRMetaAvatarControlDelegate` 클래스의 `UpdateControllerInput`함수 조건 추가

### [1.3.7] - 2024.02.16
- `PXRMetaAvatarEntityBase` 클래스 수정

### [1.3.6] - 2024.02.16
- `PXRMetaAvatarEntityBase` 클래스 수정
- `MetaAvatar` 관련 클래스 주석 추가

### [1.3.5] - 2024.02.15
- `Define Symbol` 조건 추가
- `PXRMetaAvatarEntityBase` 클래스 변수의 접근 제한자 변경

### [1.3.4] - 2024.02.15
- 불필요한 코드 제거

### [1.3.3] - 2024.02.15
- `PXRMetaAvatarControlDelegate` 클래스 수정

### [1.3.2] - 2024.02.15
- `PXRMetaAvatarControlDelegate` 클래스 추가

### [1.3.1] - 2024.02.14
- `PXRMetaAvatarEntityBase` 클래스에 `Define Symbol` 조건 추가

### [1.3.0] - 2024.02.14
- `Meta Avatar SDK`를 이용한 Meta 아바타 생성 기능 추가

### [1.2.3] - 2024.02.13
- `assembly definition` 파일 수정

### [1.2.2] - 2024.02.13
- Package Dependency 제거

### [1.2.1] - 2024.02.13
- Package Dependency 추가

### [1.2.0] - 2024.01.12
- 네임스페이스 변경 (`PlugVR.AvatarCustomizing` -> `Dive.AvatarCustom`)

### [1.1.4] - 2023.03.31
- AvatarCapture 기능 수정

### [1.1.3] - 2023.02.08
- AvatarCustomizing Editor asmdef 제거

### [1.1.2] - 2023.01.25
- AvatarArray 저장 이벤트 추가

### [1.1.1] - 2023.01.25
- AvatarCustomizeBase 클래스 SetColor 함수 매개변수 추가

### [1.1.0] - 2023.01.18
- AvatarCustomizing 폴더 구조 변경

### [1.0.7] - 2022.12.16
- Avatar Hand의 Shader Color 코드 교체

### [1.0.6] - 2022.12.01
- AvatarCustomizing UI 코드 제거

### [1.0.5] - 2022.11.27
- AvatarCustomizing asmdef 수정

### [1.0.4] - 2022.11.16
- Avatar의 Shader 교체
- 아바타 프로필 캡쳐 기능

### [1.0.3] - 2022.09.13
- Bug fix

### [1.0.2] - 2022.04.29
- Code Refactor

### [1.0.1] - 2022.04.28
- Add Sample

### [1.0.0] - 2022.04.26
- AvatarCustomizing Push
