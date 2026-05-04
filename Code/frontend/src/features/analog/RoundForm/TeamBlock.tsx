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
  align: 'left' | 'right';
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
  align,
  onSwitch,
  onEdit,
}: Props) {
  const touchStartX = useRef<number | null>(null);
  const didSwipe = useRef(false);

  const players = allPlayers.filter((p) => block.playerIds.includes(p.id));

  const isWinner =
    winningParty !== null && block.party === winningParty;
  const isLoser =
    winningParty !== null && block.party !== winningParty;

  const blockSpecialCards = specialCards.filter((sc) => block.specialCardIds.includes(sc.id));
  const blockExtraPoints = block.extraPoints.map((ep) => ({
    ...ep,
    name: extraPoints.find((e) => e.id === ep.extraPointId)?.name ?? String(ep.extraPointId),
  }));

  const handleTouchStart = (e: React.TouchEvent) => {
    touchStartX.current = e.touches[0].clientX;
    didSwipe.current = false;
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (touchStartX.current === null) return;
    const dx = e.changedTouches[0].clientX - touchStartX.current;
    touchStartX.current = null;

    const isReBlock = block.party === 'Re';
    const isKontraBlock = block.party === 'Kontra';

    // Re → swipe right; Kontra → swipe left
    if (isReBlock && dx > SWIPE_THRESHOLD) {
      didSwipe.current = true;
      onSwitch();
    } else if (isKontraBlock && dx < -SWIPE_THRESHOLD) {
      didSwipe.current = true;
      onSwitch();
    }
  };

  const handleClick = () => {
    if (!didSwipe.current) onEdit();
  };

  const bgStyle: React.CSSProperties = {
    backgroundColor: isWinner
      ? 'rgba(74, 222, 128, 0.12)'
      : isLoser
      ? 'rgba(248, 113, 113, 0.12)'
      : 'rgba(255,255,255,0.04)',
  };

  const isEmpty = players.length === 0;

  return (
    <button
      style={bgStyle}
      className={`arf-team-block ${align === 'right' ? 'arf-team-block-right' : ''}`}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
      onClick={handleClick}
      aria-label={`Team ${block.party} bearbeiten`}
    >
      {isEmpty ? (
        <span className="arf-team-empty">{t.analogTeamAdd}</span>
      ) : (
        <>
          <span className="arf-team-players">
            {players.map((p) => p.name).join(' & ')}
          </span>
          {(blockSpecialCards.length > 0 || blockExtraPoints.length > 0) && (
            <span className="arf-team-extras">
              {blockSpecialCards.map((sc) => sc.name).join(', ')}
              {blockSpecialCards.length > 0 && blockExtraPoints.length > 0 && ' · '}
              {blockExtraPoints
                .map((ep) => (ep.count > 1 ? `${ep.name} ×${ep.count}` : ep.name))
                .join(', ')}
            </span>
          )}
        </>
      )}
    </button>
  );
}
