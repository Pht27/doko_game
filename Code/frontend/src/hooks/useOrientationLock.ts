import { useEffect, useState } from 'react';

export function useOrientationLock() {
  const [lockFailed, setLockFailed] = useState(false);

  useEffect(() => {
    if (!screen.orientation?.lock) {
      setLockFailed(true);
      return;
    }
    screen.orientation.lock('landscape')
      .then(() => setLockFailed(false))
      .catch(() => setLockFailed(true));

    return () => {
      screen.orientation?.unlock?.();
    };
  }, []);

  // Re-apply after fullscreen transitions (PWA fullscreen resets the lock)
  useEffect(() => {
    function reapplyLock() {
      if (!document.fullscreenElement) return;
      screen.orientation?.lock?.('landscape')
        ?.then(() => setLockFailed(false))
        ?.catch(() => setLockFailed(true));
    }
    document.addEventListener('fullscreenchange', reapplyLock);
    return () => document.removeEventListener('fullscreenchange', reapplyLock);
  }, []);

  return { lockFailed };
}
