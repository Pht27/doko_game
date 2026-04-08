import { t } from '../translations';
import '../styles/PlayerSwitcher.css';

interface PlayerSwitcherProps {
  activePlayer: number;
  onSwitch: (player: number) => void;
}

export function PlayerSwitcher({ activePlayer, onSwitch }: PlayerSwitcherProps) {
  return (
    <div className="player-switcher">
      {[0, 1, 2, 3].map((p) => (
        <button
          key={p}
          onClick={() => onSwitch(p)}
          className={`player-switcher-btn ${activePlayer === p ? 'player-switcher-btn-active' : 'player-switcher-btn-inactive'}`}
        >
          {t.playerName(p)}
        </button>
      ))}
    </div>
  );
}
