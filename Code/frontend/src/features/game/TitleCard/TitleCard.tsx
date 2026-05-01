import { useEffect } from 'react';
import { t } from '@/utils/translations';
import './TitleCard.css';

interface TitleCardProps {
  gameMode: string | null;
  declarerSeat: number | null;
  partnerSeat: number | null;
  exchangeCardCount?: number | null;
  returnedTrump?: boolean | null;
  onDone: () => void;
}

const MODE_ICONS: Record<string, string> = {
  Armut: '♦',
  Hochzeit: '♣',
  KaroSolo: '♦',
  KreuzSolo: '♣',
  PikSolo: '♠',
  HerzSolo: '♥',
  Damensolo: '♛',
  Bubensolo: '♝',
  Fleischloses: '∅',
  Knochenloses: '∅',
  SchlankerMartin: '✦',
};

const GLOW_COLORS: Record<string, string> = {
  solo: 'oklch(55% 0.18 75)',
  armut: 'oklch(50% 0.18 238)',
  hochzeit: 'oklch(50% 0.16 278)',
  normal: 'transparent',
};

export function TitleCard({ gameMode, declarerSeat, partnerSeat, exchangeCardCount, returnedTrump, onDone }: TitleCardProps) {
  useEffect(() => {
    const id = setTimeout(onDone, 2450);
    return () => clearTimeout(id);
  }, [onDone]);

  const isSolo = gameMode !== null && !['Armut', 'Hochzeit'].includes(gameMode);
  const isArmut = gameMode === 'Armut';
  const isHochzeit = gameMode === 'Hochzeit';

  const colClass = isSolo
    ? 'tc-solo'
    : isArmut
      ? 'tc-armut'
      : isHochzeit
        ? 'tc-hochzeit'
        : 'tc-normal';

  const glowKey = isSolo ? 'solo' : isArmut ? 'armut' : isHochzeit ? 'hochzeit' : 'normal';
  const label = t.gameModeLabel(gameMode);
  const icon = gameMode ? (MODE_ICONS[gameMode] ?? '🃏') : '🃏';
  const partnerFound = partnerSeat !== null;

  return (
    <div className="title-card">
      <div className="title-card-glow" style={{ background: GLOW_COLORS[glowKey] }} />
      <div className="title-card-inner">
        <div className="tc-icon">{icon}</div>
        <div className={`tc-mode ${colClass}`}>{label}</div>
        <div className="tc-line" />
        {(isArmut || isHochzeit) && declarerSeat !== null && (
          <div className="tc-players">
            <span className="tc-chip tc-chip-re">S{declarerSeat + 1}</span>
            {partnerFound
              ? <span className="tc-chip tc-chip-re">S{partnerSeat! + 1}</span>
              : <span className="tc-chip tc-chip-unk">?</span>
            }
          </div>
        )}
        {isSolo && declarerSeat !== null && (
          <div className="tc-players">
            <span className="tc-chip tc-chip-solo">S{declarerSeat + 1}</span>
          </div>
        )}
        {isArmut && exchangeCardCount != null && returnedTrump != null && (
          <div className="tc-exchange-info">{t.armutExchangeInfo(exchangeCardCount, returnedTrump)}</div>
        )}
      </div>
    </div>
  );
}
