# Changelog

## [Unreleased]
### Added
- テストホストプロジェクト(`XRealBasePackage/`)にXREAL公式SDK(`com.xreal.xr`)とXR Plug-in Management設定(`Assets/XR/`)を導入し、実機開発の土台として使えるように整備。
- パッケージREADMEに「XREAL実機開発を始めるためのチェックリスト」を追加(このパッケージ+SDKで揃うもの/プロジェクトごとに手動で用意が必要なものを明記)。

## [0.1.0] - 2026-08-18
### Added
- XrealFishDemoAR から共通基盤を移植して初期リリース。
- AR: `MarkerEventRelay`, `IMarkerContentSource`, `ARMarkerDebugHUD`
- Audio: `AudioCue`, `AudioDirector`, `VoiceEntry`
- Effects: `DissolveEffect`, `WorldExpandEffect`
- Scene: `EventSceneConfig`, `EventSceneDirector`, `SceneActivation`
- Interaction: `GrabbableEventSender`, `IntroWindowPlacer`, `PalmSummonWindow`, `WindowGimmick`, `PinchOpenDoors`
- Debug: `HandGimmickDebugHUD`
- Editor: `ParticleScaleTool`
