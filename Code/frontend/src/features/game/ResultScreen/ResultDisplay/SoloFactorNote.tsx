import { t } from '@/utils/translations';

interface SoloFactorNoteProps {
  soloFactor: number;
}

export function SoloFactorNote({ soloFactor }: SoloFactorNoteProps) {
  return (
    <div className="rd-table">
      <div className="rd-row">
        <span className="rd-row-label rd-row-label-factor">{t.soloFaktor(soloFactor)}</span>
      </div>
    </div>
  );
}
