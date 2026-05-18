import { PlayerLink } from '@/components/PlayerLink/PlayerLink';
import { usePlayerPreference } from '@/context/PlayerPreferenceContext';
import type { NormalizedTeam } from '../RoundCard';

export function TeamBlock({
  team,
  won,
  expanded,
}: {
  team: NormalizedTeam;
  won: boolean;
  expanded: boolean;
}) {
  const { selectedPlayer } = usePlayerPreference();
  const hasSpecials = team.specialCards.length > 0;
  const hasExtras = team.extraPoints.length > 0;

  return (
    <div className={`ahr-team-block ${won ? 'ahr-party-win' : 'ahr-party-lose'}${expanded ? ' ahr-block-expanded' : ''}`}>
      {team.players.map((p) => (
        <PlayerLink
          key={p.id}
          player={p}
          className={`ahr-player-name${selectedPlayer?.id === p.id ? ' ahr-player-name--selected' : ''}`}
        />
      ))}
      {hasSpecials && (
        <div className={`ahr-team-section${expanded ? ' ahr-section-show' : ''}`}>
          <span className="ahr-section-label">Sonderkarten</span>
          {team.specialCards.map((sc) => (
            <span key={sc.id} className="ahr-section-item">{sc.name}</span>
          ))}
        </div>
      )}
      {hasExtras && (
        <div className={`ahr-team-section${expanded ? ' ahr-section-show' : ''}`}>
          <span className="ahr-section-label">Extrapunkte</span>
          {team.extraPoints.map((ep) => (
            <span key={ep.id} className="ahr-section-item">
              {ep.name}{ep.count > 1 ? ` (${ep.count})` : ''}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
