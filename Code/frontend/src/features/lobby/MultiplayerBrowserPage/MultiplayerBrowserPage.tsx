import { useState, useEffect, useCallback } from 'react';
import { t } from '@/utils/translations';
import { BackButton } from '@/components/BackButton/BackButton';
import { listLobbies, createLobby, leaveLobby } from '@/api/lobby';
import { saveLobbySession, clearLobbySession, loadAnySession } from '@/hooks/useLobby';
import { LobbyDetailView } from './LobbyDetailView';
import type { LobbyListItemResponse } from '@/api/lobby';
import type { LobbySession } from '@/hooks/useLobby';
import type { GameResultDto } from '@/types/api';

interface MultiplayerBrowserPageProps {
  selectedLobbyId?: string;
  onBack: () => void;
  onSelectLobby: (lobbyId: string) => void;
  onGameStarted: (gameId: string, session: LobbySession) => void;
  lastFinishedResult?: GameResultDto | null;
}

export function MultiplayerBrowserPage({
  selectedLobbyId,
  onBack,
  onSelectLobby,
  onGameStarted,
  lastFinishedResult,
}: MultiplayerBrowserPageProps) {
  const [lobbies, setLobbies] = useState<LobbyListItemResponse[]>([]);
  const [creating, setCreating] = useState(false);
  const [hasFetchedLobbies, setHasFetchedLobbies] = useState(false);

  const fetchLobbies = useCallback(async () => {
    try {
      const result = await listLobbies();
      setLobbies(result);
      setHasFetchedLobbies(true);
    } catch {
      // Non-fatal — keep showing last known list
    }
  }, []);

  // Initial fetch + polling every 3 s
  useEffect(() => {
    fetchLobbies();
    const id = setInterval(fetchLobbies, 3000);
    return () => clearInterval(id);
  }, [fetchLobbies]);

  // Close detail view if the selected lobby disappears from the list
  useEffect(() => {
    if (!hasFetchedLobbies || !selectedLobbyId) return;
    if (!lobbies.some((l) => l.lobbyId === selectedLobbyId)) {
      handleLobbyClosed();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lobbies, selectedLobbyId, hasFetchedLobbies]);

  async function handleCreateLobby() {
    setCreating(true);
    try {
      // Leave any existing seat before creating a new lobby
      const existing = loadAnySession();
      if (existing) {
        try {
          await leaveLobby(existing.token, existing.lobbyId);
        } catch {
          // Best-effort: lobby may already be gone
        }
        clearLobbySession();
      }

      const res = await createLobby();
      const session: LobbySession = {
        lobbyId: res.lobbyId,
        token: res.token,
        
        seatIndex: res.seatIndex,
      };
      saveLobbySession(session);
      // Refresh list before selecting so the lobby is already in lobbies
      // when the "disappeared from list" effect runs.
      await fetchLobbies();
      onSelectLobby(res.lobbyId);
    } catch (e) {
      console.error('Failed to create lobby', e);
    } finally {
      setCreating(false);
    }
  }

  function handleLobbyClosed() {
    // Session is already cleared by LobbyDetailView before this is called
    onSelectLobby('');
    fetchLobbies();
  }

  return (
    <div className="w-full h-full flex flex-col">
      {/* Back button */}
      <div className="px-4 pt-3 pb-1 shrink-0">
        <BackButton onClick={onBack} />
      </div>

      {/* Two-panel layout */}
      <div className="flex flex-1 min-h-0 gap-0">
        {/* Left panel — lobby list */}
        <div className="w-[30%] flex flex-col border-r border-white/10">
          {/* Scrollable list */}
          <div className="flex-1 overflow-y-auto px-2 py-2 flex flex-col gap-1">
            {lobbies.length === 0 ? (
              <p className="text-white/30 text-sm text-center py-4">{t.noLobbiesAvailable}</p>
            ) : (
              lobbies.map((lobby) => {
                const filledCount = lobby.seats.filter(Boolean).length;
                const isSelected = lobby.lobbyId === selectedLobbyId;
                return (
                  <button
                    key={lobby.lobbyId}
                    onClick={() => onSelectLobby(lobby.lobbyId)}
                    className={`w-full text-left px-3 py-2.5 rounded-xl transition-colors ${
                      isSelected
                        ? 'bg-indigo-600 text-white'
                        : 'bg-white/5 text-white/70 hover:bg-white/10 hover:text-white'
                    }`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <span className="text-sm font-medium truncate">
                        Lobby
                      </span>
                      <div className="flex items-center gap-1.5 shrink-0">
                        {lobby.isStarted && (
                          <span className={`text-xs font-medium ${isSelected ? 'text-orange-300' : 'text-orange-400'}`}>
                            Spiel läuft
                          </span>
                        )}
                        <span className={`text-xs ${isSelected ? 'text-white/80' : 'text-white/40'}`}>
                          {filledCount}/4
                        </span>
                      </div>
                    </div>
                    {/* Seat indicator dots */}
                    <div className="flex gap-1 mt-1">
                      {lobby.seats.map((occupied, i) => (
                        <div
                          key={i}
                          className={`w-2 h-2 rounded-full ${
                            occupied
                              ? isSelected ? 'bg-white' : 'bg-green-400'
                              : isSelected ? 'bg-white/30' : 'bg-white/15'
                          }`}
                        />
                      ))}
                    </div>
                  </button>
                );
              })
            )}
          </div>

          {/* Create lobby button */}
          <div className="px-2 py-2 shrink-0 border-t border-white/10">
            <button
              onClick={handleCreateLobby}
              disabled={creating}
              className="w-full py-2.5 text-sm font-semibold rounded-xl bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 text-white transition-colors disabled:opacity-50"
            >
              {creating ? t.loading : t.createLobby}
            </button>
          </div>
        </div>

        {/* Right panel — lobby detail */}
        <div className="flex-1 min-w-0">
          {selectedLobbyId ? (
            <LobbyDetailView
              key={selectedLobbyId}
              lobbyId={selectedLobbyId}
              onGameStarted={onGameStarted}
              onLobbyClosed={handleLobbyClosed}
              lastFinishedResult={lastFinishedResult}
            />
          ) : (
            <div className="h-full flex items-center justify-center text-white/30 text-sm">
              {t.noLobbiesAvailable}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
