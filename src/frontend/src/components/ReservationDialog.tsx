import { t } from '../translations';
import '../styles/ReservationDialog.css';

interface ReservationDialogProps {
  playerId: number;
  eligibleReservations: string[];
  mustDeclare?: boolean;
  onDeclare: (reservation: string | null, hochzeitCondition: string | null, armutPartner: number | null) => void;
}

const HOCHZEIT_CONDITIONS = ['FirstTrick', 'FirstFehlTrick', 'FirstTrumpTrick'];

export function ReservationDialog({ playerId, eligibleReservations, mustDeclare = false, onDeclare }: ReservationDialogProps) {
  return (
    <div className="bg-gray-800/95 rounded-2xl p-4 w-80 shadow-2xl flex flex-col gap-2 border border-white/10">
      <h2 className="text-white font-bold text-sm">{t.reservationTitle(playerId)}</h2>

      {!mustDeclare && (
        <button
          onClick={() => onDeclare(null, null, null)}
          className="w-full bg-white/10 hover:bg-white/20 text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
        >
          {t.pass}
        </button>
      )}

      {eligibleReservations.map((res) => {
        if (res === 'Hochzeit') {
          return (
            <div key={res} className="flex flex-col gap-1">
              {HOCHZEIT_CONDITIONS.map((cond) => (
                <button
                  key={cond}
                  onClick={() => onDeclare('Hochzeit', cond, null)}
                  className="w-full bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg py-1.5 text-xs transition-colors"
                >
                  {t.hochzeitLabel(cond)}
                </button>
              ))}
            </div>
          );
        }

        return (
          <button
            key={res}
            onClick={() => onDeclare(res, null, null)}
            className="w-full bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
          >
            {res}
          </button>
        );
      })}
    </div>
  );
}
