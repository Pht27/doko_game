import { t } from '@/utils/translations';
import type { TeamBlockState } from '@/hooks/useRoundForm';
import type { ExtraPoint } from '@/types/analog';
import { EXTRA_POINT_ICONS } from '../../analogIcons';
import { ExtraPointRow } from './ExtraPointRow/ExtraPointRow';

interface ExtraPointsPickerProps {
  activeExtraPoints: TeamBlockState['extraPoints'];
  extraPoints: ExtraPoint[];
  onAdd: (id: number) => void;
  onUpdateCount: (id: number, delta: number) => void;
  onRemove: (id: number) => void;
}

export function ExtraPointsPicker({ activeExtraPoints, extraPoints, onAdd, onUpdateCount, onRemove }: ExtraPointsPickerProps) {
  const availableExtraPoints = extraPoints.filter(
    (ep) => !activeExtraPoints.some((e) => e.extraPointId === ep.id),
  );

  return (
    <div>
      <div className="tem-section-label">{t.analogTeamExtraPointsSection}</div>

      {activeExtraPoints.length > 0 && (
        <div className="tem-ep-list">
          {activeExtraPoints.map((ep) => {
            const def = extraPoints.find((e) => e.id === ep.extraPointId);
            return (
              <ExtraPointRow
                key={ep.extraPointId}
                name={def?.name ?? String(ep.extraPointId)}
                icon={def ? EXTRA_POINT_ICONS[def.name] : undefined}
                count={ep.count}
                onUpdateCount={(delta) => onUpdateCount(ep.extraPointId, delta)}
                onRemove={() => onRemove(ep.extraPointId)}
              />
            );
          })}
        </div>
      )}

      {availableExtraPoints.length > 0 && (
        <div className="tem-chip-grid">
          {availableExtraPoints.map((ep) => (
            <button
              key={ep.id}
              className="tem-chip"
              onClick={() => onAdd(ep.id)}
            >
              {EXTRA_POINT_ICONS[ep.name] && (
                <span className="tem-chip-icon">{EXTRA_POINT_ICONS[ep.name]}</span>
              )}
              {ep.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
