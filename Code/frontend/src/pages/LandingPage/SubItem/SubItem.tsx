export function SubItem({ label, hint, onClick, disabled, hasDivider }: {
  label: string; hint: string; onClick?: () => void; disabled?: boolean; hasDivider?: boolean;
}) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      style={{
        background: 'transparent', border: 'none',
        borderBottom: hasDivider ? '1px solid rgba(255,255,255,0.08)' : 'none',
        padding: '11px 14px',
        color: disabled ? 'rgba(255,255,255,0.25)' : '#eee',
        display: 'flex', alignItems: 'center',
        borderRadius: hasDivider ? 0 : 10,
        cursor: disabled ? 'not-allowed' : 'pointer',
        fontFamily: 'inherit', textAlign: 'left', width: '100%',
      }}
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: 2, alignItems: 'flex-start' }}>
        <span style={{ fontSize: 14, fontWeight: 600, color: 'inherit' }}>{label}</span>
        <span style={{ fontSize: 11, color: 'rgba(255,255,255,0.5)' }}>{hint}</span>
      </div>
    </button>
  );
}
