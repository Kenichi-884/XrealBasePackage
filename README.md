# XrealBasePackage

XREAL (Nreal) 向け AR プロジェクトの共通基盤 UPM パッケージ。

パッケージ本体は `XRealBasePackage/Packages/com.xreal.basepackage/` に Embedded Package として置かれている。詳細(提供コンポーネント一覧、導入方法、使い方)は以下を参照:

- [パッケージ README](XRealBasePackage/Packages/com.xreal.basepackage/README.md)

`XRealBasePackage/` は、このパッケージを Unity Editor で開いてコンパイル・動作確認するための最小構成のテストホストプロジェクト。XREAL公式SDK(`com.xreal.xr`, `LocalPackages/`)とXR Plug-in Management設定(`Assets/XR/`)も導入済みなので、新規プロジェクトを実機開発可能な状態にする際のひな形としても使える(詳細はパッケージREADMEの「XREAL実機開発を始めるためのチェックリスト」を参照)。
