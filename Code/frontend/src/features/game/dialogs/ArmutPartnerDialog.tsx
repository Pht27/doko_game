import { t } from '@/utils/translations';
import './ArmutPartnerDialog.css';

interface ArmutPartnerDialogProps {
  playerId: number;
  onRespond: (accepts: boolean) => void;
}

export function ArmutPartnerDialog({ playerId, onRespond }: ArmutPartnerDialogProps) {
  return (
    <div className="armut-partner-dialog">
      <h2 className="armut-partner-title">{t.armutPartnerTitle(playerId)}</h2>
      <p className="armut-partner-description">{t.armutPartnerDescription}</p>

      <button
        onClick={() => onRespond(true)}
        className="armut-partner-btn-accept"
      >
        {t.annehmen}
      </button>

      <button
        onClick={() => onRespond(false)}
        className="armut-partner-btn-decline"
      >
        {t.ablehnen}
      </button>
    </div>
  );
}
