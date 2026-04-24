# Frontend Guidelines

## Stack

- **React 19** + **TypeScript** (strict)
- **Vite** — build tool and dev server
- **Tailwind CSS v4** — utility-first styling
- **vite-plugin-svgr** — import SVGs as React components
- **@microsoft/signalr** — real-time game events
- **vite-plugin-pwa** — PWA manifest + service worker
- **Target**: mobile landscape (fullscreen PWA), also works in desktop browser

## Folder Structure

```
src/
  api/           HTTP client wrappers + SignalR setup + card SVG mapping
  assets/        Card SVGs (suits: kr/p/h/k, ranks: 9/10/B/D/K/A)
  components/    Generic, non-domain UI (currently: PortraitOverlay)
  features/
    game/        All game-domain components (GameBoard, TrickArea, HandDisplay, …)
    lobby/       Lobby UI (MultiplayerBrowserPage, LobbyDetailView)
  hooks/         Custom React hooks (useGameState, useGameActions, useLobby, …)
  pages/         Route-level views (LandingPage)
  styles/        index.css — global Tailwind entry point only
  types/         TypeScript types mirroring backend DTOs (api.ts)
  utils/         Pure utilities (translations.ts, env.ts)
  App.tsx
  main.tsx
```

## Path Alias

Use `@/` for all cross-feature imports:

```ts
import { t } from '@/utils/translations';
import { GameBoard } from '@/features/game/GameBoard/GameBoard';
```

Use relative imports only within the same feature/folder.

## Component Conventions

- One component per file, filename matches export name (`PascalCase`)
- Co-locate CSS with the component file it belongs to (e.g. `TrickArea/TrickArea.css`)
- CSS files reference global Tailwind via `@reference "../../../styles/index.css"` (adjust depth)
- Complex components get a `subcomponents/` subfolder; flat siblings for same-level extractions
- No default exports — always named exports

## Styling

- Prefer Tailwind utility classes inline
- Extract to `.css` class names (with `@apply`) when:
  - A class combination repeats across multiple JSX elements in the same component
  - An animation or pseudo-selector is needed
- CSS class names use `kebab-case` with a component-specific prefix (e.g. `rd-` for ResultDisplay, `rh-` for LobbyHistory)
- No global styles except in `styles/index.css`

## State & Data Flow

- No external state management library — `useState` + custom hooks
- Game state flows: `useGameState` polls/reacts to SignalR → `App.tsx` passes down via props
- API calls live in `src/api/`, never inline in components
- Hooks in `src/hooks/` own side effects; components are pure renderers

## TypeScript

- Strict mode (`noUnusedLocals`, `noUnusedParameters`, `verbatimModuleSyntax`)
- Types for all DTOs in `src/types/api.ts` — keep in sync with backend responses
- Use `import type` for type-only imports
- No `any`

## Mobile / PWA

- Designed for **landscape orientation** — `orientation: "landscape"` in PWA manifest
- `PortraitOverlay` blocks interaction in portrait and prompts rotation
- `screen.orientation.lock("landscape")` attempted on mount (not supported on iOS — handled by overlay)
- Fullscreen entered on first user interaction when running as installed PWA
- Test at 375×667 rotated (iPhone landscape) and 768×1024 rotated (iPad landscape)

## Translations

All user-facing strings live in `src/utils/translations.ts` — import via `t.key`.  
No hardcoded German strings in component files.

## Environment

- `src/utils/env.ts` exports feature flags and config read from `import.meta.env`
- `.env.local` / `.env.production` / `.env.staging` hold environment-specific values
- Never access `import.meta.env` directly in components — go through `env.ts`
