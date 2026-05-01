import { t } from '@/utils/translations';
import { usePlayerName } from '@/context/PlayerNamesContext';
import './SchwarzesSauSoloDialog.css';

interface SchwarzesSauSoloDialogProps {
  playerId: number;
  eligibleSolos: string[];
  onChoose: (solo: string) => void;
}

/** Renders the grid of choosable solos. SchlankerMartin spans the full width. */
function SoloPicker({
  eligibleSolos,
  onChoose,
}: {
  eligibleSolos: string[];
  onChoose: (solo: string) => void;
}) {
  return (
    <div className="ssd-body">
      {eligibleSolos.map((solo) => (
        <button
          key={solo}
          className={`ssd-solo-btn${solo === 'SchlankerMartin' ? ' ssd-solo-btn-martin' : ''}`}
          onClick={() => onChoose(solo)}
        >
          {t.soloLabel(solo)}
        </button>
      ))}
    </div>
  );
}

export function SchwarzesSauSoloDialog({
  playerId,
  eligibleSolos,
  onChoose,
}: SchwarzesSauSoloDialogProps) {
  const playerName = usePlayerName(playerId);
  return (
    <div className="ssd-dialog">
      <div className="ssd-header">
        <span className="ssd-title">{t.schwarzesSauSoloTitle(playerId, playerName)}</span>
        <span className="ssd-subtitle">{t.schwarzesSauSoloSubtitle}</span>
      </div>
      <SoloPicker eligibleSolos={eligibleSolos} onChoose={onChoose} />
    </div>
  );
}
