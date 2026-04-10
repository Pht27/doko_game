import { t } from '../../translations';
import '../../styles/ReservationDialog.css';

interface ReservationDialogProps {
  playerId: number;
  eligibleReservations: string[];
  mustDeclare?: boolean;
  onDeclare: (reservation: string | null, hochzeitCondition: string | null, armutPartner: number | null) => void;
}

const HOCHZEIT_CONDITIONS = ['FirstTrick', 'FirstFehlTrick', 'FirstTrumpTrick'];

export function ReservationDialog({ playerId, eligibleReservations, mustDeclare = false, onDeclare }: ReservationDialogProps) {
  return (
    <div className="reservation-dialog">
      <h2 className="reservation-title">{t.reservationTitle(playerId)}</h2>

      {!mustDeclare && (
        <button
          onClick={() => onDeclare(null, null, null)}
          className="reservation-btn-pass"
        >
          {t.pass}
        </button>
      )}

      {eligibleReservations.map((res) => {
        if (res === 'Hochzeit') {
          return (
            <div key={res} className="reservation-hochzeit-options">
              {HOCHZEIT_CONDITIONS.map((cond) => (
                <button
                  key={cond}
                  onClick={() => onDeclare('Hochzeit', cond, null)}
                  className="reservation-btn-hochzeit"
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
            className="reservation-btn"
          >
            {res}
          </button>
        );
      })}
    </div>
  );
}
