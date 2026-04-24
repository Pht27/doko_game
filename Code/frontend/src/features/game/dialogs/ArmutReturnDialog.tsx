import { t } from '@/utils/translations';
import './ArmutReturnDialog.css';

interface ArmutReturnDialogProps {
  playerId: number;
  cardReturnCount: number;
  selectedCount: number;
  onConfirm: () => void;
}

export function ArmutReturnDialog({
  playerId,
  cardReturnCount,
  selectedCount,
  onConfirm,
}: ArmutReturnDialogProps) {
  return (
    <div className="armut-return-dialog">
      <h2 className="armut-return-title">
        {t.armutReturnTitle(playerId, cardReturnCount)}
      </h2>
      <p className="armut-return-description">
        {t.armutReturnDescription(selectedCount, cardReturnCount)}
      </p>
      <button
        disabled={selectedCount !== cardReturnCount}
        onClick={onConfirm}
        className="armut-return-btn-confirm"
      >
        {t.bestaetigen}
      </button>
    </div>
  );
}
