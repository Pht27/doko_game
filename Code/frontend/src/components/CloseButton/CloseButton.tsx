interface CloseButtonProps {
  onClick: () => void;
  className?: string;
  ariaLabel?: string;
}

export function CloseButton({ onClick, className = '', ariaLabel = 'Schließen' }: CloseButtonProps) {
  return (
    <button
      onClick={onClick}
      className={`text-(--app-text-muted) hover:text-(--app-text) active:text-(--app-text-muted) text-xl leading-none transition-colors px-1 py-1 ${className}`}
      aria-label={ariaLabel}
    >
      ×
    </button>
  );
}
