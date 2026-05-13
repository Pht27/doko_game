# Theme Extraction & Semantic Color System

## Goal

Analyze the frontend codebase and extract the current color system into a centralized semantic theme structure.

The end result should allow creating a new theme by copying a single theme file and changing only a few core colors.

---

# Tasks

## 1. Scan the codebase for all colors

Find and collect:

- Hex colors
- rgb/rgba
- hsl/hsla
- CSS variables
- Tailwind color classes
- Inline styles
- SCSS/SASS variables
- Styled-components / Emotion colors
- SVG fills/strokes

Generate:

```txt
color-inventory.json
```

Example:

```json
[
  {
    "value": "#4f46e5",
    "count": 37,
    "files": ["Button.tsx", "Navbar.tsx"]
  }
]
```

---

## 2. Normalize colors

- Merge equivalent colors
- Group near-identical shades
- Remove duplicates

Goal:
Reduce the palette to the actual core colors in use.

---

## 3. Categorize colors semantically

Classify colors into:

### Core
- primary
- secondary
- accent

### Neutral palette
- neutral-50
- neutral-100
- ...
- neutral-900

### Status
- success
- warning
- error

Optional:
- info
- focus
- overlay

---

## 4. Infer semantic usage

Analyze where colors are used and map them to semantic roles.

Examples:

| Usage | Semantic Token |
|---|---|
| body text | text.primary |
| muted text | text.secondary |
| page background | background |
| card background | surface |
| primary button | button.primary |
| borders | border.default |

Use:
- component names
- class names
- usage patterns
- DOM hierarchy
- Tailwind semantics

---

## 5. Generate centralized theme file

Generate:

```txt
theme.json
```

or:

```txt
theme.ts
```

Structure should separate:
- raw colors
- semantic tokens

Example:

```json
{
  "primary": "#4f46e5",

  "neutral": {
    "50": "#fafafa",
    "900": "#171717"
  },

  "semantic": {
    "text.primary": "neutral.900",
    "background": "neutral.50",
    "button.primary.bg": "primary"
  }
}
```

---

## 6. Generate CSS variables

Generate a CSS variable layer:

```css
:root {
  --color-primary: #4f46e5;
  --text-primary: #171717;
}
```

Goal:
No hardcoded colors in UI components anymore.

---

## 7. Refactor suggestions

Generate migration suggestions:

Example:

```txt
#111827
→ var(--text-primary)
```

or:

```txt
text-gray-900
→ text-primary
```

---

## 8. Generate analysis report

Generate:

```txt
theme-analysis.md
```

Include:
- detected colors
- grouped palette
- semantic mapping
- duplicate/redundant colors
- inconsistent usages
- contrast issues
- refactor opportunities

---

# Important

- Do NOT derive neutral colors automatically from primary colors
- Neutral palettes should be treated independently
- Goal is semantic consistency, not automatic color generation
- Avoid hardcoded colors in components
- Prefer semantic tokens everywhere
