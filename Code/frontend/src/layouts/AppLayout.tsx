import { useEffect } from 'react';
import { Outlet } from 'react-router-dom';

export function AppLayout() {
  useEffect(() => {
    screen.orientation?.lock?.('portrait')?.catch(() => {});
  }, []);

  return <Outlet />;
}
