import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import { MultiplayerBrowserPage } from '@/features/lobby/MultiplayerBrowserPage/MultiplayerBrowserPage';
import { loadLobbySession, saveLobbySession } from '@/hooks/useLobby';
import { getLobby, joinSeat } from '@/api/lobby';
import { t } from '@/utils/translations';
import type { LobbySession } from '@/hooks/useLobby';
import type { GameResultDto } from '@/types/api';

export function LobbyPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams, setSearchParams] = useSearchParams();

  // ?id=<lobbyId> — from invite link redirect or stored session redirect
  const inviteLobbyId = searchParams.get('id');
  // ?selected=<lobbyId> — currently open lobby in the detail panel
  const selectedLobbyId = searchParams.get('selected') ?? undefined;

  const [joinError, setJoinError] = useState<string | null>(null);
  const [joining, setJoining] = useState(false);

  // lastFinishedResult passed via router state when navigating back from a game
  const lastFinishedResult = (location.state as { lastFinishedResult?: GameResultDto | null; fromLobbyId?: string } | null)
    ?.lastFinishedResult ?? null;
  const fromLobbyId = (location.state as { fromLobbyId?: string } | null)?.fromLobbyId;

  // Auto-select the lobby we came from (so the detail panel opens directly)
  useEffect(() => {
    if (!fromLobbyId || selectedLobbyId) return;
    setSearchParams({ selected: fromLobbyId }, { replace: true });
  }, [fromLobbyId]);

  // Auto-join when arriving via invite URL without an existing session
  useEffect(() => {
    if (!inviteLobbyId) return;

    const stored = loadLobbySession(inviteLobbyId);
    if (stored) {
      // Already have a session for this lobby — just select it
      setSearchParams({ selected: inviteLobbyId }, { replace: true });
      return;
    }

    let cancelled = false;
    setJoinError(null);
    setJoining(true);

    async function autoJoin() {
      try {
        const lobbyView = await getLobby(inviteLobbyId!);
        if (cancelled) return;

        const seatIndex = lobbyView.seats.findIndex((occupied) => !occupied);
        if (seatIndex === -1) {
          if (!cancelled) setJoinError(t.lobbyFull);
          return;
        }

        const res = await joinSeat(inviteLobbyId!, seatIndex);
        if (cancelled) return;

        const session: LobbySession = {
          lobbyId: res.lobbyId,
          token: res.token,
          seatIndex: res.seatIndex,
        };
        saveLobbySession(session);
        setSearchParams({ selected: res.lobbyId }, { replace: true });
      } catch (e: unknown) {
        if (!cancelled) setJoinError(e instanceof Error ? e.message : String(e));
      } finally {
        if (!cancelled) setJoining(false);
      }
    }

    autoJoin();
    return () => { cancelled = true; };
  }, [inviteLobbyId]);

  function handleGameStarted(gameId: string, session: LobbySession) {
    const gameSession: LobbySession = { ...session, activeGameId: gameId };
    saveLobbySession(gameSession);
    navigate(`/game/${gameId}`);
  }

  if (joining || joinError) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        {joinError
          ? <p className="text-red-400 text-lg">{joinError}</p>
          : <p className="text-white/60 text-lg">{t.joiningLobby}</p>}
      </div>
    );
  }

  return (
    <MultiplayerBrowserPage
      selectedLobbyId={selectedLobbyId}
      onBack={() => navigate('/')}
      onSelectLobby={(lobbyId) =>
        lobbyId
          ? setSearchParams({ selected: lobbyId })
          : setSearchParams({})
      }
      onGameStarted={handleGameStarted}
      lastFinishedResult={selectedLobbyId === fromLobbyId ? lastFinishedResult : null}
    />
  );
}
