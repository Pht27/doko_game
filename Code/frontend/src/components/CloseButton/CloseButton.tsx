interface CloseButtonProps {
  onClick: () => void;
  className?: string;
  ariaLabel?: string;
}

export function CloseButton({ onClick, className = '', ariaLabel = 'Schließen' }: CloseButtonProps) {
  return (
    <button
      onClick={onClick}
      className={`text-white/40 hover:text-white/70 active:text-white/30 text-xl leading-none transition-colors px-1 py-1 ${className}`}
      aria-label={ariaLabel}
    >
      ×
    </button>
  );
}
