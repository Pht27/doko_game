import { useEffect } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import { loadLobbySession, loadAnySession } from './hooks/useLobby';

export function Root() {
  const navigate = useNavigate();

  // Handle legacy invite URL (?lobby=<id>) and stored session redirects
  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const lobbyId = params.get('lobby');

    if (lobbyId) {
      const stored = loadLobbySession(lobbyId);
      if (stored?.activeGameId) {
        navigate(`/game/${stored.activeGameId}`, { replace: true });
      } else {
        navigate(`/lobby?id=${lobbyId}`, { replace: true });
      }
      return;
    }

    const anySession = loadAnySession();
    if (anySession?.activeGameId) {
      navigate(`/game/${anySession.activeGameId}`, { replace: true });
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // PWA fullscreen: enter on first interaction when running as installed PWA
  useEffect(() => {
    const isPwa =
      window.matchMedia('(display-mode: standalone)').matches ||
      window.matchMedia('(display-mode: fullscreen)').matches ||
      (window.navigator as { standalone?: boolean }).standalone === true;

    if (!isPwa) return;

    function enterFullscreen() {
      if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().catch(() => {});
      }
    }

    window.addEventListener('click', enterFullscreen, { once: true });
    window.addEventListener('touchstart', enterFullscreen, { once: true });
    return () => {
      window.removeEventListener('click', enterFullscreen);
      window.removeEventListener('touchstart', enterFullscreen);
    };
  }, []);

  return <Outlet />;
}
