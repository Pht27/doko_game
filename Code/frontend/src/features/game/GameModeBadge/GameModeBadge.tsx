import { useState, useEffect } from 'react';
import type { SonderkarteNotification } from '@/types/api';
import { t } from '@/utils/translations';
import './GameModeBadge.css';

interface GameModeBadgeProps {
  gameMode: string | null;
  declarerSeat: number | null;
  partnerSeat: number | null;
  trickNumber: number;
  totalTricks: number;
  activeSonderkarten: SonderkarteNotification[];
  isSchwarzesSau?: boolean;
  phase?: string;
}

const PHASE_INFO: Record<string, { icon: string; label: string; cssClass: string }> = {
  ReservationHealthCheck:   { icon: '◎', label: 'Gesundheitsabfrage', cssClass: 'gmb-ph-health' },
  ReservationSoloCheck:     { icon: '♛', label: 'Solo?',              cssClass: 'gmb-ph-solo' },
  ReservationArmutCheck:    { icon: '♦', label: 'Armut?',             cssClass: 'gmb-ph-armut' },
  ReservationSchmeissenCheck:{ icon: '✕', label: 'Schmeißen?',        cssClass: 'gmb-ph-schmeissen' },
  ReservationHochzeitCheck: { icon: '♣', label: 'Hochzeit?',          cssClass: 'gmb-ph-hochzeit' },
  ArmutPartnerFinding:      { icon: '♦', label: 'Armut-Partner',      cssClass: 'gmb-ph-armut' },
  ArmutCardExchange:        { icon: '↔', label: 'Armut-Tausch',       cssClass: 'gmb-ph-armut' },
  SchwarzesSauSoloSelect:   { icon: '♠', label: 'Schwarze Sau',       cssClass: 'gmb-ph-schmeissen' },
};

const MODE_ICONS: Record<string, string> = {
  Armut: '♦',
  Hochzeit: '♣',
  KaroSolo: '♦',
  KreuzSolo: '♣',
  PikSolo: '♠',
  HerzSolo: '♥',
  Damensolo: 'D',
  Bubensolo: 'B',
  Fleischloses: '∅',
  Knochenloses: '∅',
  SchlankerMartin: '✦',
};

const SK_ICONS: Record<string, string> = {
  Schweinchen: '♦A',
  Superschweinchen: '♦10',
  Hyperschweinchen: '♦K',
  LinksGehangter: '↺',
  RechtsGehangter: '↻',
  Genscherdamen: '♥♥',
  Gegengenscherdamen: '♦♦',
  Heidmann: '⚔',
  Heidfrau: '🛡',
  Kemmerich: '♦B',
};

export function GameModeBadge({
  gameMode,
  declarerSeat,
  partnerSeat,
  trickNumber,
  totalTricks,
  activeSonderkarten,
  isSchwarzesSau,
  phase,
}: GameModeBadgeProps) {
  const [expanded, setExpanded] = useState(false);

  const isSolo = gameMode !== null && !['Armut', 'Hochzeit'].includes(gameMode);
  const isArmut = gameMode === 'Armut';
  const isHochzeit = gameMode === 'Hochzeit';
  const isNormal = gameMode === null && !isSchwarzesSau;

  // nachdem ein Solo in der Schwarzen Sau gewählt wurde, soll es nicht mehr als Schwarze Sau angezeigt werden, auch wenn die Bedingungen für die Schwarze Sau weiterhin erfüllt sind
  isSchwarzesSau = isSchwarzesSau && isNormal;

  const phaseInfo = phase ? (PHASE_INFO[phase] ?? null) : null;

  const modeClass = phaseInfo
    ? phaseInfo.cssClass
    : isSchwarzesSau
      ? 'gmb-schwarze-sau'
      : isNormal
        ? 'gmb-normal'
        : isSolo
          ? 'gmb-solo'
          : isArmut
            ? 'gmb-armut'
            : 'gmb-hochzeit';

  const label = phaseInfo ? phaseInfo.label : (isSchwarzesSau ? 'Schwarze Sau' : t.gameModeLabel(gameMode));
  const icon = phaseInfo ? phaseInfo.icon : (isSchwarzesSau ? '♠' : (gameMode ? (MODE_ICONS[gameMode] ?? null) : null));
  const trickDisplay = totalTricks > 0 ? `${trickNumber}/${totalTricks}` : `${trickNumber}`;
  const skCount = activeSonderkarten.length;
  const partnerFound = partnerSeat !== null;
  useEffect(() => {
    if (!expanded) return;
    const close = () => setExpanded(false);
    const id = setTimeout(() => window.addEventListener('click', close), 80);
    return () => { clearTimeout(id); window.removeEventListener('click', close); };
  }, [expanded]);

  return (
    <div
      className={`gmb ${modeClass}`}
      onClick={e => { e.stopPropagation(); setExpanded(v => !v); }}
      role="button"
    >
      {/* Always-visible pill row */}
      <div className="gmb-pill-row">
        {icon && <span className="gmb-icon">{icon}</span>}
        <span className="gmb-label">{label}</span>

        {isSolo && declarerSeat !== null && (
          <div className="gmb-players">
            <span className="gmb-dot">·</span>
            <span className="gmb-chip gmb-chip-solo">S{declarerSeat + 1}</span>
          </div>
        )}
        {(isArmut || isHochzeit) && declarerSeat !== null && (
          <div className="gmb-players">
            <span className="gmb-dot">·</span>
            <span className="gmb-chip gmb-chip-re">S{declarerSeat + 1}</span>
            {partnerFound
              ? <span className="gmb-chip gmb-chip-re">S{partnerSeat! + 1}</span>
              : <span className="gmb-chip gmb-chip-unk">?</span>
            }
          </div>
        )}

        <span className="gmb-dot">·</span>
        <span className="gmb-trick">{trickDisplay}</span>

        {skCount > 0 && <span className="gmb-sk-count">{skCount}</span>}
        <span className={`gmb-chevron${expanded ? ' gmb-chevron-open' : ''}`}>▾</span>
      </div>

      {/* Expandable sonderkarten section */}
      <div className={`gmb-sk-section${expanded ? ' gmb-sk-section-open' : ''}`}>
        <div className="gmb-sk-inner">
          <div className="gmb-sk-divider" />
          <div className="gmb-sk-list">
            <div className="gmb-sk-title">Sonderkarten</div>
            {skCount === 0 && <div className="gmb-sk-empty">Keine aktiv</div>}
            {activeSonderkarten.map((sk, i) => (
              <div className="gmb-sk-row" key={i}>
                <span className="gmb-sk-icon">{SK_ICONS[sk.type] ?? '✦'}</span>
                <span className="gmb-sk-name">{t.sonderkarteName(sk.type)}</span>
                <span className="gmb-sk-player">S{sk.player + 1}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
