# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

`azu-topo` は Azure リソース（Function Apps・Service Bus）のトポロジーをビジュアル化する Web アプリケーション。現在はフロントエンドのみ存在する。

## Commands

作業ディレクトリはすべて `frontend/` 。パッケージマネージャーは **pnpm**。

```bash
# 開発サーバー起動
cd frontend && pnpm dev

# ビルド（型チェック込み）
cd frontend && pnpm build

# Lint
cd frontend && pnpm lint

# Storybook 起動
cd frontend && pnpm storybook

# テスト実行（Playwright/Chromium ヘッドレス）
cd frontend && pnpm vitest
```

## Architecture

### Domain Models (`src/models/`)

アプリのコアデータ構造。グラフ全体の型は `TopologyGraph`：

- **Node** (`node.ts`): `function | serviceBusQueue | serviceBusTopic | serviceBusSubscription` の4種。各 type に対応した `Properties` 型が union で定義されている。
- **Edge** (`edge.ts`): `contains | trigger | output` の3種。`source`/`target` は NodeId。
- **Metric** (`metrics.ts`): ノード単位のメトリクス値（`name`, `value`, `unit`）のリスト。
- **Warning** (`warning.ts`): `connectionNotFound | resourceNotFound | metricUnavailable | unsupportedBinding` の警告コード付き。

### Component Convention (`src/components/`)

ノードコンポーネントは `nodes/<NodeName>/` フォルダ単位で管理：

```
nodes/FunctionNode/
  FunctionNode.tsx          # コンポーネント本体
  FunctionNode.module.scss  # SCSS Modules でスタイル
  FunctionNode.stories.tsx  # Storybook + テスト兼用
  index.ts                  # 再エクスポート
```

### Testing Strategy

テストは **Storybook の `play` 関数** として記述する。Vitest + `@storybook/addon-vitest` が Storybook ストーリーをブラウザテストとして実行する（Playwright/Chromium ヘッドレス）。独立したユニットテストファイルは作らず、ストーリーがテストを兼ねる。

### Tooling Notes

- **React Compiler** が有効（`babel-plugin-react-compiler`）。手動 `useMemo`/`useCallback` は原則不要。
- **Linter**: oxlint（ESLint ではない）。設定は `.oxlintrc.json`。
- **Formatter**: Prettier（設定は `.prettierrc`）— シングルクォート・セミコロンあり・タブ幅2。
- **SCSS Modules** を使用。グローバル CSS は `src/index.css` のみ。
- Azure リソースのアイコン SVG は `public/icons/azure/` に配置し、`img src` で参照する。
