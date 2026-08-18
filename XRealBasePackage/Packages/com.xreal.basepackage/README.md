# XrealBasePackage

XREAL (Nreal) 向け AR プロジェクトの共通基盤 UPM パッケージ。
`XrealFishDemoAR` から「マーカー検出→コンテンツ表示→シーン進行→音声/演出」という一連の流れで再利用できる部分を抽出したもの。個別プロジェクト固有のロジック(魚モデルの挙動など)は含まない。

## リポジトリ構成

```
XrealBasePackage/ (リポジトリルート)
└── XRealBasePackage/                          # パッケージ動作確認用の Unity プロジェクト(テストホスト)
    ├── Assets/
    ├── Packages/
    │   ├── manifest.json                      # com.xreal.basepackage を file:com.xreal.basepackage で埋め込み参照
    │   └── com.xreal.basepackage/              # このパッケージ本体(Embedded Package)
    │       ├── package.json
    │       ├── Runtime/
    │       └── Editor/
    └── ProjectSettings/
```

パッケージ本体は Unity の Embedded Package として `XRealBasePackage/Packages/com.xreal.basepackage/` に配置している。Unity Editor で `XRealBasePackage/` を開けば Package Manager がそのまま検出し、`Runtime/` や `Editor/` の変更は即座にコンパイルへ反映される。

他プロジェクトから利用する場合は、`Packages/manifest.json` に git URL を `path` クエリ付きで追記する(パッケージ本体がリポジトリ直下ではなくサブフォルダにあるため)。

```json
{
  "dependencies": {
    "com.xreal.basepackage": "https://github.com/Kenichi-884/XrealBasePackage.git?path=XRealBasePackage/Packages/com.xreal.basepackage"
  }
}
```

## 提供コンポーネント

### AR
| スクリプト | 役割 |
|---|---|
| `IMarkerContentSource` | マーカー検出でコンテンツを表示するコンポーネントが実装するインターフェース。`MarkerEventRelay` はこれ経由でのみ検出元と結合する。 |
| `MarkerEventRelay` | `IMarkerContentSource` 実装のイベントを受け取り、シーン/オーディオ側へ中継する。指定した全マーカーが揃うと `onAllRequiredMarkersFound` を発火。 |
| `ARMarkerDebugHUD` | 実機デバッグ用。`ARTrackedImageManager` のトラッキング状態を TMP テキストに表示する。 |

### Audio
| スクリプト | 役割 |
|---|---|
| `AudioCue` | BGM/アンビエント/ボイス1つ分の再生設定を持つ ScriptableObject。 |
| `AudioDirector` | BGM・アンビエント・ボイスの再生を一元管理するシングルトン。クロスフェード、ボイスの順次再生に対応。 |
| `VoiceEntry` | 順番再生するボイス1エントリ(`AudioCue` + 前のクリップからの待機秒数)。 |

### Effects
| スクリプト | 役割 |
|---|---|
| `DissolveEffect` | Dissolve Burn シェーダーの Dissolve Amount を制御してオブジェクトをフェードイン/アウトさせる。非対応マテリアルには enabled 切替でフォールバック。 |
| `WorldExpandEffect` | 起動時に localScale を 0 から目標値までイージング拡大する。 |

### Scene
| スクリプト | 役割 |
|---|---|
| `EventSceneConfig` | シーン1つ分の BGM/アンビエント/ボイスシーケンス設定を持つ ScriptableObject。 |
| `EventSceneDirector` | Unity シーンを切り替えず、内部ステートで体験全体の進行(シーン0→1→2…)を管理する。`AdvanceScene()` / `GoToScene(id)` で進行。 |
| `SceneActivation` | `EventSceneDirector.onSceneEntered` を購読し、指定シーンIDで自身を表示/非表示にする(`DissolveEffect` があれば併用)。 |

### Interaction
| スクリプト | 役割 |
|---|---|
| `GrabbableEventSender` | `XRGrabInteractable` のつかむ/離すイベントを `UnityEvent` として公開する薄いラッパー。 |
| `IntroWindowPlacer` | 起動時にオブジェクトをカメラ正面に配置する。 |
| `PalmSummonWindow` | 掌を上に向け続けるとウィンドウをカメラ前方に召喚し、`PinchOpenDoors` を有効化する。 |
| `PinchOpenDoors` | ピンチ動作を検知して左右ドアを自動アニメで開き、開き切ると `onDoorsOpened` を発火する。 |
| `WindowGimmick` | 左右ドアの回転角を監視し、両方が閾値以上開いたら `onWindowOpened` を発火する(ドア自体の回転は他コンポーネント任せ)。 |

### Debug
| スクリプト | 役割 |
|---|---|
| `HandGimmickDebugHUD` | 実機デバッグ用。ハンドトラッキング状態、掌方向、ピンチ距離、`PalmSummonWindow`/`PinchOpenDoors` の進行状況を TMP テキストに表示する。 |

### Editor
| スクリプト | 役割 |
|---|---|
| `ParticleScaleTool` (`Tools/XrealBase/Particle Scale Tool`) | 選択オブジェクトの `Transform.localScale` を変更してパーティクルを非破壊スケールする。`ParticleSystem.scalingMode` を自動で `Hierarchy` に設定。 |

## 依存パッケージ
`package.json` の `dependencies` を参照。主に以下を利用する:
- `com.unity.xr.arfoundation`
- `com.unity.xr.interaction.toolkit`
- `com.unity.xr.hands`
- `com.unity.inputsystem`
- `com.unity.ugui` (TextMeshPro)

Unity 6000.0 (Unity 6) 以降を想定。

## 導入方法
「リポジトリ構成」節を参照(`path` クエリ付きの git URL で `Packages/manifest.json` に追記する)。特定バージョンを固定したい場合は URL 末尾に `#v0.1.0` のようにタグを追加する。

特定バージョンを固定したい場合は `#v0.1.0` のようにタグを指定する。

## 使い方の要点

### マーカー検出をこのパッケージに繋ぐ
自プロジェクト側のマーカースポナー(例: `GoldfishMarkerSpawner` に相当するコンポーネント)に `IMarkerContentSource` を実装させ、`OnContentShown` プロパティでコンテンツ表示イベントを公開する。同じ GameObject に `MarkerEventRelay` をアタッチすれば、プロジェクト固有のスポナーとパッケージ側を疎結合のまま連携できる。

```csharp
public class MyMarkerSpawner : MonoBehaviour, IMarkerContentSource
{
    [SerializeField] UnityEvent<string, GameObject> m_OnContentShown = new();
    public UnityEvent<string, GameObject> OnContentShown => m_OnContentShown;
    // ... マーカー検出・スポーン処理 ...
}
```

### シーン進行とオーディオ
1. `EventSceneConfig` をシーンの数だけ作成。
2. `EventSceneDirector` にアサインし、各 `SceneEntry` の `onEnter` / `onExit` に表示切替などを配線。
3. `AudioDirector` をシーンに1つ配置(シングルトン)。`EventSceneDirector` が自動で BGM/アンビエント/ボイスを再生する。

### 掌ウィンドウ召喚 → ピンチで開く
`PalmSummonWindow` → (掌を上向きにキープ) → ウィンドウ表示 & `PinchOpenDoors` 有効化 → (ピンチ) → ドアが開いて `onDoorsOpened` 発火、という一連の流れをそのまま利用できる。実機確認には `HandGimmickDebugHUD` を使う。

## 移植元との差分
`XrealFishDemoAR` のコードをそのまま複製せず、以下の点を汎用化している:
- `MarkerEventRelay` は魚専用スポナーへの直接依存(`RequireComponent`)を廃し、`IMarkerContentSource` インターフェース経由に変更。
- `AudioDirector` が使う `VoiceEntry` を `EventSceneConfig` のネストクラスから独立させ、Audio → Scene の片方向依存に整理(循環依存の解消)。
- `HandGimmickDebugHUD` からプロジェクト固有の `PinchGrabBowl` 依存を削除。
- `CreateAssetMenu` のメニューパスを `FishAR/...` から `XrealBase/...` に変更。
- Goldfish/Bowl 固有ロジック(マーカースポナー本体、掴み演出、リップシンク、遊泳挙動、シーンセットアップエディタ拡張、デモ用デバッグパネル)は移植対象外。

## ライセンス
社内/関係者限定利用。`LICENSE.md` を参照。
