import { useRef, useState } from 'react';
import { t } from '@/utils/translations';
import type { PlayerListItem } from '@/types/analog';

interface PlayerPickerProps {
  selectedIds: number[];
  allPlayers: PlayerListItem[];
  assignedPlayerIds: number[];
  onSetPlayers: (ids: number[]) => void;
}

export function PlayerPicker({ selectedIds, allPlayers, assignedPlayerIds, onSetPlayers }: PlayerPickerProps) {
  const [search, setSearch] = useState('');
  const searchRef = useRef<HTMLInputElement>(null);

  const blockPlayers = allPlayers.filter((p) => selectedIds.includes(p.id));
  const availablePlayers = allPlayers.filter(
    (p) =>
      p.isActive &&
      !assignedPlayerIds.includes(p.id) &&
      !selectedIds.includes(p.id) &&
      p.name.toLowerCase().startsWith(search.toLowerCase()),
  );

  const addPlayer = (id: number) => {
    onSetPlayers([...selectedIds, id]);
    setSearch('');
    searchRef.current?.focus();
  };

  const removePlayer = (id: number) => {
    onSetPlayers(selectedIds.filter((pid) => pid !== id));
  };

  return (
    <div>
      <div className="tem-section-label">{t.analogTeamPlayersSection}</div>

      {blockPlayers.length > 0 && (
        <div className="tem-tag-list">
          {blockPlayers.map((p) => (
            <span key={p.id} className="tem-tag">
              {p.name}
              <button
                className="tem-tag-remove"
                onClick={() => removePlayer(p.id)}
                aria-label={`${p.name} entfernen`}
              >
                ✕
              </button>
            </span>
          ))}
        </div>
      )}

      {selectedIds.length < 2 && (
        <div className="tem-search-wrap">
          <input
            ref={searchRef}
            className="tem-search-input"
            type="text"
            placeholder={t.analogPlayerSearch}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <div className="tem-search-results">
              {availablePlayers.length === 0 ? (
                <div className="tem-search-empty">Keine Spieler gefunden</div>
              ) : (
                availablePlayers.map((p) => (
                  <button
                    key={p.id}
                    className="tem-search-result"
                    onClick={() => addPlayer(p.id)}
                  >
                    {p.name}
                  </button>
                ))
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
