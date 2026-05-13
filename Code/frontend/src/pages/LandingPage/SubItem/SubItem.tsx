import './SubItem.css';

export function SubItem({ label, hint, onClick, disabled, hasDivider }: {
  label: string; hint: string; onClick?: () => void; disabled?: boolean; hasDivider?: boolean;
}) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`sub-item${hasDivider ? ' sub-item--divider' : ''}${disabled ? ' sub-item--disabled' : ''}`}
    >
      <div className="sub-item-content">
        <span className="sub-item-label">{label}</span>
        <span className="sub-item-hint">{hint}</span>
      </div>
    </button>
  );
}
