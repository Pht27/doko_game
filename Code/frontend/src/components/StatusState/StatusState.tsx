import { t } from '@/utils/translations';
import './StatusState.css';

type StatusStateType = 'loading' | 'error' | 'empty';

interface StatusStateProps {
  type: StatusStateType;
  message?: string;
  className?: string;
  dashed?: boolean;
}

const defaultMessages = (): Record<StatusStateType, string> => ({
  loading: t.loading,
  error: t.statusError,
  empty: t.statusEmpty,
});

export function StatusState({ type, message, className = '', dashed = false }: StatusStateProps) {
  const text = message ?? defaultMessages()[type];

  if (type === 'loading') {
    return (
      <div className={`ss-root ss-loading ${className}`}>
        <div className="ss-spinner" aria-hidden />
        {text && <span className="ss-text">{text}</span>}
      </div>
    );
  }

  if (type === 'error') {
    return (
      <div className={`ss-root ss-error ${className}`}>
        <span className="ss-icon" aria-hidden>⚠</span>
        <span className="ss-text">{text}</span>
      </div>
    );
  }

  return (
    <div className={`ss-root ss-empty${dashed ? ' ss-empty--dashed' : ''} ${className}`}>
      <span className="ss-text">{text}</span>
    </div>
  );
}
