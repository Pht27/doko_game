import { useEffect, useRef } from 'react';
import { CloseButton } from '@/components/CloseButton/CloseButton';
import './BottomSheet.css';

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
      className="bottom-sheet-backdrop fixed inset-0 flex items-end justify-center z-[200]"
      onClick={handleBackdrop}
    >
      <div
        className={`bottom-sheet-panel w-full flex flex-col overflow-hidden ${className}`}
        style={{ maxHeight }}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="bottom-sheet-header flex items-center justify-between flex-shrink-0">
          <h2 className="bottom-sheet-title">{title}</h2>
          <CloseButton onClick={handleBackdrop} />
        </div>
        {children}
      </div>
    </div>
  );
}
