# PocketMoneyManager

Unity 2022.3 LTS で開発するお小遣い管理アプリです。

## プレイ（WebGL）

ブラウザですぐにプレイできます。

https://kuremasa.github.io/PocketMoneyManager/

> GitHub Pages の初回公開後、反映まで数分かかることがあります。

## 開発環境

- Unity 2022.3.62f1

## プロジェクト構成

```
PocketMoneyManager/          # リポジトリルート
├── docs/                    # GitHub Pages 公開用 WebGL ビルド
├── PocketMoneyMonager/      # Unity プロジェクト
│   ├── Assets/
│   ├── Packages/
│   └── ProjectSettings/
└── README.md
```

## セットアップ

1. [Unity Hub](https://unity.com/download) から Unity 2022.3.62f1 をインストール
2. Unity Hub で `PocketMoneyMonager` を開く

## WebGL 公開手順

1. Unity で **File → Build Settings → WebGL** を選択してビルド
   - 出力先: `PocketMoneyMonager/Builds/WebGL/PocketMoneyManager/`
   - **Player Settings → Publishing Settings → Compression Format** は **Disabled**
2. ビルド成果物を `docs/` にコピー（`index.html`, `Build/`, `TemplateData/`）
3. 変更を commit & push
4. GitHub リポジトリの **Settings → Pages** で **Branch: main / Folder: /docs** を設定

## ライセンス

使用フォント:

- [Liberation Sans](https://github.com/liberationfonts/liberation-fonts) (SIL Open Font License)
- [Noto Sans JP](https://fonts.google.com/noto/specimen/Noto+Sans+JP) (SIL Open Font License)
