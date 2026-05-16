import type { PlayerRoundListItem } from '@/types/analog';
import { t } from '@/utils/translations';
import './BestWorstCards.css';

function MiniCard({ round, label, labelClass }: {
  round: PlayerRoundListItem | null;
  label: string;
  labelClass: string;
}) {
  if (!round) return null;
  const won = round.pointDelta >= 0;
  const date = new Date(round.playedAt).toLocaleDateString('de-DE', { day: '2-digit', month: 'short', year: '2-digit' });
  const delta = round.pointDelta;
  const pts = (delta >= 0 ? '+' : '') + delta.toFixed(0);
  return (
    <div className={`ps-bw-card ${labelClass}`}>
      <div className="ps-bw-card-label">{label}</div>
      <div className={`ps-bw-card-pts${won ? ' ps-bw-pts-win' : ' ps-bw-pts-loss'}`}>{pts}</div>
      <div className="ps-bw-card-mode">{round.gameMode}</div>
      <div className="ps-bw-card-date">{date}</div>
    </div>
  );
}

export function BestWorstCards({ best, worst }: {
  best: PlayerRoundListItem | null;
  worst: PlayerRoundListItem | null;
}) {
  if (!best && !worst) return null;
  return (
    <div className="ps-bw-row">
      <MiniCard round={best} label={t.playerBestGame} labelClass="ps-bw-best" />
      <MiniCard round={worst} label={t.playerWorstGame} labelClass="ps-bw-worst" />
    </div>
  );
}
