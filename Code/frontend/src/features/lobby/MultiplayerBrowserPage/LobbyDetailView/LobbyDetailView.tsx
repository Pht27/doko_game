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
import { SeatCard } from './SeatCard/SeatCard';

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
  const [busySeat, setBusySeat] = useState<number | null>(null);
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
  const isInAnotherLobby = !isMyLobby && loadAnySession() !== null;
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

          return (
            <SeatCard
              key={i}
              seatIndex={i}
              occupied={occupied}
              isOpa={isOpa}
              isMe={isMe}
              isReady={isReady}
              isBusy={isBusy}
              canInteract={canInteract}
              canAddOpa={canAddOpa}
              canRemoveOpa={canRemoveOpa}
              playerName={playerNames[i]}
              isEditingName={isEditingName && isMe}
              nameInput={nameInput}
              nameInputRef={nameInputRef}
              onNameChange={setNameInput}
              onNameBlur={submitName}
              onNameKeyDown={handleNameKeyDown}
              onStartEditingName={startEditingName}
              onClick={() => {
                if (!canInteract || isBusy) return;
                if (canSwapSeats) doSwap(i);
                else doJoin(i);
              }}
              onAddOpa={() => doAddOpa(i)}
              onRemoveOpa={() => doRemoveOpa(i)}
            />
          );
        })}
      </div>

      <p className="text-white/40 text-xs shrink-0">
        {t.playerCount(filledCount, 4)}
        {isStarted
          ? <span className="text-orange-400"> · {t.gameRunning}</span>
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
          <span className="text-white/40 text-xs uppercase tracking-wider">{t.scenarioLabel}</span>
          <select
            disabled={settingScenario}
            value={selectedScenario ?? ''}
            onChange={(e) => handleSelectScenario(e.target.value || null)}
            className="w-full bg-white/10 border border-white/10 rounded-xl px-3 py-2 text-white text-sm disabled:opacity-50 focus:outline-none focus:border-indigo-500/50"
          >
            <option value="" className="bg-gray-900 text-white/50">{t.scenarioRandom}</option>
            {availableScenarios.map((name) => (
              <option key={name} value={name} className="bg-gray-900 text-white">{name}</option>
            ))}
          </select>
        </div>
      )}
      {isMyLobby && (
        <div className="flex flex-col gap-2 shrink-0">
          {lastFinishedResult && (
            <button
              onClick={() => setShowHistory(true)}
              className="w-full py-2.5 text-xs font-bold uppercase tracking-wider rounded-lg bg-white/10 hover:bg-white/20 active:bg-white/5 text-white/70 transition-colors"
            >
              {t.spielverlauf}
            </button>
          )}
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
