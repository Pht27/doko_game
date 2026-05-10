interface ExtraPointRowProps {
  name: string;
  icon?: string;
  count: number;
  onUpdateCount: (delta: number) => void;
  onRemove: () => void;
}

export function ExtraPointRow({ name, icon, count, onUpdateCount, onRemove }: ExtraPointRowProps) {
  return (
    <div className="tem-ep-row">
      {icon && (
        <span className="tem-ep-icon">{icon}</span>
      )}
      <span className="tem-ep-name">{name}</span>
      <div className="tem-ep-counter">
        <button
          className="tem-ep-count-btn"
          onClick={() => onUpdateCount(-1)}
          aria-label="Weniger"
        >
          −
        </button>
        <span className="tem-ep-count">{count}</span>
        <button
          className="tem-ep-count-btn"
          onClick={() => onUpdateCount(1)}
          aria-label="Mehr"
        >
          +
        </button>
      </div>
      <button
        className="tem-ep-remove"
        onClick={onRemove}
        aria-label="Entfernen"
      >
        ✕
      </button>
    </div>
  );
}
