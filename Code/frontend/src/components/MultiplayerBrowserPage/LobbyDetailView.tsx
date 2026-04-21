import { useState, useEffect } from 'react';
import { t } from '../../translations';
import { leaveLobby, joinSeat, swapSeat, getLobby, voteReady, withdrawReady, addOpa, removeOpa, getScenarios, setScenario } from '../../api/lobby';
import {
  useLobby,
  loadLobbySession,
  loadAnySession,
  saveLobbySession,
  clearLobbySession,
} from '../../hooks/useLobby';
import { ResultScreen } from '../ResultScreen/ResultScreen';
import { ReadyVoteButton } from '../shared/ReadyVoteButton';
import type { LobbySession } from '../../hooks/useLobby';
import type { GameResultDto } from '../../types/api';

interface LobbyDetailViewProps {
  lobbyId: string;
  onGameStarted: (gameId: string, session: LobbySession) => void;
  onLobbyClosed: () => void;
  lastFinishedResult?: GameResultDto | null;
}

export function LobbyDetailView({ lobbyId, onGameStarted, onLobbyClosed, lastFinishedResult }: LobbyDetailViewProps) {
  const [session, setSession] = useState<LobbySession | null>(() => loadLobbySession(lobbyId));

  const { seats, opaSeats, gameId, isStarted, lobbyClosed, startVoteCount, selectedScenario, error } = useLobby(session, lobbyId);

  const [copied, setCopied] = useState(false);
  const [hasVoted, setHasVoted] = useState(false);
  const [voting, setVoting] = useState(false);
  const [leaving, setLeaving] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const [busySeat, setBusySeat] = useState<number | null>(null); // index being joined/swapped
  const [actionError, setActionError] = useState<string | null>(null);
  const [showScenarioPicker, setShowScenarioPicker] = useState(false);
  const [availableScenarios, setAvailableScenarios] = useState<string[]>([]);
  const [settingScenario, setSettingScenario] = useState(false);

  const inviteUrl = `${window.location.origin}${window.location.pathname}?lobby=${lobbyId}`;

  useEffect(() => {
    if (gameId && session) {
      onGameStarted(gameId, session);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [gameId]);

  useEffect(() => {
    if (lobbyClosed) {
      clearLobbySession();
      setSession(null);
      onLobbyClosed();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lobbyClosed]);

  async function copyLink() {
    await navigator.clipboard.writeText(inviteUrl);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  /** Join an empty seat for the first time (no existing session in this lobby). */
  async function doJoin(targetSeat: number) {
    setBusySeat(targetSeat);
    setActionError(null);
    try {
      const res = await joinSeat(lobbyId, targetSeat);
      const newSession: LobbySession = {
        lobbyId: res.lobbyId,
        token: res.token,
        
        seatIndex: res.seatIndex,
      };
      saveLobbySession(newSession);
      setSession(newSession);

      // If a game is already running, navigate straight to it
      const lobby = await getLobby(lobbyId);
      if (lobby.isStarted && lobby.activeGameId) {
        clearLobbySession();
        onGameStarted(lobby.activeGameId, newSession);
      }
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    } finally {
      setBusySeat(null);
    }
  }

  /** Leave current seat and immediately occupy a different one in the same lobby. */
  async function doSwap(targetSeat: number) {
    if (!session) return;
    setBusySeat(targetSeat);
    setActionError(null);
    try {
      const res = await swapSeat(session.token, lobbyId, targetSeat);
      const newSession: LobbySession = {
        lobbyId: res.lobbyId,
        token: res.token,
        seatIndex: res.seatIndex,
      };
      saveLobbySession(newSession);
      setSession(newSession);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    } finally {
      setBusySeat(null);
    }
  }

  async function handleVote() {
    if (!session || voting) return;
    setVoting(true);
    setActionError(null);
    try {
      await voteReady(session.token, lobbyId);
      setHasVoted(true);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    } finally {
      setVoting(false);
    }
  }

  async function handleWithdraw() {
    if (!session || voting) return;
    setVoting(true);
    setActionError(null);
    try {
      await withdrawReady(session.token, lobbyId);
      setHasVoted(false);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    } finally {
      setVoting(false);
    }
  }

  async function doAddOpa(targetSeat: number) {
    if (!session) return;
    setActionError(null);
    try {
      await addOpa(session.token, lobbyId, targetSeat);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function doRemoveOpa(opaSeatIndex: number) {
    if (!session) return;
    setActionError(null);
    try {
      await removeOpa(session.token, lobbyId, opaSeatIndex);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function openScenarioPicker() {
    setActionError(null);
    try {
      const res = await getScenarios();
      setAvailableScenarios(res.scenarios);
      setShowScenarioPicker(true);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function handleSelectScenario(name: string | null) {
    if (!session) return;
    setSettingScenario(true);
    setActionError(null);
    try {
      await setScenario(session.token, lobbyId, name);
      setShowScenarioPicker(false);
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    } finally {
      setSettingScenario(false);
    }
  }

  async function handleLeaveSeat() {
    if (!session) return;
    setLeaving(true);
    setActionError(null);
    try {
      await leaveLobby(session.token, lobbyId);
      clearLobbySession();
      setSession(null);
      onLobbyClosed();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
      setLeaving(false);
    }
  }

  const filledCount = seats.filter(Boolean).length;
  const isMyLobby = session !== null;
  // True if user has a session stored for a *different* lobby — blocks joining seats here
  const isInAnotherLobby = !isMyLobby && loadAnySession() !== null;
  // When a game is running, seat swapping is not allowed — only joining empty seats
  const canSwapSeats = isMyLobby && !isStarted;

  return (
    <div className="flex flex-col h-full gap-3 p-4 overflow-y-auto">
      {/* Seat grid — 2×2: top-left=0, top-right=3, bottom-left=1, bottom-right=2 */}
      <div className="grid grid-cols-2 gap-2 shrink-0">
        {([0, 3, 1, 2] as const).map((i) => {
          const occupied = seats[i];
          const isOpa = opaSeats.includes(i);
          const isMe = isMyLobby && session!.seatIndex === i;
          const isBusy = busySeat === i;
          const canInteract = !occupied && !isInAnotherLobby && !isMe;
          const canAddOpa = isMyLobby && !occupied && !isStarted;
          const canRemoveOpa = isMyLobby && isOpa && !isStarted;

          function handleClick() {
            if (!canInteract || isBusy) return;
            if (canSwapSeats) doSwap(i);
            else doJoin(i);
          }

          return (
            <div
              key={i}
              className={`flex items-center gap-2 px-3 py-3 rounded-xl transition-colors ${
                isMe
                  ? 'bg-indigo-600/50 text-white ring-1 ring-indigo-400'
                  : isOpa
                    ? 'bg-white/15 text-white'
                    : occupied
                      ? 'bg-white/15 text-white'
                      : canInteract
                        ? 'bg-white/5 text-white/50 hover:bg-white/10 hover:text-white/80 cursor-pointer'
                        : 'bg-white/5 text-white/20'
              }`}
              onClick={!isOpa ? handleClick : undefined}
              role={!isOpa && canInteract ? 'button' : undefined}
            >
              <div
                className={`w-2.5 h-2.5 rounded-full shrink-0 ${
                  occupied ? 'bg-green-400' : 'bg-white/20'
                }`}
              />
              <span className="text-sm font-medium truncate flex-1">
                {isBusy ? (
                  t.joiningLobby
                ) : isOpa ? (
                  <>
                    Opa
                    <span className="text-white/40 text-xs ml-1">🤖</span>
                  </>
                ) : occupied ? (
                  <>
                    {t.playerSlot(i)}
                    {isMe && <span className="text-white/50 text-xs">{t.youSuffix}</span>}
                  </>
                ) : (
                  t.seatLabel(i)
                )}
              </span>
              {canRemoveOpa && (
                <button
                  onClick={(e) => { e.stopPropagation(); doRemoveOpa(i); }}
                  className="ml-auto text-red-400 hover:text-red-300 text-xs px-1 shrink-0"
                  title="Opa entfernen"
                >
                  ✕
                </button>
              )}
              {canAddOpa && (
                <button
                  onClick={(e) => { e.stopPropagation(); doAddOpa(i); }}
                  className="ml-auto text-white/30 hover:text-white/60 text-xs px-1 shrink-0"
                  title="Opa hinzufügen"
                >
                  🤖
                </button>
              )}
            </div>
          );
        })}
      </div>

      <p className="text-white/40 text-xs shrink-0">
        {t.playerCount(filledCount, 4)}
        {isStarted
          ? <span className="text-orange-400"> · Spiel läuft</span>
          : filledCount < 4 && ` · ${t.waitingForPlayers}`}
      </p>

      {/* Invite link */}
      <div className="flex flex-col gap-1.5 shrink-0">
        <span className="text-white/40 text-xs uppercase tracking-wider">{t.inviteLink}</span>
        <div className="flex gap-2">
          <div className="flex-1 bg-white/10 rounded-xl px-3 py-2 text-white/50 text-xs font-mono truncate">
            {inviteUrl}
          </div>
          <button
            onClick={copyLink}
            className="px-3 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 text-white text-xs font-semibold transition-colors shrink-0"
          >
            {copied ? t.linkCopied : t.copyLink}
          </button>
        </div>
      </div>

      {/* Actions — only shown when user has a seat in this lobby */}
      {isMyLobby && (
        <div className="flex flex-col gap-2 shrink-0">
          {/* Scenario picker */}
          {!isStarted && (
            <div className="flex flex-col gap-1">
              <div className="flex items-center gap-2">
                <span className="text-white/40 text-xs uppercase tracking-wider flex-1">Szenario</span>
                <button
                  onClick={openScenarioPicker}
                  className="text-xs text-indigo-300 hover:text-indigo-200 transition-colors"
                >
                  {selectedScenario ? 'Ändern' : 'Laden'}
                </button>
                {selectedScenario && (
                  <button
                    onClick={() => handleSelectScenario(null)}
                    className="text-xs text-red-400 hover:text-red-300 transition-colors"
                  >
                    ✕
                  </button>
                )}
              </div>
              {selectedScenario && (
                <div className="bg-indigo-600/20 border border-indigo-500/30 rounded-lg px-3 py-2 text-indigo-200 text-xs font-medium">
                  {selectedScenario}
                </div>
              )}
            </div>
          )}

          {/* Scenario modal */}
          {showScenarioPicker && (
            <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/60 p-4" onClick={() => setShowScenarioPicker(false)}>
              <div className="bg-gray-900 rounded-2xl w-full max-w-sm p-4 flex flex-col gap-2" onClick={(e) => e.stopPropagation()}>
                <p className="text-white font-semibold text-sm mb-1">Szenario auswählen</p>
                <button
                  disabled={settingScenario}
                  onClick={() => handleSelectScenario(null)}
                  className={`w-full text-left px-3 py-2.5 rounded-xl text-sm transition-colors ${
                    !selectedScenario
                      ? 'bg-indigo-600/40 text-white'
                      : 'text-white/50 hover:bg-white/10 hover:text-white'
                  }`}
                >
                  Kein Szenario (Zufällig)
                </button>
                {availableScenarios.map((name) => (
                  <button
                    key={name}
                    disabled={settingScenario}
                    onClick={() => handleSelectScenario(name)}
                    className={`w-full text-left px-3 py-2.5 rounded-xl text-sm transition-colors ${
                      selectedScenario === name
                        ? 'bg-indigo-600/40 text-white'
                        : 'text-white/70 hover:bg-white/10 hover:text-white'
                    }`}
                  >
                    {name}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {isMyLobby && (
        <div className="flex flex-col gap-2 shrink-0">
          {/* Match history button */}
          {lastFinishedResult && (
            <button
              onClick={() => setShowHistory(true)}
              className="w-full py-2.5 text-xs font-bold uppercase tracking-wider rounded-lg bg-white/10 hover:bg-white/20 active:bg-white/5 text-white/70 transition-colors"
            >
              {t.spielverlauf}
            </button>
          )}
          {/* Leave + Ready row */}
          {!isStarted && (
            <div className="flex gap-2">
              <button
                onClick={handleLeaveSeat}
                disabled={leaving}
                className="flex-1 py-2.5 text-xs font-bold uppercase tracking-wider rounded-lg bg-red-900/50 hover:bg-red-800/70 active:bg-red-900/80 text-red-300 transition-colors disabled:opacity-40"
              >
                {leaving ? t.loading : t.leaveSeat}
              </button>
              <ReadyVoteButton
                hasVoted={hasVoted}
                voteCount={startVoteCount}
                disabled={filledCount < 4 || voting}
                onClick={hasVoted ? handleWithdraw : handleVote}
                className="flex-1"
              />
            </div>
          )}
          {/* Game running: only show leave button */}
          {isStarted && (
            <button
              onClick={handleLeaveSeat}
              disabled={leaving}
              className="w-full py-2.5 text-xs font-bold uppercase tracking-wider rounded-lg bg-red-900/50 hover:bg-red-800/70 active:bg-red-900/80 text-red-300 transition-colors disabled:opacity-40"
            >
              {leaving ? t.loading : t.leaveSeat}
            </button>
          )}
        </div>
      )}

      {(error || actionError) && (
        <p className="text-red-400 text-xs text-center shrink-0">{error ?? actionError}</p>
      )}

      {showHistory && lastFinishedResult && (
        <ResultScreen
          result={lastFinishedResult}
          onNewGame={() => setShowHistory(false)}
          viewOnly
        />
      )}
    </div>
  );
}
