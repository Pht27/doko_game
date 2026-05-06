import { useEffect, useRef, useState } from 'react';
import { t } from '@/utils/translations';
import type { TeamBlockState } from '@/hooks/useRoundForm';
import type { PlayerListItem, SpecialCard, ExtraPoint } from '@/types/analog';
import './TeamEditorModal.css';

interface Props {
  block: TeamBlockState;
  blockIndex: number;
  allPlayers: PlayerListItem[];
  specialCards: SpecialCard[];
  extraPoints: ExtraPoint[];
  assignedPlayerIds: number[]; // all player IDs in use across all other blocks
  onClose: () => void;
  onSetPlayers: (ids: number[]) => void;
  onAddSpecialCard: (id: number) => void;
  onRemoveSpecialCard: (id: number) => void;
  onAddExtraPoint: (id: number) => void;
  onUpdateExtraPointCount: (id: number, delta: number) => void;
  onRemoveExtraPoint: (id: number) => void;
}

export function TeamEditorModal({
  block,
  allPlayers,
  specialCards,
  extraPoints,
  assignedPlayerIds,
  onClose,
  onSetPlayers,
  onAddSpecialCard,
  onRemoveSpecialCard,
  onAddExtraPoint,
  onUpdateExtraPointCount,
  onRemoveExtraPoint,
}: Props) {
  const [search, setSearch] = useState('');
  const [showScDropdown, setShowScDropdown] = useState(false);
  const [showEpDropdown, setShowEpDropdown] = useState(false);
  const searchRef = useRef<HTMLInputElement>(null);
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  // Browser-back closes modal — use ref so pushState only fires once
  useEffect(() => {
    history.pushState({ modal: true }, '');
    const handler = () => onCloseRef.current();
    window.addEventListener('popstate', handler);
    return () => window.removeEventListener('popstate', handler);
  }, []);

  const availablePlayers = allPlayers.filter(
    (p) =>
      p.isActive &&
      !assignedPlayerIds.includes(p.id) &&
      !block.playerIds.includes(p.id) &&
      p.name.toLowerCase().startsWith(search.toLowerCase()),
  );

  const availableSpecialCards = specialCards.filter(
    (sc) => !block.specialCardIds.includes(sc.id),
  );

  const availableExtraPoints = extraPoints.filter(
    (ep) => !block.extraPoints.some((e) => e.extraPointId === ep.id),
  );

  const addPlayer = (id: number) => {
    onSetPlayers([...block.playerIds, id]);
    setSearch('');
    searchRef.current?.focus();
  };

  const removePlayer = (id: number) => {
    onSetPlayers(block.playerIds.filter((pid) => pid !== id));
  };

  const handleBackdrop = () => {
    history.back();
  };

  const blockPlayers = allPlayers.filter((p) => block.playerIds.includes(p.id));
  const blockSpecialCards = specialCards.filter((sc) => block.specialCardIds.includes(sc.id));

  return (
    <div className="tem-overlay" onClick={handleBackdrop}>
      <div className="tem-sheet" onClick={(e) => e.stopPropagation()}>
        <div className="tem-header">
          <h2 className="tem-title">{t.analogTeamEditTitle}</h2>
          <button className="tem-close" onClick={handleBackdrop} aria-label="Schließen">
            ×
          </button>
        </div>

        <div className="tem-body">
          {/* ── Spieler ── */}
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

            {block.playerIds.length < 2 && (
              <div className="tem-search-wrap">
                <input
                  ref={searchRef}
                  className="tem-search-input"
                  type="text"
                  placeholder={t.analogPlayerSearch}
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  autoFocus
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

          {/* ── Sonderkarten ── */}
          <div>
            <div className="tem-section-label">{t.analogTeamSpecialCardsSection}</div>

            {blockSpecialCards.length > 0 && (
              <div className="tem-tag-list">
                {blockSpecialCards.map((sc) => (
                  <span key={sc.id} className="tem-tag">
                    {sc.name}
                    <button
                      className="tem-tag-remove"
                      onClick={() => onRemoveSpecialCard(sc.id)}
                      aria-label={`${sc.name} entfernen`}
                    >
                      ✕
                    </button>
                  </span>
                ))}
              </div>
            )}

            {availableSpecialCards.length > 0 && (
              <div className="tem-dropdown-wrap">
                <button
                  className="tem-add-btn"
                  onClick={() => setShowScDropdown((v) => !v)}
                >
                  {t.analogAddSpecialCard}
                </button>
                {showScDropdown && (
                  <div className="tem-dropdown">
                    {availableSpecialCards.map((sc) => (
                      <button
                        key={sc.id}
                        className="tem-dropdown-item"
                        onClick={() => {
                          onAddSpecialCard(sc.id);
                          setShowScDropdown(false);
                        }}
                      >
                        {sc.name}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* ── Extrapunkte ── */}
          <div>
            <div className="tem-section-label">{t.analogTeamExtraPointsSection}</div>

            {block.extraPoints.length > 0 && (
              <div className="tem-ep-list">
                {block.extraPoints.map((ep) => {
                  const def = extraPoints.find((e) => e.id === ep.extraPointId);
                  return (
                    <div key={ep.extraPointId} className="tem-ep-row">
                      <span className="tem-ep-name">{def?.name ?? ep.extraPointId}</span>
                      <div className="tem-ep-counter">
                        <button
                          className="tem-ep-count-btn"
                          onClick={() => onUpdateExtraPointCount(ep.extraPointId, -1)}
                          aria-label="Weniger"
                        >
                          −
                        </button>
                        <span className="tem-ep-count">{ep.count}</span>
                        <button
                          className="tem-ep-count-btn"
                          onClick={() => onUpdateExtraPointCount(ep.extraPointId, 1)}
                          aria-label="Mehr"
                        >
                          +
                        </button>
                      </div>
                      <button
                        className="tem-ep-remove"
                        onClick={() => onRemoveExtraPoint(ep.extraPointId)}
                        aria-label="Entfernen"
                      >
                        ✕
                      </button>
                    </div>
                  );
                })}
              </div>
            )}

            {availableExtraPoints.length > 0 && (
              <div className="tem-dropdown-wrap">
                <button
                  className="tem-add-btn"
                  onClick={() => setShowEpDropdown((v) => !v)}
                >
                  {t.analogAddExtraPoint}
                </button>
                {showEpDropdown && (
                  <div className="tem-dropdown">
                    {availableExtraPoints.map((ep) => (
                      <button
                        key={ep.id}
                        className="tem-dropdown-item"
                        onClick={() => {
                          onAddExtraPoint(ep.id);
                          setShowEpDropdown(false);
                        }}
                      >
                        {ep.name}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
