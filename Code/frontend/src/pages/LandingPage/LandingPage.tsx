import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRegisterSW } from 'virtual:pwa-register/react';
import { t } from '@/utils/translations';
import { appVersion } from '@/utils/releaseNotes';
import { ReleaseNotesModal } from '@/components/ReleaseNotesModal/ReleaseNotesModal';

const RED_SUIT      = '#f87171';
const BLACK_SUIT    = 'rgba(255,255,255,0.75)';
const SURFACE       = 'rgba(255,255,255,0.05)';
const RE_BLUE       = '#6366f1';
const KONTRA_PURPLE = 'oklch(65% 0.23 303)';
const ORANGE        = '#fb923c';

const SUITS = {
  kreuz: { glyph: '♣', color: BLACK_SUIT, glow: 'rgba(99,102,241,0.5)',
           openBg: 'rgba(99,102,241,0.14)', openBorder: RE_BLUE },
  herz:  { glyph: '♥', color: RED_SUIT,   glow: 'rgba(248,113,113,0.5)',
           openBg: 'rgba(248,113,113,0.14)', openBorder: RED_SUIT },
  pik:   { glyph: '♠', color: BLACK_SUIT, glow: 'oklch(65% 0.23 303 / 0.55)',
           openBg: 'oklch(65% 0.23 303 / 0.16)', openBorder: KONTRA_PURPLE },
  karo:  { glyph: '♦', color: RED_SUIT,   glow: 'rgba(251,146,60,0.5)',
           openBg: 'rgba(251,146,60,0.12)', openBorder: ORANGE },
};

type Suit = {
  glyph: string; color: string; glow: string;
  openBg?: string; openBorder?: string;
};
type DrawerKey = 'kreuz' | 'pik' | 'herz' | 'karo' | null;

function Tile({
  suit, label, sub, expanded, onClick,
}: {
  suit: Suit; label: string; sub: string;
  expanded?: boolean; onClick?: () => void;
}) {
  const expandedBg     = suit.openBg     ?? SURFACE;
  const expandedBorder = suit.openBorder ?? 'rgba(255,255,255,0.2)';

  const bg          = expanded ? expandedBg : SURFACE;
  const borderColor = expanded ? expandedBorder : 'rgba(255,255,255,0.07)';
  const boxShadow   = expanded
    ? `0 0 0 1px ${expandedBorder}, 0 0 28px ${suit.glow}`
    : 'none';

  return (
    <button
      onClick={onClick}
      className="landing-tile"
      style={{
        aspectRatio: '1 / 1',
        borderRadius: 18,
        border: `1px solid ${borderColor}`,
        padding: 16,
        display: 'flex', flexDirection: 'column',
        alignItems: 'flex-start', justifyContent: 'space-between',
        color: '#fff', cursor: 'pointer', position: 'relative',
        fontFamily: 'inherit', textAlign: 'left', background: bg,
        boxShadow,
        transition: 'transform 0.15s, border-color 0.25s, background 0.25s, box-shadow 0.25s',
        WebkitTapHighlightColor: 'transparent',
      }}
    >
      {/* corner suit glyph */}
      <div style={{
        position: 'absolute', top: 10, left: 12,
        fontFamily: 'serif', lineHeight: 1,
        color: suit.color,
        opacity: expanded ? 0.95 : 0.65,
        fontSize: 14,
      }}>
        {suit.glyph}
      </div>

      {/* big center glyph */}
      <div style={{
        fontFamily: 'serif', fontSize: 60, lineHeight: 0.8,
        color: suit.color,
        opacity: expanded ? 1 : 0.5,
        textShadow: expanded ? `0 0 24px ${suit.glow}` : 'none',
        transition: 'opacity 0.25s, text-shadow 0.25s',
        alignSelf: 'flex-end',
        pointerEvents: 'none',
      }}>
        {suit.glyph}
      </div>

      {/* label block */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <div style={{ fontSize: 18, fontWeight: 700, letterSpacing: '-0.01em', color: '#fff' }}>
          {label}
        </div>
        <div style={{
          fontSize: 12, fontWeight: 500,
          color: expanded ? 'rgba(255,255,255,0.78)' : 'rgba(255,255,255,0.5)',
        }}>
          {sub}
        </div>
      </div>
    </button>
  );
}

function SubItem({ label, hint, onClick, disabled, hasDivider }: {
  label: string; hint: string; onClick?: () => void; disabled?: boolean; hasDivider?: boolean;
}) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      style={{
        background: 'transparent', border: 'none',
        borderBottom: hasDivider ? '1px solid rgba(255,255,255,0.08)' : 'none',
        padding: '11px 14px',
        color: disabled ? 'rgba(255,255,255,0.25)' : '#eee',
        display: 'flex', alignItems: 'center',
        borderRadius: hasDivider ? 0 : 10,
        cursor: disabled ? 'not-allowed' : 'pointer',
        fontFamily: 'inherit', textAlign: 'left', width: '100%',
      }}
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: 2, alignItems: 'flex-start' }}>
        <span style={{ fontSize: 14, fontWeight: 600, color: 'inherit' }}>{label}</span>
        <span style={{ fontSize: 11, color: 'rgba(255,255,255,0.5)' }}>{hint}</span>
      </div>
    </button>
  );
}

export function LandingPage() {
  const navigate = useNavigate();
  const [open, setOpen] = useState<DrawerKey>(null);
  const [showReleaseNotes, setShowReleaseNotes] = useState(false);
  const { needRefresh: [needRefresh], updateServiceWorker } = useRegisterSW();

  const toggle = (key: DrawerKey) => setOpen(prev => prev === key ? null : key);

  return (
    <div style={{
      width: '100%', height: '100%',
      background: '#1a1a2e',
      paddingTop: 28, paddingLeft: 22, paddingRight: 22, paddingBottom: 24,
      display: 'flex', flexDirection: 'column',
      color: '#eee', fontFamily: 'system-ui, -apple-system, "Segoe UI", Helvetica, Arial, sans-serif',
      position: 'relative', overflow: 'hidden',
      boxSizing: 'border-box',
    }}>

      <style>{`.landing-tile:active { transform: scale(0.97); }`}</style>

      {/* Faint corner suit watermarks */}
      <div style={{ position: 'absolute', inset: 0, pointerEvents: 'none', fontFamily: 'serif' }}>
        <span style={{ position: 'absolute', top: '6%',    left: '4%',   fontSize: 130, opacity: 0.045, lineHeight: 1, color: BLACK_SUIT }}>♣</span>
        <span style={{ position: 'absolute', top: '4%',    right: '3%',  fontSize: 130, opacity: 0.045, lineHeight: 1, color: RED_SUIT   }}>♥</span>
        <span style={{ position: 'absolute', bottom: '8%', left: '3%',   fontSize: 130, opacity: 0.045, lineHeight: 1, color: RED_SUIT   }}>♦</span>
        <span style={{ position: 'absolute', bottom: '6%', right: '4%',  fontSize: 130, opacity: 0.045, lineHeight: 1, color: BLACK_SUIT }}>♠</span>
      </div>

      {/* Header */}
      <div style={{ textAlign: 'center', marginBottom: 26, zIndex: 1 }}>
        <div style={{
          display: 'flex', justifyContent: 'center', gap: 10,
          fontSize: 16, opacity: 0.55, fontFamily: 'serif', marginBottom: 10,
        }}>
          <span style={{ color: BLACK_SUIT }}>♣</span>
          <span style={{ color: RED_SUIT   }}>♥</span>
          <span style={{ color: RED_SUIT   }}>♦</span>
          <span style={{ color: BLACK_SUIT }}>♠</span>
        </div>
        <h1 style={{
          fontSize: 38, fontWeight: 800, letterSpacing: '-0.02em',
          color: '#fff', lineHeight: 1, margin: 0,
        }}>
          {t.landingTitle}
        </h1>
      </div>

      {/* Spacer: schiebt Grid in die Mitte zwischen Titel und Pill */}
      <div style={{ flexGrow: 1 }} />

      {/* 2×2 grid */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, zIndex: 1 }}>

        {/* ♣ Kreuz — Eintragen */}
        <Tile suit={SUITS.kreuz} label="Eintragen" sub="Spieler und Runden verwalten"
          expanded={open === 'kreuz'} onClick={() => toggle('kreuz')} />

        {/* ♠ Pik — Spielen */}
        <Tile suit={SUITS.pik} label="Spielen" sub="Mehrspieler & Test"
          expanded={open === 'pik'} onClick={() => toggle('pik')} />

        {/* ♥ Herz — Übersicht */}
        <Tile suit={SUITS.herz} label="Übersicht" sub="Runden & Statistik"
          expanded={open === 'herz'} onClick={() => toggle('herz')} />

        {/* ♦ Karo — Regeln */}
        <Tile suit={SUITS.karo} label="Regeln" sub="& Regelsets"
          expanded={open === 'karo'} onClick={() => toggle('karo')} />

      </div>

      {/* Expansion drawer */}
      <div style={{
        overflow: 'hidden',
        maxHeight: open ? 160 : 0,
        opacity: open ? 1 : 0,
        marginTop: open ? 12 : 0,
        transition: 'max-height 0.3s ease, opacity 0.25s, margin-top 0.3s',
        zIndex: 1,
      }}>
        {open === 'kreuz' && (
          <div style={{
            background: SURFACE, borderRadius: 14, padding: '6px 6px',
            border: `1px solid ${SUITS.kreuz.openBorder}`,
            display: 'flex', flexDirection: 'column',
          }}>
            <SubItem label={t.landingSpielEintragen} hint="Runde aufschreiben" hasDivider
              onClick={() => navigate('/analog/new')} />
            <SubItem label={t.analogPlayersTitle} hint="Namen verwalten"
              onClick={() => navigate('/analog/players')} />
          </div>
        )}
        {open === 'pik' && (
          <div style={{
            background: SURFACE, borderRadius: 14, padding: '6px 6px',
            border: `1px solid ${SUITS.pik.openBorder}`,
            display: 'flex', flexDirection: 'column',
          }}>
            <SubItem label={t.multiplayer} hint="Online spielen" hasDivider
              onClick={() => navigate('/lobby')} />
            <SubItem label={t.testGame} hint="Allein ausprobieren"
              onClick={() => navigate('/hot-seat')} />
          </div>
        )}
        {open === 'herz' && (
          <div style={{
            background: SURFACE, borderRadius: 14, padding: '6px 6px',
            border: `1px solid ${SUITS.herz.openBorder}`,
            display: 'flex', flexDirection: 'column',
          }}>
            <SubItem label={t.landingRundenubersicht} hint="Vergangene Spiele" hasDivider
              onClick={() => navigate('/analog/history')} />
            <SubItem label={t.landingStats} hint="Deine Zahlen" disabled />
          </div>
        )}
        {open === 'karo' && (
          <div style={{
            background: SURFACE, borderRadius: 14, padding: '6px 6px',
            border: `1px solid ${SUITS.karo.openBorder}`,
            display: 'flex', flexDirection: 'column',
          }}>
            <SubItem label={t.rulesTitle} hint="Doppelkopf nachlesen" hasDivider
              onClick={() => navigate('/rules')} />
            <SubItem label={t.landingRegelsets} hint="Noch nicht verfügbar" disabled />
          </div>
        )}
      </div>

      {/* Spacer */}
      <div style={{ flexGrow: 1 }} />

      {/* Version pill */}
      <div
        onClick={() => setShowReleaseNotes(true)}
        style={{
          alignSelf: 'center',
          display: 'flex', alignItems: 'center', gap: 6,
          background: needRefresh ? 'rgba(99,102,241,0.15)' : SURFACE,
          border: needRefresh ? '1px solid rgba(99,102,241,0.45)' : '1px solid rgba(255,255,255,0.08)',
          borderRadius: 999,
          padding: '8px 14px',
          fontSize: 12, color: 'rgba(255,255,255,0.7)',
          cursor: 'pointer', zIndex: 1,
          transition: 'background 0.3s, border-color 0.3s',
        }}
      >
        {needRefresh && (
          <span style={{
            width: 7, height: 7, borderRadius: '50%',
            background: '#818cf8',
            boxShadow: '0 0 6px #818cf8',
            flexShrink: 0,
          }} />
        )}
        <span style={{ fontWeight: 600 }}>v{appVersion}</span>
        <span style={{ opacity: 0.55 }}>· {needRefresh ? 'Update verfügbar' : 'Was ist neu?'}</span>
      </div>

      {showReleaseNotes && (
        <ReleaseNotesModal
          onClose={() => setShowReleaseNotes(false)}
          needRefresh={needRefresh}
          updateSW={updateServiceWorker}
        />
      )}
    </div>
  );
}
