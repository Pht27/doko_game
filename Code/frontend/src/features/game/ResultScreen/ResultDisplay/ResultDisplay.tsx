import type { GameResultDto } from '@/types/api';
import { t } from '@/utils/translations';
import { type Party, buildComponentRows } from './resultDisplay.utils';
import { WinnerBanner } from './WinnerBanner';
import { AugenRow } from './AugenRow';
import { ValueComponentsTable } from './ValueComponentsTable';
import { AwardsTable } from './AwardsTable';
import { SoloFactorNote } from './SoloFactorNote';
import { TotalRow } from './TotalRow';

interface ResultDisplayProps {
  result: GameResultDto;
  mySeat?: number;
}

export function ResultDisplay({ result, mySeat }: ResultDisplayProps) {
  const winner = result.winner as Party;
  const isReWinner = winner === 'Re';

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

  const gameModeLabel = result.gameMode ? t.gameModeLabel(result.gameMode) : null;
  const headerLabel = gameModeLabel ? `${gameModeLabel}: ${winner} gewinnt` : `${winner} gewinnt`;

  const componentRows = buildComponentRows(
    result.valueComponents,
    result.announcementRecords,
    winner,
    result.feigheit,
  );

  return (
    <div className="rd-container">
      <WinnerBanner winner={winner} headerLabel={headerLabel} />

      <AugenRow
        reAugen={result.reAugen}
        reStiche={result.reStiche}
        kontraAugen={result.kontraAugen}
        kontraStiche={result.kontraStiche}
        isReWinner={isReWinner}
        result={result}
      />

      {result.feigheit && (
        <div className="result-feigheit-banner">{t.feigheit}</div>
      )}

      <div className="rd-separator" />
      <ValueComponentsTable rows={componentRows} sign={sign} />

      {result.allAwards.length > 0 && (
        <>
          <div className="rd-separator" />
          <AwardsTable awards={result.allAwards} myParty={myParty} result={result} />
        </>
      )}

      {result.soloFactor > 1 && myParty === (result.gameMode === 'KontraSolo' ? 'Kontra' : 'Re') && (
        <>
          <div className="rd-separator" />
          <SoloFactorNote soloFactor={result.soloFactor} />
        </>
      )}

      <div className="rd-separator" />
      <TotalRow myNetPoints={myNetPoints} totalScore={result.totalScore} />
    </div>
  );
}
