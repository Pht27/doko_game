import { useRef } from 'react';
import { t } from '@/utils/translations';
import type { TeamBlockState, Party } from '@/hooks/useRoundForm';
import type { PlayerListItem, SpecialCard, ExtraPoint } from '@/types/analog';

interface Props {
  block: TeamBlockState;
  winningParty: Party | null;
  allPlayers: PlayerListItem[];
  specialCards: SpecialCard[];
  extraPoints: ExtraPoint[];
  animateOnLoad?: boolean;
  onSwitch: () => void;
  onEdit: () => void;
}

const SWIPE_THRESHOLD = 50;

export function TeamBlock({
  block,
  winningParty,
  allPlayers,
  specialCards,
  extraPoints,
  animateOnLoad,
  onSwitch,
  onEdit,
}: Props) {
  const touchStartX = useRef<number | null>(null);
  const didSwipe = useRef(false);

  const players = allPlayers.filter((p) => block.playerIds.includes(p.id));
  const blockSpecialCards = specialCards.filter((sc) => block.specialCardIds.includes(sc.id));
  const blockExtraPoints = block.extraPoints.map((ep) => ({
    ...ep,
    name: extraPoints.find((e) => e.id === ep.extraPointId)?.name ?? String(ep.extraPointId),
  }));

  const isWinner = winningParty !== null && block.party === winningParty;
  const isLoser  = winningParty !== null && block.party !== winningParty;

  const handleTouchStart = (e: React.TouchEvent) => {
    touchStartX.current = e.touches[0].clientX;
    didSwipe.current = false;
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (touchStartX.current === null) return;
    const dx = e.changedTouches[0].clientX - touchStartX.current;
    touchStartX.current = null;

    if (block.party === 'Re' && dx > SWIPE_THRESHOLD) {
      didSwipe.current = true;
      onSwitch();
    } else if (block.party === 'Kontra' && dx < -SWIPE_THRESHOLD) {
      didSwipe.current = true;
      onSwitch();
    }
  };

  const isEmpty = players.length === 0 && blockSpecialCards.length === 0 && blockExtraPoints.length === 0;

  return (
    <button
      className={[
        'arf-team-block',
        isWinner ? 'arf-winning' : '',
        isLoser ? 'arf-losing' : '',
        animateOnLoad ? 'arf-swipe-animate' : '',
      ].filter(Boolean).join(' ')}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
      onClick={() => { if (!didSwipe.current) onEdit(); }}
      aria-label={`Team ${block.party} bearbeiten`}
    >
      {isEmpty ? (
        <span className="arf-team-empty">{t.analogTeamAdd}</span>
      ) : (
        <div className="arf-team-info">
          {players.length > 0 && (
            <div className="arf-team-name">{players.map((p) => p.name).join(', ')}</div>
          )}
          {blockSpecialCards.length > 0 && (
            <div className="arf-team-section">
              <span className="arf-team-section-label">Sonderkarten:</span>
              {blockSpecialCards.map((sc) => (
                <span key={sc.id} className="arf-team-section-item">{sc.name}</span>
              ))}
            </div>
          )}
          {blockExtraPoints.length > 0 && (
            <div className="arf-team-section">
              <span className="arf-team-section-label">Extrapunkte:</span>
              {blockExtraPoints.map((ep) => (
                <span key={ep.extraPointId} className="arf-team-section-item">
                  {ep.name}{ep.count > 1 ? ` (${ep.count})` : ''}
                </span>
              ))}
            </div>
          )}
        </div>
      )}
    </button>
  );
}
