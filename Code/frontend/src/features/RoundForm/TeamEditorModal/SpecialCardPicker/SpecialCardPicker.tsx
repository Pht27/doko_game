import { t } from '@/utils/translations';
import type { SpecialCard } from '@/types/analog';
import { SPECIAL_CARD_ICONS } from '../../analogIcons';

interface SpecialCardPickerProps {
  selectedIds: number[];
  specialCards: SpecialCard[];
  assignedSpecialCardIds: number[];
  onAdd: (id: number) => void;
  onRemove: (id: number) => void;
}

export function SpecialCardPicker({ selectedIds, specialCards, assignedSpecialCardIds, onAdd, onRemove }: SpecialCardPickerProps) {
  const selectedCards = specialCards.filter((sc) => selectedIds.includes(sc.id));
  const availableSpecialCards = specialCards.filter(
    (sc) => !selectedIds.includes(sc.id) && !assignedSpecialCardIds.includes(sc.id),
  );

  return (
    <div>
      <div className="tem-section-label">{t.analogTeamSpecialCardsSection}</div>

      {selectedCards.length > 0 && (
        <div className="tem-tag-list">
          {selectedCards.map((sc) => (
            <span key={sc.id} className="tem-tag">
              {SPECIAL_CARD_ICONS[sc.name] && (
                <span className="tem-tag-icon">{SPECIAL_CARD_ICONS[sc.name]}</span>
              )}
              {sc.name}
              <button
                className="tem-tag-remove"
                onClick={() => onRemove(sc.id)}
                aria-label={`${sc.name} entfernen`}
              >
                ✕
              </button>
            </span>
          ))}
        </div>
      )}

      {availableSpecialCards.length > 0 && (
        <div className="tem-chip-grid">
          {availableSpecialCards.map((sc) => (
            <button
              key={sc.id}
              className="tem-chip"
              onClick={() => onAdd(sc.id)}
            >
              {SPECIAL_CARD_ICONS[sc.name] && (
                <span className="tem-chip-icon">{SPECIAL_CARD_ICONS[sc.name]}</span>
              )}
              {sc.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
