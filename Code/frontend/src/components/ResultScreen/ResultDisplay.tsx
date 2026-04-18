import type { GameResultDto } from '../../types/api';
import { t } from '../../translations';

interface ResultDisplayProps {
  result: GameResultDto;
  mySeat?: number;
}

/** Derives a seat's party from the winner and net points. */
function getSeatParty(seat: number, result: GameResultDto): 'Re' | 'Kontra' | null {
  const pts = result.netPointsPerSeat[seat];
  if (pts === undefined || pts === 0) return null;
  return pts > 0 ? (result.winner as 'Re' | 'Kontra') : result.winner === 'Re' ? 'Kontra' : 'Re';
}

export function ResultDisplay({ result, mySeat }: ResultDisplayProps) {
  const isReWinner = result.winner === 'Re';
  const isKontraWinner = result.winner === 'Kontra';

  // Sum of all extrapunkte for the "Extrapoints" row in column 3
  const extraSum = result.allAwards.reduce((acc, a) => acc + a.delta, 0);
  const myNetPoints = mySeat !== undefined ? (result.netPointsPerSeat[mySeat] ?? 0) : null;

  return (
    <div className="rd-columns">
      {/* ── Column 1: Stiche & Augen ── */}
      <div className="rd-col">
        <div className={isReWinner ? 'rd-winner-banner rd-winner-re' : 'rd-winner-banner rd-winner-kontra'}>
          {t.winnerLabel(result.winner)}
        </div>

        <div className="rd-augen-block">
          <div className={isReWinner ? 'rd-augen-row rd-augen-winner' : 'rd-augen-row'}>
            <span className="rd-augen-label">{t.reLabel}</span>
            <span className="rd-augen-value">{result.reAugen}</span>
          </div>
          <div className={isKontraWinner ? 'rd-augen-row rd-augen-winner' : 'rd-augen-row'}>
            <span className="rd-augen-label">{t.kontraLabel}</span>
            <span className="rd-augen-value">{result.kontraAugen}</span>
          </div>
        </div>

        {result.feigheit && (
          <div className="result-feigheit-banner">{t.feigheit}</div>
        )}
      </div>

      {/* ── Column 2: Extrapunkte ── */}
      <div className="rd-col">
        {result.allAwards.length > 0 ? (
          <ul className="rd-awards-list">
            {result.allAwards.map((award, i) => {
              const party = getSeatParty(award.benefittingPlayer, result);
              const nameCls =
                party === 'Re'
                  ? 'rd-award-player rd-award-re'
                  : party === 'Kontra'
                    ? 'rd-award-player rd-award-kontra'
                    : 'rd-award-player';
              return (
                <li key={i} className="rd-award-item">
                  <span className={nameCls}>{t.seatShort(award.benefittingPlayer)}</span>
                  <span className="rd-award-type">{award.type}</span>
                  <span className="rd-award-delta">
                    {award.delta > 0 ? '+' : ''}
                    {award.delta}
                  </span>
                </li>
              );
            })}
          </ul>
        ) : (
          <span className="rd-empty">{t.keineExtrapunkte}</span>
        )}
      </div>

      {/* ── Column 3: Spielwert ── */}
      <div className="rd-col">
        <div className="rd-breakdown">
          {result.valueComponents.map((c, i) => (
            <div key={i} className="rd-breakdown-row">
              <span className="rd-breakdown-label">{c.label}</span>
              <span className="rd-breakdown-value">
                {c.value > 0 ? `+${c.value}` : c.value}
              </span>
            </div>
          ))}

          {result.allAwards.length > 0 && (
            <div className="rd-breakdown-row">
              <span className="rd-breakdown-label">{t.extrapunkteNetto}</span>
              <span className="rd-breakdown-value">
                {extraSum > 0 ? `+${extraSum}` : extraSum}
              </span>
            </div>
          )}

          {result.soloFactor > 1 && (
            <div className="rd-breakdown-row">
              <span className="rd-breakdown-label">{t.soloFaktor(result.soloFactor)}</span>
              <span className="rd-breakdown-value" />
            </div>
          )}

          <div className="rd-breakdown-total">
            <span>{t.spielwert}</span>
            <span>
              {myNetPoints !== null
                ? (myNetPoints > 0 ? `+${myNetPoints}` : `${myNetPoints}`)
                : result.totalScore}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
}
