import { useState } from 'react';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';
import { ToggleSwitch } from '@/components/ToggleSwitch/ToggleSwitch';
import './PlayerPickerModal.css';

interface PlayerPickerModalProps {
  players: PlayerListItem[];
  visibleIds: Set<number>;
  colorMap: Map<number, string>;
  onConfirm: (ids: Set<number>) => void;
  onClose: () => void;
}

export function PlayerPickerModal({ players, visibleIds, colorMap, onConfirm, onClose }: PlayerPickerModalProps) {
  const [selected, setSelected] = useState(() => new Set(visibleIds));

  const toggle = (id: number) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleConfirm = () => {
    onConfirm(selected);
    onClose();
  };

  const active = players.filter((p) => p.isActive);
  const inactive = players.filter((p) => !p.isActive);

  const renderRow = (p: PlayerListItem) => {
    const on = selected.has(p.id);
    return (
      <div
        key={p.id}
        className={`ppm-item${!p.isActive ? ' ppm-item--inactive' : ''}`}
        onClick={() => toggle(p.id)}
      >
        <span className="ppm-dot" style={{ background: colorMap.get(p.id) ?? 'var(--app-border-md)' }} />
        <span className="ppm-name">{p.name}</span>
        <ToggleSwitch on={on} onChange={() => toggle(p.id)} size="sm" stopPropagation />
      </div>
    );
  };

  return (
    <div className="ppm-backdrop" onClick={onClose}>
      <div className="ppm-modal" onClick={(e) => e.stopPropagation()}>
        <div className="ppm-header">
          <span className="ppm-title">{t.analogLeaderboardGraphPlayers}</span>
          <button className="ppm-done" onClick={handleConfirm}>
            {t.analogLeaderboardGraphDone}
          </button>
        </div>

        <div className="ppm-list">
          {active.map(renderRow)}

          {inactive.length > 0 && (
            <>
              <div className="ppm-sep">{t.analogLeaderboardGraphInactivePlayers}</div>
              {inactive.map(renderRow)}
            </>
          )}
        </div>
      </div>
    </div>
  );
}
