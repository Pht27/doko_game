import { useState } from 'react';

export const NEUTRALS = ['navy', 'rose', 'aqua', 'classic', 'cherry', 'nautical'] as const;
export const ACCENTS  = ['indigo', 'rose', 'aqua', 'classic', 'cherry', 'nautical'] as const;

export type Neutral = (typeof NEUTRALS)[number];
export type Accent  = (typeof ACCENTS)[number];

export interface Preset {
  id:      string;
  name:    string;
  neutral: Neutral;
  accent:  Accent;
  swatches: [string, string, string, string]; // bg, surface, re, kontra
}

export const PRESETS: Preset[] = [
  { id: 'default',  name: 'Default',  neutral: 'navy',     accent: 'indigo',   swatches: ['#1a1a2e', '#22223a', '#818cf8', '#c084fc'] },
  { id: 'rose',     name: 'Rosé',     neutral: 'rose',     accent: 'rose',     swatches: ['#FAE4EE', '#F0C0D6', '#3A70C8', '#D43060'] },
  { id: 'aqua',     name: 'Aqua',     neutral: 'aqua',     accent: 'aqua',     swatches: ['#D8F0F2', '#B0D8DC', '#0E8898', '#CC1010'] },
  { id: 'classic',  name: 'Classic',  neutral: 'classic',  accent: 'classic',  swatches: ['#E8ECF2', '#D0D8E8', '#2860B0', '#2D7D42'] },
  { id: 'cherry',   name: 'Cherry',   neutral: 'cherry',   accent: 'cherry',   swatches: ['#F8E0DC', '#F0B8B0', '#880008', '#B81818'] },
  { id: 'nautical', name: 'Nautical', neutral: 'nautical', accent: 'nautical', swatches: ['#D0E4F0', '#A8CCDE', '#002C54', '#C5001A'] },
];

const STORAGE_NEUTRAL = 'doko-theme-neutral';
const STORAGE_ACCENT  = 'doko-theme-accent';

function readNeutral(): Neutral {
  const stored = localStorage.getItem(STORAGE_NEUTRAL);
  return (NEUTRALS as readonly string[]).includes(stored ?? '') ? (stored as Neutral) : 'navy';
}

function readAccent(): Accent {
  const stored = localStorage.getItem(STORAGE_ACCENT);
  return (ACCENTS as readonly string[]).includes(stored ?? '') ? (stored as Accent) : 'indigo';
}

function applyClasses(neutral: Neutral, accent: Accent) {
  const el = document.documentElement;
  for (const n of NEUTRALS) el.classList.remove(`neutral-${n}`);
  for (const a of ACCENTS)  el.classList.remove(`accent-${a}`);
  el.classList.add(`neutral-${neutral}`, `accent-${accent}`);
}

export function useTheme() {
  const [neutral, setNeutralState] = useState<Neutral>(readNeutral);
  const [accent,  setAccentState]  = useState<Accent>(readAccent);

  function setNeutral(n: Neutral) {
    setNeutralState(n);
    applyClasses(n, accent);
    localStorage.setItem(STORAGE_NEUTRAL, n);
  }

  function setAccent(a: Accent) {
    setAccentState(a);
    applyClasses(neutral, a);
    localStorage.setItem(STORAGE_ACCENT, a);
  }

  function setPreset(preset: Preset) {
    setNeutralState(preset.neutral);
    setAccentState(preset.accent);
    applyClasses(preset.neutral, preset.accent);
    localStorage.setItem(STORAGE_NEUTRAL, preset.neutral);
    localStorage.setItem(STORAGE_ACCENT,  preset.accent);
  }

  const activePreset = PRESETS.find(p => p.neutral === neutral && p.accent === accent) ?? PRESETS[0];

  return { neutral, accent, setNeutral, setAccent, setPreset, activePreset };
}
