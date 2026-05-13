export type SuitKey = 'kreuz' | 'pik' | 'herz' | 'karo';

const GLYPHS: Record<SuitKey, string> = {
  kreuz: '♣',
  pik:   '♠',
  herz:  '♥',
  karo:  '♦',
};

export function Tile({
  suit, label, sub, expanded, onClick,
}: {
  suit: SuitKey; label: string; sub: string;
  expanded?: boolean; onClick?: () => void;
}) {
  const glyph = GLYPHS[suit];

  return (
    <button
      onClick={onClick}
      className={`landing-tile tile--${suit}${expanded ? ' expanded' : ''}`}
    >
      <div className="tile-corner-glyph">{glyph}</div>
      <div className="tile-center-glyph">{glyph}</div>
      <div className="tile-label-block">
        <div className="tile-label">{label}</div>
        <div className="tile-sub">{sub}</div>
      </div>
    </button>
  );
}
