import { t } from '../translations';
import '../styles/ArmutReturnDialog.css';

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
    <div className="bg-gray-800/95 rounded-2xl p-4 w-72 shadow-2xl flex flex-col gap-2 border border-white/10">
      <h2 className="text-white font-bold text-sm">
        {t.armutReturnTitle(playerId, cardReturnCount)}
      </h2>
      <p className="text-white/60 text-xs">
        {t.armutReturnDescription(selectedCount, cardReturnCount)}
      </p>
      <button
        disabled={selectedCount !== cardReturnCount}
        onClick={onConfirm}
        className="w-full bg-orange-600 hover:bg-orange-500 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
      >
        {t.bestaetigen}
      </button>
    </div>
  );
}
