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

export function cardSvgPath(suit: string, rank: string): string {
  return `/src/assets/cards/${SUIT_MAP[suit]}${RANK_MAP[rank]}.svg`;
}
