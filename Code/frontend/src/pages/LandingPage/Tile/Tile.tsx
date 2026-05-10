const SURFACE = 'rgba(255,255,255,0.05)';

export type Suit = {
  glyph: string; color: string; glow: string;
  openBg?: string; openBorder?: string;
};

export function Tile({
  suit, label, sub, expanded, onClick,
}: {
  suit: Suit; label: string; sub: string;
  expanded?: boolean; onClick?: () => void;
}) {
  const expandedBg     = suit.openBg     ?? SURFACE;
  const expandedBorder = suit.openBorder ?? 'rgba(255,255,255,0.2)';

  const bg          = expanded ? expandedBg : SURFACE;
  const borderColor = expanded ? expandedBorder : 'rgba(255,255,255,0.07)';
  const boxShadow   = expanded
    ? `0 0 0 1px ${expandedBorder}, 0 0 28px ${suit.glow}`
    : 'none';

  return (
    <button
      onClick={onClick}
      className="landing-tile"
      style={{
        aspectRatio: '1 / 1',
        borderRadius: 18,
        border: `1px solid ${borderColor}`,
        padding: 16,
        display: 'flex', flexDirection: 'column',
        alignItems: 'flex-start', justifyContent: 'space-between',
        color: '#fff', cursor: 'pointer', position: 'relative',
        fontFamily: 'inherit', textAlign: 'left', background: bg,
        boxShadow,
        transition: 'transform 0.15s, border-color 0.25s, background 0.25s, box-shadow 0.25s',
        WebkitTapHighlightColor: 'transparent',
      }}
    >
      {/* corner suit glyph */}
      <div style={{
        position: 'absolute', top: 10, left: 12,
        fontFamily: 'serif', lineHeight: 1,
        color: suit.color,
        opacity: expanded ? 0.95 : 0.65,
        fontSize: 14,
      }}>
        {suit.glyph}
      </div>

      {/* big center glyph */}
      <div style={{
        fontFamily: 'serif', fontSize: 60, lineHeight: 0.8,
        color: suit.color,
        opacity: expanded ? 1 : 0.5,
        textShadow: expanded ? `0 0 24px ${suit.glow}` : 'none',
        transition: 'opacity 0.25s, text-shadow 0.25s',
        alignSelf: 'flex-end',
        pointerEvents: 'none',
      }}>
        {suit.glyph}
      </div>

      {/* label block */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <div style={{ fontSize: 18, fontWeight: 700, letterSpacing: '-0.01em', color: '#fff' }}>
          {label}
        </div>
        <div style={{
          fontSize: 12, fontWeight: 500,
          color: expanded ? 'rgba(255,255,255,0.78)' : 'rgba(255,255,255,0.5)',
        }}>
          {sub}
        </div>
      </div>
    </button>
  );
}
