const SUIT_MAP: Record<string, string> = {
  Kreuz: 'kr',
  Pik: 'p',
  Herz: 'h',
  Karo: 'k',
};

const RANK_MAP: Record<string, string> = {
  Neun: '9',
  Zehn: '10',
  Bube: 'B',
  Dame: 'D',
  Koenig: 'K',
  Ass: 'A',
};

// Eagerly import all card SVGs as URLs via Vite's glob import
const svgUrls = import.meta.glob('../assets/cards/*.svg', {
  eager: true,
  query: '?url',
  import: 'default',
}) as Record<string, string>;

export function cardSvgPath(suit: string, rank: string): string {
  const filename = `${SUIT_MAP[suit]}${RANK_MAP[rank]}.svg`;
  const url = svgUrls[`../assets/cards/${filename}`];
  if (!url) console.warn(`Card SVG not found: ${filename}`);
  return url ?? '';
}

import type React from 'react';

// Eagerly import all card SVGs as React components via vite-plugin-svgr
const svgComponents = import.meta.glob('../assets/cards/*.svg', {
  eager: true,
  query: '?react',
  import: 'default',
}) as Record<string, React.FC<React.SVGProps<SVGSVGElement>>>;

export function cardSvgComponent(suit: string, rank: string): React.FC<React.SVGProps<SVGSVGElement>> {
  const filename = `${SUIT_MAP[suit]}${RANK_MAP[rank]}.svg`;
  const component = svgComponents[`../assets/cards/${filename}`];
  if (!component) console.warn(`Card SVG component not found: ${filename}`);
  return component;
}
