import { useEffect, useState } from 'react';
import { Outlet } from 'react-router-dom';
import { PortraitOverlay } from '@/components/PortraitOverlay/PortraitOverlay';

export function GamingLayout() {
  const [landscapeLockFailed, setLandscapeLockFailed] = useState(false);

  useEffect(() => {
    if (!screen.orientation?.lock) {
      setLandscapeLockFailed(true);
      return;
    }
    setLandscapeLockFailed(false);
    screen.orientation.lock('landscape')
      .then(() => setLandscapeLockFailed(false))
      .catch(() => setLandscapeLockFailed(true));
  }, []);

  // Re-apply after fullscreen transitions (PWA fullscreen resets the lock)
  useEffect(() => {
    function reapplyLock() {
      if (!document.fullscreenElement) return;
      screen.orientation?.lock?.('landscape')
        ?.then(() => setLandscapeLockFailed(false))
        ?.catch(() => setLandscapeLockFailed(true));
    }
    document.addEventListener('fullscreenchange', reapplyLock);
    return () => document.removeEventListener('fullscreenchange', reapplyLock);
  }, []);

  return (
    <>
      <PortraitOverlay requireLandscape={landscapeLockFailed} />
      <Outlet />
    </>
  );
}
