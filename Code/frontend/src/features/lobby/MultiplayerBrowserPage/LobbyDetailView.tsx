import { useState, useEffect, useRef } from 'react';
import { t } from '@/utils/translations';
import { leaveLobby, joinSeat, swapSeat, getLobby, voteReady, withdrawReady, addOpa, removeOpa, getScenarios, setScenario, setLobbyPlayerName } from '@/api/lobby';
import {
  useLobby,
  loadLobbySession,
  loadAnySession,
  saveLobbySession,
  clearLobbySession,
} from '@/hooks/useLobby';
import { useSetPlayerNames } from '@/context/PlayerNamesContext';
import { ResultScreen } from '@/features/game/ResultScreen/ResultScreen';
import { ReadyVoteButton } from '@/features/game/shared/ReadyVoteButton';
import type { LobbySession } from '@/hooks/useLobby';
import type { GameResultDto } from '@/types/api';

interface LobbyDetailViewProps {
  lobbyId: string;
  onGameStarted: (gameId: string, session: LobbySession) => void;
  onLobbyClosed: () => void;
  lastFinishedResult?: GameResultDto | null;
}

export function LobbyDetailView({ lobbyId, onGameStarted, onLobbyClosed, lastFinishedResult }: LobbyDetailViewProps) {
  const [session, setSession] = useState<LobbySession | null>(() => loadLobbySession(lobbyId));

  const { seats, opaSeats, playerNames, gameId, isStarted, lobbyClosed, startVoteCount, readySeats, selectedScenario, error } = useLobby(session, lobbyId);
  const setPlayerNamesCtx = useSetPlayerNames();

  const [copied, setCopied] = useState(false);
  const [isEditingName, setIsEditingName] = useState(false);
  const [nameInput, setNameInput] = useState('');
  const nameInputRef = useRef<HTMLInputElement>(null);
  const [hasVoted, setHasVoted] = useState(false);
  const [voting, setVoting] = useState(false);
  const [leaving, setLeaving] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const [busySeat, setBusySeat] = useState<number | null>(null); // index being joined/swapped
  const [actionError, setActionError] = useState<string | null>(null);
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

  // Sync lobby player names into the app-level context so they persist into the game
  useEffect(() => {
    setPlayerNamesCtx(playerNames);
  }, [playerNames, setPlayerNamesCtx]);

  function startEditingName() {
    if (!session) return;
    const current = playerNames[session.seatIndex] ?? '';
    setNameInput(current);
    setIsEditingName(true);
    setTimeout(() => nameInputRef.current?.focus(), 0);
  }

  async function submitName() {
    if (!session) return;
    setIsEditingName(false);
    const trimmed = nameInput.trim() || null;
    try {
      await setLobbyPlayerName(session.token, lobbyId, trimmed);
    } catch {
      // best-effort: SignalR will update the name for everyone including us
    }
  }

  function handleNameKeyDown(e: React.KeyboardEvent) {
    if (e.key === 'Enter') submitName();
    if (e.key === 'Escape') setIsEditingName(false);
  }

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

  useEffect(() => {
    getScenarios()
      .then((res) => setAvailableScenarios(res.scenarios))
      .catch((e) => setActionError(e instanceof Error ? e.message : String(e)));
  }, []);

  async function handleSelectScenario(name: string | null) {
    if (!session) return;
    setSettingScenario(true);
    setActionError(null);
    try {
      await setScenario(session.token, lobbyId, name);
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
          const isReady = readySeats.includes(i);
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
              <span className="text-sm font-medium truncate flex-1 min-w-0">
                {isBusy ? (
                  t.joiningLobby
                ) : isOpa ? (
                  <>
                    Opa
                    <span className="text-white/40 text-xs ml-1">🤖</span>
                  </>
                ) : occupied ? (
                  isMe && isEditingName ? (
                    <input
                      ref={nameInputRef}
                      value={nameInput}
                      onChange={(e) => setNameInput(e.target.value)}
                      onBlur={submitName}
                      onKeyDown={handleNameKeyDown}
                      maxLength={16}
                      placeholder={t.playerSlot(i)}
                      className="bg-transparent border-b border-white/40 outline-none text-white text-sm w-full"
                      onClick={(e) => e.stopPropagation()}
                    />
                  ) : (
                    <>
                      <span className="truncate">{playerNames[i] ?? t.playerSlot(i)}</span>
                      {isMe && <span className="text-white/50 text-xs shrink-0">{t.youSuffix}</span>}
                    </>
                  )
                ) : (
                  t.seatLabel(i)
                )}
              </span>
              {isMe && !isEditingName && !canRemoveOpa && !canAddOpa && (
                <button
                  onClick={(e) => { e.stopPropagation(); startEditingName(); }}
                  className="ml-auto text-white/30 hover:text-white/70 text-xs px-1 shrink-0"
                  title="Namen ändern"
                >
                  ✏️
                </button>
              )}
              {isReady && !isMe && !canRemoveOpa && !canAddOpa && (
                <span className="ml-auto text-green-400 text-sm shrink-0" title="Bereit">✓</span>
              )}
              {isReady && isMe && !isEditingName && !canRemoveOpa && !canAddOpa && (
                <span className="text-green-400 text-sm shrink-0" title="Bereit">✓</span>
              )}
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
      {isMyLobby && !isStarted && (
        <div className="flex flex-col gap-1 shrink-0">
          <span className="text-white/40 text-xs uppercase tracking-wider">Szenario</span>
          <select
            disabled={settingScenario}
            value={selectedScenario ?? ''}
            onChange={(e) => handleSelectScenario(e.target.value || null)}
            className="w-full bg-white/10 border border-white/10 rounded-xl px-3 py-2 text-white text-sm disabled:opacity-50 focus:outline-none focus:border-indigo-500/50"
          >
            <option value="" className="bg-gray-900 text-white/50">Zufällig</option>
            {availableScenarios.map((name) => (
              <option key={name} value={name} className="bg-gray-900 text-white">{name}</option>
            ))}
          </select>
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
