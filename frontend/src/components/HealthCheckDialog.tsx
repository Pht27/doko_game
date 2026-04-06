interface HealthCheckDialogProps {
  playerId: number;
  onDeclare: (hasVorbehalt: boolean) => void;
}

export function HealthCheckDialog({ playerId, onDeclare }: HealthCheckDialogProps) {
  return (
    <div className="bg-gray-800/95 rounded-2xl p-4 w-72 shadow-2xl flex flex-col gap-2 border border-white/10">
      <h2 className="text-white font-bold text-sm">P{playerId}: Gesund oder Vorbehalt?</h2>

      <button
        onClick={() => onDeclare(false)}
        className="w-full bg-green-700 hover:bg-green-600 text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
      >
        Gesund
      </button>

      <button
        onClick={() => onDeclare(true)}
        className="w-full bg-red-700 hover:bg-red-600 text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
      >
        Vorbehalt
      </button>
    </div>
  );
}
