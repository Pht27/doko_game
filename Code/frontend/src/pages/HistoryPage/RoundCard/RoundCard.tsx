import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getRound } from '@/api/analog';
import { t } from '@/utils/translations';
import type { RoundListItem, RoundDetail } from '@/types/analog';
import { TeamBlock } from './TeamBlock/TeamBlock';

export interface NormalizedTeam {
  players: { id: number; name: string }[];
  specialCards: { id: number; name: string }[];
  extraPoints: { id: number; name: string; count: number }[];
}

function toFallback(players: { id: number; name: string }[]): NormalizedTeam[] {
  return players.map((p) => ({ players: [p], specialCards: [], extraPoints: [] }));
}

function fromDetail(detail: RoundDetail, party: 'Re' | 'Kontra'): NormalizedTeam[] {
  return detail.teams
    .filter((t) => t.party === party)
    .map((t) => ({
      players: t.players,
      specialCards: t.specialCards,
      extraPoints: t.extraPoints,
    }));
}

export function RoundCard({
  round,
  onDelete,
}: {
  round: RoundListItem;
  onDelete: (id: number) => void;
}) {
  const navigate = useNavigate();
  const [expanded, setExpanded] = useState(false);
  const [detail, setDetail] = useState<RoundDetail | null>(null);

  const reWon = round.winningParty === 'Re';
  const isSolo =
    round.gameMode.toLowerCase().includes('solo') || round.gameMode === 'Fleischloser';
  const date = new Date(round.playedAt);

  const handleToggle = () => {
    const next = !expanded;
    setExpanded(next);
    if (next && !detail) {
      getRound(round.id).then(setDetail).catch(() => {});
    }
  };

  const reTeams = detail ? fromDetail(detail, 'Re') : toFallback(round.rePlayers);
  const kontraTeams = detail ? fromDetail(detail, 'Kontra') : toFallback(round.kontraPlayers);

  return (
    <div className="ahr-card">
      <div className="ahr-title-bar">Runde #{round.id}</div>

      <div className="ahr-content">
        <div className="ahr-match-header">
          <div className="ahr-header-left">
            {date.toLocaleDateString('de-DE', {
              weekday: 'short',
              day: '2-digit',
              month: 'short',
              year: 'numeric',
            })}
          </div>
          <div className={`ahr-header-center${isSolo ? ' ahr-header-solo' : ''}`}>
            {round.gameMode}
          </div>
          <div className="ahr-header-right">
            {date.toLocaleTimeString('de-DE', { hour: '2-digit', minute: '2-digit' })}
          </div>
        </div>

        <div className="ahr-meta-row">
          <div className="ahr-party-title">Re</div>
          <div className="ahr-points-title">PKT</div>
          <div className="ahr-party-title ahr-party-right">Kontra</div>
        </div>

        <div className="ahr-body">
          <div className={`ahr-party-col${expanded ? ' ahr-col-expanded' : ''}`}>
            {reTeams.map((team, i) => (
              <TeamBlock
                key={i}
                team={team}
                won={reWon}
                expanded={expanded}
                onPlayerClick={(id) => navigate(`/players/${id}`)}
              />
            ))}
          </div>

          <div className="ahr-points-col">
            <div className="ahr-points-box">{round.points}</div>
          </div>

          <div className={`ahr-party-col ahr-kontra-col${expanded ? ' ahr-col-expanded' : ''}`}>
            {kontraTeams.map((team, i) => (
              <TeamBlock
                key={i}
                team={team}
                won={!reWon}
                expanded={expanded}
                onPlayerClick={(id) => navigate(`/players/${id}`)}
              />
            ))}
          </div>
        </div>

        {round.comment && (
          <div className={`ahr-comment-section${expanded ? ' ahr-comment-show' : ''}`}>
            <div className="ahr-comment">„{round.comment}"</div>
          </div>
        )}

        <div className="ahr-controls">
          <button
            className={`ahr-ctrl-btn ahr-ctrl-left${expanded ? ' ahr-ctrl-show' : ''}`}
            onClick={(e) => {
              e.stopPropagation();
              onDelete(round.id);
            }}
          >
            {t.analogHistoryDelete}
          </button>

          <button
            className={`ahr-expand-toggle${expanded ? ' ahr-toggle-open' : ''}`}
            onClick={handleToggle}
            aria-label="Details anzeigen"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="24"
              height="24"
              viewBox="0 0 24 24"
              className="ahr-arrow"
            >
              <path
                fill="currentColor"
                fillRule="nonzero"
                d="M12 13.2 16.5 9l1.5 1.4-6 5.6-6-5.6L7.5 9z"
              />
            </svg>
          </button>

          <button
            className={`ahr-ctrl-btn ahr-ctrl-right${expanded ? ' ahr-ctrl-show' : ''}`}
            onClick={(e) => {
              e.stopPropagation();
              navigate(`/analog/edit/${round.id}`);
            }}
          >
            {t.analogHistoryEdit}
          </button>
        </div>
      </div>
    </div>
  );
}
