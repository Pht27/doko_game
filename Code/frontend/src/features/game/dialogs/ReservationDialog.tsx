import { useState } from 'react';
import { t } from '@/utils/translations';
import './ReservationDialog.css';

interface ReservationDialogProps {
  playerId: number;
  eligibleReservations: string[];
  mustDeclare?: boolean;
  onDeclare: (reservation: string | null, hochzeitCondition: string | null, armutPartner: number | null) => void;
}

const SOLO_RESERVATIONS = new Set([
  'KaroSolo', 'KreuzSolo', 'PikSolo', 'HerzSolo',
  'Damensolo', 'Bubensolo', 'Fleischloses', 'Knochenloses',
]);

const HOCHZEIT_CONDITIONS = ['FirstTrick', 'FirstFehlTrick', 'FirstTrumpTrick'];

type Category = 'Solo' | 'SchlankerMartin' | 'Armut' | 'Schmeissen' | 'Hochzeit';

function getCategories(eligibleReservations: string[]): Category[] {
  const cats: Category[] = [];
  if (eligibleReservations.some((r) => SOLO_RESERVATIONS.has(r))) cats.push('Solo');
  if (eligibleReservations.includes('SchlankerMartin')) cats.push('SchlankerMartin');
  if (eligibleReservations.includes('Armut')) cats.push('Armut');
  if (eligibleReservations.includes('Schmeissen')) cats.push('Schmeissen');
  if (eligibleReservations.includes('Hochzeit')) cats.push('Hochzeit');
  return cats;
}

export function ReservationDialog({ playerId, eligibleReservations, mustDeclare = false, onDeclare }: ReservationDialogProps) {
  const categories = getCategories(eligibleReservations);
  const [selectedCategory, setSelectedCategory] = useState<Category>(categories[0] ?? 'Solo');

  const eligibleSolos = eligibleReservations.filter((r) => SOLO_RESERVATIONS.has(r));

  return (
    <div className="rd-dialog">
      <div className="rd-header">
        <span className="rd-title">{t.reservationTitle(playerId)}</span>
        {!mustDeclare && (
          <button className="rd-pass-btn" onClick={() => onDeclare(null, null, null)}>
            {t.pass}
          </button>
        )}
      </div>

      {categories.length > 0 && (
        <div className="rd-body">
          {/* Left column: categories */}
          <div className="rd-left">
            {categories.map((cat) => (
              <button
                key={cat}
                className={`rd-cat-btn ${selectedCategory === cat ? 'rd-cat-btn-active' : ''}`}
                onClick={() => setSelectedCategory(cat)}
              >
                {t.reservationCategoryLabel(cat)}
              </button>
            ))}
          </div>

          {/* Right column: detail options */}
          <div className="rd-right">
            {selectedCategory === 'Solo' && eligibleSolos.map((res) => (
              <button
                key={res}
                className="rd-detail-btn"
                onClick={() => onDeclare(res, null, null)}
              >
                {t.soloLabel(res)}
              </button>
            ))}

            {selectedCategory === 'Hochzeit' && HOCHZEIT_CONDITIONS.map((cond) => (
              <button
                key={cond}
                className="rd-detail-btn"
                onClick={() => onDeclare('Hochzeit', cond, null)}
              >
                {t.hochzeitConditionLabel(cond)}
              </button>
            ))}

            {(selectedCategory === 'SchlankerMartin' ||
              selectedCategory === 'Armut' ||
              selectedCategory === 'Schmeissen') && (
              <button
                className="rd-detail-btn rd-detail-btn-confirm"
                onClick={() => onDeclare(selectedCategory, null, null)}
              >
                {t.bestaetigenSolo}
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
