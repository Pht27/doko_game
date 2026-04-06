import type { GameResultDto } from '../types/api';

interface ResultScreenProps {
  result: GameResultDto;
  onNewGame: () => void;
}

export function ResultScreen({ result, onNewGame }: ResultScreenProps) {
  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-2xl p-8 w-96 shadow-2xl flex flex-col gap-4 text-white">
        <h2 className="text-2xl font-bold text-center">
          {result.winner} wins!
        </h2>

        <div className="grid grid-cols-2 gap-2 text-sm">
          <div className="text-white/60">Re points</div>
          <div className="font-semibold">{result.rePoints}</div>
          <div className="text-white/60">Kontra points</div>
          <div className="font-semibold">{result.kontraPoints}</div>
          <div className="text-white/60">Game value</div>
          <div className="font-semibold">{result.gameValue}</div>
          {result.feigheit && (
            <>
              <div className="text-white/60">Note</div>
              <div className="text-yellow-400">Feigheit</div>
            </>
          )}
        </div>

        {result.allAwards.length > 0 && (
          <div>
            <div className="text-white/60 text-sm mb-2">Extra points:</div>
            <ul className="flex flex-col gap-1">
              {result.allAwards.map((award, i) => (
                <li key={i} className="flex justify-between text-sm">
                  <span>{award.type} (P{award.benefittingPlayer})</span>
                  <span className="font-semibold">{award.delta > 0 ? '+' : ''}{award.delta}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        <button
          onClick={onNewGame}
          className="mt-2 bg-indigo-500 hover:bg-indigo-600 text-white rounded-lg py-3 font-bold transition-colors"
        >
          New Game
        </button>
      </div>
    </div>
  );
}
