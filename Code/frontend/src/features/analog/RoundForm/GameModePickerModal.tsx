import { useEffect, useRef } from 'react';
import { t } from '@/utils/translations';
import type { GameMode } from '@/types/analog';
import { GAME_MODE_ICONS } from '../analogIcons';
import './GameModePickerModal.css';

interface Props {
  gameModes: GameMode[];
  selectedId: number | null;
  onSelect: (id: number) => void;
  onClose: () => void;
}

export function GameModePickerModal({ gameModes, selectedId, onSelect, onClose }: Props) {
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  useEffect(() => {
    history.pushState({ modal: true }, '');
    const handler = () => onCloseRef.current();
    window.addEventListener('popstate', handler);
    return () => window.removeEventListener('popstate', handler);
  }, []);

  const normalModes = gameModes.filter((gm) => !gm.isSolo);
  const soloModes   = gameModes.filter((gm) => gm.isSolo);

  const handleBackdrop = () => history.back();

  const selectMode = (id: number) => {
    onSelect(id);
    history.back();
  };

  return (
    <div className="gmp-overlay" onClick={handleBackdrop}>
      <div className="gmp-sheet" onClick={(e) => e.stopPropagation()}>
        <div className="gmp-header">
          <h2 className="gmp-title">{t.analogGameModeLabel}</h2>
          <button className="gmp-close" onClick={handleBackdrop} aria-label="Schließen">×</button>
        </div>

        <div className="gmp-body">
          <div className="gmp-section-label">Normalspiel</div>
          <div className="gmp-grid">
            {normalModes.map((gm) => (
              <button
                key={gm.id}
                className={`gmp-card${selectedId === gm.id ? ' gmp-card-selected' : ''}`}
                onClick={() => selectMode(gm.id)}
              >
                <span className="gmp-icon">{GAME_MODE_ICONS[gm.name] ?? '♣'}</span>
                <span className="gmp-name">{gm.name}</span>
              </button>
            ))}
          </div>

          <div className="gmp-section-label">Solo</div>
          <div className="gmp-grid">
            {soloModes.map((gm) => (
              <button
                key={gm.id}
                className={`gmp-card gmp-card-solo${selectedId === gm.id ? ' gmp-card-selected' : ''}`}
                onClick={() => selectMode(gm.id)}
              >
                <span className="gmp-icon">{GAME_MODE_ICONS[gm.name] ?? '♛'}</span>
                <span className="gmp-name">{gm.name}</span>
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
