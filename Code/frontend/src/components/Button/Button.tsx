type ButtonVariant = 'primary' | 'secondary' | 'link';

interface ButtonProps {
  variant?: ButtonVariant;
  onClick?: () => void;
  disabled?: boolean;
  children: React.ReactNode;
  className?: string;
  type?: 'button' | 'submit';
}

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    'bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 text-white font-semibold rounded-xl px-4 py-2.5 transition-colors disabled:opacity-40 disabled:cursor-not-allowed',
  secondary:
    'bg-white/8 hover:bg-white/12 active:bg-white/6 text-white/80 rounded-xl px-4 py-2.5 transition-colors',
  link: 'text-white/40 hover:text-white/60 transition-colors',
};

export function Button({
  variant = 'primary',
  onClick,
  disabled,
  children,
  className,
  type = 'button',
}: ButtonProps) {
  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={[variantClasses[variant], className].filter(Boolean).join(' ')}
    >
      {children}
    </button>
  );
}
