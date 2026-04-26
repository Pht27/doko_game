import type { CSSProperties } from 'react';
import type { CardDto } from '@/types/api';
import { cardSvgPath, cardBackSvgPath } from '@/api/cards';
import { t } from '@/utils/translations';

interface CardProps {
  card: CardDto;
  faceDown?: boolean;
  className?: string;
  style?: CSSProperties;
  onClick?: () => void;
  alt?: string;
}

export function Card({ card, faceDown = false, className, style, onClick, alt }: CardProps) {
  const src = faceDown ? cardBackSvgPath : cardSvgPath(card.suit, card.rank);
  const defaultAlt = faceDown ? 'Kartenrücken' : t.cardAlt(card.rank, card.suit);

  return (
    <img
      src={src}
      alt={alt ?? defaultAlt}
      className={className}
      style={style}
      onClick={onClick}
    />
  );
}
