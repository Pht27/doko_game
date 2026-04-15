import { useState } from 'react';
import { t } from '../../translations';
import { startLobbyGame } from '../../api/lobby';
import type { LobbySession } from '../../hooks/useLobby';

interface LobbyPageProps {
  session: LobbySession;
  playerCount: number;
  onGameStarted: (gameId: string) => void;
}

export function LobbyPage({ session, playerCount, onGameStarted }: LobbyPageProps) {
  const [copied, setCopied] = useState(false);
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const inviteUrl = `${window.location.origin}${window.location.pathname}?lobby=${session.lobbyId}`;

  async function copyLink() {
    await navigator.clipboard.writeText(inviteUrl);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  async function handleStartGame() {
    setStarting(true);
    setError(null);
    try {
      const res = await startLobbyGame(session.token, session.lobbyId);
      onGameStarted(res.gameId);
    } catch (e) {
      setError(e instanceof Error ? e.message : String(e));
      setStarting(false);
    }
  }

  return (
    <div className="w-full h-full flex flex-col items-center justify-center gap-8 px-6">
      <h1 className="text-3xl font-bold text-white">{t.lobbyTitle}</h1>

      {/* Player slots */}
      <div className="flex flex-col gap-3 w-full max-w-sm">
        {Array.from({ length: 4 }, (_, i) => {
          const filled = i < playerCount;
          const isMe = i === session.playerId;
          return (
            <div
              key={i}
              className={`flex items-center gap-3 px-4 py-3 rounded-xl transition-colors ${
                filled ? 'bg-white/15 text-white' : 'bg-white/5 text-white/30'
              }`}
            >
              <div
                className={`w-3 h-3 rounded-full flex-shrink-0 ${
                  filled ? 'bg-green-400' : 'bg-white/20'
                }`}
              />
              <span className="text-lg">
                {t.playerSlot(i)}
                {isMe && <span className="text-white/50 text-base">{t.youSuffix}</span>}
              </span>
            </div>
          );
        })}
      </div>

      <p className="text-white/50 text-sm">
        {t.playerCount(playerCount, 4)}
        {playerCount < 4 && ` · ${t.waitingForPlayers}`}
      </p>

      {/* Invite link */}
      <div className="flex flex-col gap-2 w-full max-w-sm">
        <span className="text-white/50 text-xs uppercase tracking-wider">{t.inviteLink}</span>
        <div className="flex gap-2">
          <div className="flex-1 bg-white/10 rounded-xl px-3 py-3 text-white/60 text-sm font-mono truncate">
            {inviteUrl}
          </div>
          <button
            onClick={copyLink}
            className="px-4 py-3 rounded-xl bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 text-white text-sm font-semibold transition-colors flex-shrink-0"
          >
            {copied ? t.linkCopied : t.copyLink}
          </button>
        </div>
      </div>

      {/* Start button (host only) */}
      {session.isHost && (
        <button
          onClick={handleStartGame}
          disabled={playerCount < 4 || starting}
          className="w-full max-w-sm py-4 text-xl font-semibold rounded-2xl bg-green-600 hover:bg-green-500 active:bg-green-700 text-white transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          {starting ? t.loading : t.startGame}
        </button>
      )}

      {error && <p className="text-red-400 text-sm text-center">{error}</p>}
    </div>
  );
}
