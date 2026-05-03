import { Outlet } from 'react-router-dom';
import { PortraitOverlay } from '@/components/PortraitOverlay/PortraitOverlay';
import { useOrientationLock } from '@/hooks/useOrientationLock';

export function GamingLayout() {
  const { lockFailed } = useOrientationLock();

  return (
    <>
      <PortraitOverlay active={lockFailed} />
      <Outlet />
    </>
  );
}
