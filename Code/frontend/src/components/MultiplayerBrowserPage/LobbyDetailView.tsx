import { useState, useEffect } from 'react';
import { t } from '../../translations';
import { leaveLobby, joinSeat, getLobby, voteReady, withdrawReady } from '../../api/lobby';
import {
  useLobby,
  loadLobbySession,
  loadAnySession,
  saveLobbySession,
  clearLobbySession,
} from '../../hooks/useLobby';
import { ResultScreen } from '../ResultScreen/ResultScreen';
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

  const { seats, gameId, isStarted, lobbyClosed, startVoteCount, error } = useLobby(session, lobbyId);

  const [copied, setCopied] = useState(false);
  const [hasVoted, setHasVoted] = useState(false);
  const [voting, setVoting] = useState(false);
  const [leaving, setLeaving] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const [busySeat, setBusySeat] = useState<number | null>(null); // index being joined/swapped
  const [actionError, setActionError] = useState<string | null>(null);

  const inviteUrl = `${window.location.origin}${window.location.pathname}?lobby=${lobbyId}`;

  useEffect(() => {
    if (gameId && session) {
      clearLobbySession();
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
        playerId: res.playerId,
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
      // Leave first — ignore backend errors (e.g. lobby already gone) so we don't get stuck
      try {
        await leaveLobby(session.token, lobbyId);
      } catch {
        // best-effort
      }
      clearLobbySession();
      setSession(null);

      const res = await joinSeat(lobbyId, targetSeat);
      const newSession: LobbySession = {
        lobbyId: res.lobbyId,
        token: res.token,
        playerId: res.playerId,
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
          const isMe = isMyLobby && session!.seatIndex === i;
          const isBusy = busySeat === i;
          // Can click if: seat is empty AND not blocked by a session elsewhere AND not own current seat
          const canInteract = !occupied && !isInAnotherLobby && !isMe;

          function handleClick() {
            if (!canInteract || isBusy) return;
            if (canSwapSeats) doSwap(i);
            else doJoin(i);
          }

          return (
            <button
              key={i}
              onClick={handleClick}
              disabled={!canInteract || isBusy}
              className={`flex items-center gap-2 px-3 py-3 rounded-xl transition-colors text-left w-full ${
                isMe
                  ? 'bg-indigo-600/50 text-white ring-1 ring-indigo-400'
                  : occupied
                    ? 'bg-white/15 text-white cursor-default'
                    : canInteract
                      ? 'bg-white/5 text-white/50 hover:bg-white/10 hover:text-white/80 cursor-pointer'
                      : 'bg-white/5 text-white/20 cursor-default'
              }`}
            >
              <div
                className={`w-2.5 h-2.5 rounded-full shrink-0 ${
                  occupied ? 'bg-green-400' : 'bg-white/20'
                }`}
              />
              <span className="text-sm font-medium truncate">
                {isBusy ? (
                  t.joiningLobby
                ) : occupied ? (
                  <>
                    {t.playerSlot(i)}
                    {isMe && <span className="text-white/50 text-xs">{t.youSuffix}</span>}
                  </>
                ) : (
                  t.seatLabel(i)
                )}
              </span>
            </button>
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
          {!isStarted && (
            <button
              onClick={hasVoted ? handleWithdraw : handleVote}
              disabled={filledCount < 4 || voting}
              className={`w-full py-3 text-base font-semibold rounded-2xl transition-colors disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-between px-4 ${
                hasVoted
                  ? 'bg-green-600 hover:bg-green-500 active:bg-green-700 text-white'
                  : 'bg-white/15 hover:bg-white/25 active:bg-white/10 text-white'
              }`}
            >
              <span className="text-sm opacity-70">{startVoteCount}/4 👤</span>
              <span>{hasVoted ? t.zurueckziehen : t.bereit}</span>
              <span className="w-12" />
            </button>
          )}
          <button
            onClick={handleLeaveSeat}
            disabled={leaving}
            className="w-full py-2.5 text-sm font-semibold rounded-2xl bg-white/10 hover:bg-white/20 active:bg-white/5 text-white/70 transition-colors disabled:opacity-40"
          >
            {leaving ? t.loading : t.leaveSeat}
          </button>
        </div>
      )}

      {/* Match history button — only when previous game data is available */}
      {lastFinishedResult && (
        <button
          onClick={() => setShowHistory(true)}
          className="w-full py-2.5 text-sm font-semibold rounded-2xl bg-white/10 hover:bg-white/20 active:bg-white/5 text-white/70 transition-colors shrink-0"
        >
          {t.spielverlauf}
        </button>
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
