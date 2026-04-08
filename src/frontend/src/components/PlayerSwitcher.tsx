import { t } from '../translations';
import '../styles/PlayerSwitcher.css';

interface PlayerSwitcherProps {
  activePlayer: number;
  onSwitch: (player: number) => void;
}

export function PlayerSwitcher({ activePlayer, onSwitch }: PlayerSwitcherProps) {
  return (
    <div className="flex gap-1 rounded-lg bg-white/10 p-1">
      {[0, 1, 2, 3].map((p) => (
        <button
          key={p}
          onClick={() => onSwitch(p)}
          className={[
            'px-3 py-1 rounded-md text-sm font-semibold transition-colors',
            activePlayer === p
              ? 'bg-indigo-500 text-white shadow'
              : 'text-white/70 hover:text-white hover:bg-white/10',
          ].join(' ')}
        >
          {t.playerName(p)}
        </button>
      ))}
    </div>
  );
}
