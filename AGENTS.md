# Repository Guidelines

## Project Structure & Module Organization

This repository currently centers on a Vite React application in `frontend/`. Source code lives in `frontend/src/`, with shared domain types in `frontend/src/models/`, graph components in `frontend/src/components/TopologyGraph/`, and Azure node components in `frontend/src/components/nodes/`. Component styles use colocated SCSS modules such as `FunctionNode.module.scss`, and Storybook stories are colocated as `*.stories.tsx`. Static assets are split between `frontend/public/` for served files and `frontend/src/assets/` for bundled imports. Schema documentation belongs under `docs/schema/`.

## Build, Test, and Development Commands

Run commands from `frontend/` and use pnpm, as indicated by `frontend/pnpm-lock.yaml`.

- `pnpm install` installs dependencies.
- `pnpm dev` starts the Vite development server.
- `pnpm build` runs TypeScript project builds and creates the Vite production bundle.
- `pnpm lint` runs Oxlint.
- `pnpm storybook` starts Storybook on port 6006.
- `pnpm build-storybook` builds the static Storybook site.
- `pnpm exec vitest run` runs the configured Vitest projects, including Storybook browser tests.

## Coding Style & Naming Conventions

Use TypeScript, React function components, and ESM imports. Follow the existing two-space indentation and single-quote import style. Name React components and component directories in PascalCase, for example `TopologyGraph` or `ServiceBusQueueNode`. Name model files by domain noun in lowercase, such as `graph.ts` or `warning.ts`. Keep component public exports in colocated `index.ts` files. Prefer strongly typed props and domain model reuse from `frontend/src/models/` over ad hoc object shapes.

## Testing Guidelines

Storybook is the primary component test surface. Add or update a colocated `*.stories.tsx` file when changing component behavior, and use `play` functions with `storybook/test` assertions for visible UI expectations. Run `pnpm exec vitest run` before submitting changes that affect stories or rendering, and run `pnpm build` for TypeScript and production build validation.

## Commit & Pull Request Guidelines

Recent history uses Conventional Commit style with scopes, such as `feat(frontend): add Dagre auto-layout` and `style(frontend): define node status colors`. Keep commits focused and use scopes like `frontend` or `docs` when applicable. Pull requests should describe the change, list validation commands run, link related issues, and include screenshots or Storybook references for visual component changes.

## Agent-Specific Instructions

Do not overwrite generated or user-authored files without checking first. Keep changes scoped to the requested area, and avoid committing build output such as `frontend/dist/` unless explicitly requested.
