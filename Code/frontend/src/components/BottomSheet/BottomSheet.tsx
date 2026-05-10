import { useEffect, useRef } from 'react';
import { CloseButton } from '@/components/CloseButton/CloseButton';

interface BottomSheetProps {
  title: string;
  onClose: () => void;
  children: React.ReactNode;
  maxHeight?: string;
  className?: string;
}

export function BottomSheet({ title, onClose, children, maxHeight = '88vh', className = '' }: BottomSheetProps) {
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  useEffect(() => {
    history.pushState({ modal: true }, '');
    const handler = () => onCloseRef.current();
    window.addEventListener('popstate', handler);
    return () => window.removeEventListener('popstate', handler);
  }, []);

  const handleBackdrop = () => history.back();

  return (
    <div
      className="fixed inset-0 flex items-end justify-center z-[200]"
      style={{ background: 'rgba(0,0,0,0.65)', paddingBottom: 'env(safe-area-inset-bottom, 0)' }}
      onClick={handleBackdrop}
    >
      <div
        className={`w-full flex flex-col overflow-hidden ${className}`}
        style={{
          maxWidth: 480,
          maxHeight,
          background: '#22223a',
          borderRadius: '16px 16px 0 0',
          borderTop: '1px solid rgba(255,255,255,0.08)',
        }}
        onClick={(e) => e.stopPropagation()}
      >
        <div
          className="flex items-center justify-between flex-shrink-0"
          style={{ padding: '16px 20px 12px', borderBottom: '1px solid rgba(255,255,255,0.08)' }}
        >
          <h2 style={{ fontSize: '1rem', fontWeight: 700, margin: 0, color: '#eeeeee' }}>{title}</h2>
          <CloseButton onClick={handleBackdrop} />
        </div>
        {children}
      </div>
    </div>
  );
}
