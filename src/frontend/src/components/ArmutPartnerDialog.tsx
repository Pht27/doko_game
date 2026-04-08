import { t } from '../translations';
import '../styles/ArmutPartnerDialog.css';

interface ArmutPartnerDialogProps {
  playerId: number;
  onRespond: (accepts: boolean) => void;
}

export function ArmutPartnerDialog({ playerId, onRespond }: ArmutPartnerDialogProps) {
  return (
    <div className="bg-gray-800/95 rounded-2xl p-4 w-72 shadow-2xl flex flex-col gap-2 border border-white/10">
      <h2 className="text-white font-bold text-sm">{t.armutPartnerTitle(playerId)}</h2>
      <p className="text-white/60 text-xs">{t.armutPartnerDescription}</p>

      <button
        onClick={() => onRespond(true)}
        className="w-full bg-green-700 hover:bg-green-600 text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
      >
        {t.annehmen}
      </button>

      <button
        onClick={() => onRespond(false)}
        className="w-full bg-white/10 hover:bg-white/20 text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
      >
        {t.ablehnen}
      </button>
    </div>
  );
}
