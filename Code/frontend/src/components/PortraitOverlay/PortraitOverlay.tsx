import { useEffect, useRef, useState } from 'react';

interface PortraitOverlayProps {
  active: boolean;
}

export function PortraitOverlay({ active }: PortraitOverlayProps) {
  const [isPortrait, setIsPortrait] = useState(
    () => window.matchMedia('(orientation: portrait)').matches
  );
  const [minimized, setMinimized] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    const mq = window.matchMedia('(orientation: portrait)');
    const handler = (e: MediaQueryListEvent) => {
      setIsPortrait(e.matches);
      if (e.matches) setMinimized(false); // re-expand on re-enter portrait
    };
    mq.addEventListener('change', handler);
    return () => mq.removeEventListener('change', handler);
  }, []);

  // Auto-minimize after 3 s
  useEffect(() => {
    if (active && isPortrait && !minimized) {
      timerRef.current = setTimeout(() => setMinimized(true), 3000);
    }
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [active, isPortrait, minimized]);

  if (!active || !isPortrait) return null;

  if (minimized) {
    return (
      <button
        onClick={() => setMinimized(false)}
        className="fixed top-4 right-4 z-9999 flex items-center justify-center w-12 h-12 rounded-full bg-green-900/90 text-white shadow-lg animate-pulse"
        aria-label="Gerät drehen"
      >
        <RotateIcon />
      </button>
    );
  }

  return (
    <div className="fixed inset-0 z-9999 flex items-end justify-center pb-16">
      {/* Blurred backdrop */}
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => setMinimized(true)} />

      <div className="relative flex flex-col items-center gap-4 rounded-2xl bg-green-900 px-8 py-6 text-white shadow-2xl max-w-xs w-full mx-4">
        <RotateIcon className="w-14 h-14 opacity-90" />
        <p className="text-lg font-semibold tracking-wide text-center">Bitte Gerät drehen</p>
        <p className="text-sm opacity-70 text-center">Dieses Spiel benötigt Querformat</p>
        <button
          onClick={() => setMinimized(true)}
          className="mt-1 text-xs opacity-50 underline underline-offset-2"
        >
          Verstanden
        </button>
      </div>
    </div>
  );
}

function RotateIcon({ className = 'w-8 h-8' }: { className?: string }) {
  return (
    <svg
      className={className}
      viewBox="0 0 64 64"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <rect x="10" y="8" width="16" height="26" rx="2" />
      <path d="M30 18 C40 14 46 22 46 30" />
      <polyline points="42,30 46,34 50,30" />
      <rect x="26" y="38" width="26" height="16" rx="2" />
    </svg>
  );
}
