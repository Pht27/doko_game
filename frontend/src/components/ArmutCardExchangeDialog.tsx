import { useState } from 'react';
import type { CardDto } from '../types/api';

interface ArmutCardExchangeDialogProps {
  playerId: number;
  hand: CardDto[];
  returnCount: number;
  onConfirm: (cardIds: number[]) => void;
}

export function ArmutCardExchangeDialog({
  playerId,
  hand,
  returnCount,
  onConfirm,
}: ArmutCardExchangeDialogProps) {
  const [selected, setSelected] = useState<Set<number>>(new Set());

  function toggleCard(id: number) {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else if (next.size < returnCount) {
        next.add(id);
      }
      return next;
    });
  }

  return (
    <div className="bg-gray-800/95 rounded-2xl p-4 w-80 shadow-2xl flex flex-col gap-3 border border-white/10">
      <h2 className="text-white font-bold text-sm">
        P{playerId}: {returnCount} Karte(n) zurückgeben
      </h2>
      <p className="text-white/60 text-xs">
        Wähle genau {returnCount} Karte(n) aus deiner Hand zurückzugeben. ({selected.size}/{returnCount} gewählt)
      </p>

      <div className="flex flex-wrap gap-1 max-h-48 overflow-y-auto">
        {hand.map((card) => {
          const isSelected = selected.has(card.id);
          return (
            <button
              key={card.id}
              onClick={() => toggleCard(card.id)}
              className={`px-2 py-1 rounded text-xs font-mono transition-colors ${
                isSelected
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white/10 hover:bg-white/20 text-white/80'
              }`}
            >
              {card.rank[0]}{card.suit[0]}
            </button>
          );
        })}
      </div>

      <button
        disabled={selected.size !== returnCount}
        onClick={() => onConfirm(Array.from(selected))}
        className="w-full bg-indigo-600 hover:bg-indigo-500 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg py-1.5 text-sm font-semibold transition-colors"
      >
        Bestätigen
      </button>
    </div>
  );
}
