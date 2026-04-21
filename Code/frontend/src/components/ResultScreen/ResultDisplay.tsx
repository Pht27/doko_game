import type { AnnouncementRecordDto, GameResultDto } from '../../types/api';
import { t } from '../../translations';

interface ResultDisplayProps {
  result: GameResultDto;
  mySeat?: number;
}

type Party = 'Re' | 'Kontra';

const componentAnnouncementType: Record<string, string> = {
  'Gewonnen': 'Win',
  'Keine 90': 'Keine90',
  'Keine 60': 'Keine60',
  'Keine 30': 'Keine30',
  'Schwarz': 'Schwarz',
};

function buildComponentRows(
  valueComponents: GameResultDto['valueComponents'],
  announcementRecords: AnnouncementRecordDto[],
  winner: string,
  feigheit: boolean,
): { label: string; value: number }[] {
  if (feigheit) return valueComponents.map(c => ({ label: c.label, value: c.value }));

  const matched = new Set<number>();

  const rows = valueComponents.map(c => {
    const aType = componentAnnouncementType[c.label];
    if (!aType) return { label: c.label, value: c.value };

    const matchingIndices = announcementRecords
      .map((r, i) => ({ r, i }))
      .filter(({ r, i }) =>
        r.type === aType &&
        !matched.has(i) &&
        (aType !== 'Win' || r.party === winner),
      )
      .map(({ i }) => i);

    matchingIndices.forEach(i => matched.add(i));
    const bonus = matchingIndices.length;
    return {
      label: bonus > 0 ? `${c.label} ${t.announcedSuffix}` : c.label,
      value: c.value + bonus,
    };
  });

  const unmatchedRows = announcementRecords
    .filter((_, i) => !matched.has(i))
    .map(r => ({
      label: `${t.announcementLabel(r.type === 'Win' ? r.party : r.type)} ${t.announcedSuffix}`,
      value: 1,
    }));

  return [...rows, ...unmatchedRows];
}

function getSeatParty(seat: number, result: GameResultDto): Party | null {
  const pts = result.netPointsPerSeat[seat];
  if (pts === undefined || pts === 0) return null;
  return pts > 0 ? (result.winner as Party) : result.winner === 'Re' ? 'Kontra' : 'Re';
}

function fmt(n: number): string {
  return n > 0 ? `+${n}` : `${n}`;
}

export function ResultDisplay({ result, mySeat }: ResultDisplayProps) {
  const winner = result.winner as Party;
  const isReWinner = winner === 'Re';

  // Determine viewing player's party and sign multiplier
  const myNetPoints = mySeat !== undefined ? (result.netPointsPerSeat[mySeat] ?? 0) : null;
  const myParty: Party | null =
    myNetPoints === null
      ? null
      : myNetPoints > 0
        ? winner
        : winner === 'Re'
          ? 'Kontra'
          : 'Re';
  const sign = myParty === null ? 1 : myParty === winner ? 1 : -1;

  // Header line: "<GameMode>: <Winner> gewinnt" or just "<Winner> gewinnt"
  const gameModeLabel = result.gameMode ? t.gameModeLabel(result.gameMode) : null;
  const headerLabel = gameModeLabel
    ? `${gameModeLabel}: ${winner} gewinnt`
    : `${winner} gewinnt`;

  const componentRows = buildComponentRows(
    result.valueComponents,
    result.announcementRecords,
    winner,
    result.feigheit,
  );

  return (
    <div className="rd-container">
      {/* Winner header */}
      <div className={isReWinner ? 'rd-winner-banner rd-winner-re' : 'rd-winner-banner rd-winner-kontra'}>
        {headerLabel}
      </div>

      {/* Augen + stiche row */}
      <div className="rd-augen-row">
        <span className={isReWinner ? 'rd-augen-party rd-augen-winner' : 'rd-augen-party'}>
          Re
        </span>
        <span className={isReWinner ? 'rd-augen-score rd-augen-winner' : 'rd-augen-score'}>
          {result.reAugen}
          {result.reStiche != null && (
            <span className="rd-augen-stiche"> ({result.reStiche})</span>
          )}
        </span>
        <span className="rd-augen-divider">|</span>
        <span className={!isReWinner ? 'rd-augen-score rd-augen-winner' : 'rd-augen-score'}>
          {result.kontraAugen}
          {result.kontraStiche != null && (
            <span className="rd-augen-stiche"> ({result.kontraStiche})</span>
          )}
        </span>
        <span className={!isReWinner ? 'rd-augen-party rd-augen-winner' : 'rd-augen-party'}>
          Kontra
        </span>
      </div>

      {result.feigheit && (
        <div className="result-feigheit-banner">{t.feigheit}</div>
      )}

      {/* Component table */}
      <div className="rd-separator" />

      <div className="rd-table">
        {componentRows.map((c, i) => (
          <div key={i} className="rd-row">
            <span className="rd-row-label">{c.label}</span>
            <span className={sign * c.value >= 0 ? 'rd-row-value rd-value-pos' : 'rd-row-value rd-value-neg'}>
              {fmt(sign * c.value)}
            </span>
          </div>
        ))}
      </div>

      {/* Extrapunkte */}
      {result.allAwards.length > 0 && (
        <>
          <div className="rd-separator" />
          <div className="rd-table">
            {result.allAwards.map((award, i) => {
              const awardParty = getSeatParty(award.benefittingPlayer, result);
              const awardSign =
                myParty === null ? 1 : awardParty === myParty ? 1 : -1;
              const value = awardSign * award.delta;
              return (
                <div key={i} className="rd-row">
                  <span className="rd-row-label">
                    {award.type}
                    <span className="rd-award-player"> ({t.seatShort(award.benefittingPlayer)})</span>
                  </span>
                  <span className={value >= 0 ? 'rd-row-value rd-value-pos' : 'rd-row-value rd-value-neg'}>
                    {fmt(value)}
                  </span>
                </div>
              );
            })}
          </div>
        </>
      )}

      {result.soloFactor > 1 && myParty === (result.gameMode === 'KontraSolo' ? 'Kontra' : 'Re') && (
        <>
          <div className="rd-separator" />
          <div className="rd-table">
            <div className="rd-row">
              <span className="rd-row-label rd-row-label-factor">{t.soloFaktor(result.soloFactor)}</span>
            </div>
          </div>
        </>
      )}

      {/* Total */}
      <div className="rd-separator" />
      <div className="rd-total">
        <span>{t.insgesamt}</span>
        <span className={
          (myNetPoints ?? result.totalScore) >= 0
            ? 'rd-total-value rd-value-pos'
            : 'rd-total-value rd-value-neg'
        }>
          {myNetPoints !== null ? fmt(myNetPoints) : fmt(result.totalScore)}
        </span>
      </div>
    </div>
  );
}
