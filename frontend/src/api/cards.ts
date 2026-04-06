const SUIT_MAP: Record<string, string> = {
  Kreuz: 'kr',
  Pik: 'p',
  Herz: 'h',
  Karo: 'k',
};

const RANK_MAP: Record<string, string> = {
  Nine: '9',
  Ten: '10',
  Jack: 'B',
  Queen: 'D',
  King: 'K',
  Ace: 'A',
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
