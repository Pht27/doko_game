interface BackButtonProps {
  onClick: () => void;
  className?: string;
}

export function BackButton({ onClick, className = '' }: BackButtonProps) {
  return (
    <button
      onClick={onClick}
      className={`text-white/60 hover:text-white active:text-white/40 text-xl leading-none transition-colors px-1 py-1 ${className}`}
      aria-label="Zurück"
    >
      ←
    </button>
  );
}
