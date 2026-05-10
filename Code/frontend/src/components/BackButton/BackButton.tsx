import { useNavigate } from 'react-router-dom';
import { t } from '@/utils/translations';

interface BackButtonProps {
  to?: string | number;
  onClick?: () => void;
  className?: string;
}

export function BackButton({ to, onClick, className = '' }: BackButtonProps) {
  const navigate = useNavigate();

  const handleClick = () => {
    if (onClick) {
      onClick();
    } else if (to !== undefined) {
      navigate(to as never);
    }
  };

  return (
    <button
      onClick={handleClick}
      className={`text-white/60 hover:text-white active:text-white/40 text-xl leading-none transition-colors px-1 py-1 ${className}`}
      aria-label={t.backAriaLabel}
    >
      ←
    </button>
  );
}
