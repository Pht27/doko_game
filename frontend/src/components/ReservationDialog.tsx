interface ReservationDialogProps {
  playerId: number;
  eligibleReservations: string[];
  onDeclare: (reservation: string | null, hochzeitCondition: string | null, armutPartner: number | null) => void;
}

const HOCHZEIT_CONDITIONS = ['FirstTrick', 'FirstFehlTrick', 'FirstTrumpTrick'];

export function ReservationDialog({ playerId, eligibleReservations, onDeclare }: ReservationDialogProps) {
  return (
    <div className="fixed inset-0 bg-black/70 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-2xl p-6 w-96 shadow-2xl flex flex-col gap-4">
        <h2 className="text-white font-bold text-lg">Player {playerId}: Declare Reservation</h2>

        <button
          onClick={() => onDeclare(null, null, null)}
          className="w-full bg-white/10 hover:bg-white/20 text-white rounded-lg py-2 font-semibold transition-colors"
        >
          Pass (Keine Vorbehalt)
        </button>

        {eligibleReservations.map((res) => {
          if (res === 'Hochzeit') {
            return (
              <div key={res} className="flex flex-col gap-2">
                <div className="text-white/70 text-sm font-semibold">{res}:</div>
                {HOCHZEIT_CONDITIONS.map((cond) => (
                  <button
                    key={cond}
                    onClick={() => onDeclare('Hochzeit', cond, null)}
                    className="w-full bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg py-2 text-sm transition-colors"
                  >
                    Hochzeit ({cond})
                  </button>
                ))}
              </div>
            );
          }

          if (res === 'Armut') {
            return (
              <div key={res} className="flex flex-col gap-2">
                <div className="text-white/70 text-sm font-semibold">{res} — choose partner:</div>
                {[0, 1, 2, 3]
                  .filter((p) => p !== playerId)
                  .map((partner) => (
                    <button
                      key={partner}
                      onClick={() => onDeclare('Armut', null, partner)}
                      className="w-full bg-orange-600 hover:bg-orange-500 text-white rounded-lg py-2 text-sm transition-colors"
                    >
                      Armut (partner: P{partner})
                    </button>
                  ))}
              </div>
            );
          }

          return (
            <button
              key={res}
              onClick={() => onDeclare(res, null, null)}
              className="w-full bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg py-2 font-semibold transition-colors"
            >
              {res}
            </button>
          );
        })}
      </div>
    </div>
  );
}
