import type { GameResultDto } from '../../types/api';
import { t } from '../../translations';

interface GeschmissenDisplayProps {
  result: GameResultDto;
  mySeat?: number;
}

export function GeschmissenDisplay({ result, mySeat }: GeschmissenDisplayProps) {
  const hasStandings = result.lobbyStandings?.length > 0;

  return (
    <>
      <h2 className="result-title">{t.geschmissenTitle}</h2>
      <p className="text-white/60 text-sm text-center">{t.geschmissenSubtitle}</p>

      {hasStandings && (
        <div>
          <div className="result-section-header">{t.gesamtstand}</div>
          <div className="result-breakdown">
            {result.lobbyStandings.map((pts, seat) => {
              const isMe = seat === mySeat;
              return (
                <div key={seat} className={isMe ? 'result-standings-row-me' : 'result-standings-row'}>
                  <span>{t.playerLabel(seat)}</span>
                  <span className="result-breakdown-value">{pts}</span>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </>
  );
}
