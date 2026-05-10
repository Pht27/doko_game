import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRegisterSW } from 'virtual:pwa-register/react';
import { t } from '@/utils/translations';
import { appVersion } from '@/utils/releaseNotes';
import { ReleaseNotesModal } from '@/components/ReleaseNotesModal/ReleaseNotesModal';
import { Tile } from './Tile/Tile';
import { SubItem } from './SubItem/SubItem';
import './LandingPage.css';

const RED_SUIT      = '#f87171';
const BLACK_SUIT    = 'rgba(255,255,255,0.75)';
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

type DrawerKey = 'kreuz' | 'pik' | 'herz' | 'karo' | null;

export function LandingPage() {
  const navigate = useNavigate();
  const [open, setOpen] = useState<DrawerKey>(null);
  const [showReleaseNotes, setShowReleaseNotes] = useState(false);
  const { needRefresh: [needRefresh], updateServiceWorker } = useRegisterSW();
  const lastOpenRef = useRef<NonNullable<DrawerKey>>('kreuz');

  const toggle = (key: DrawerKey) => setOpen(prev => prev === key ? null : key);
  if (open !== null) lastOpenRef.current = open;
  const displayKey = open ?? lastOpenRef.current;

  return (
    <div className="landing-page">

      <div className="landing-watermarks">
        <span className="landing-watermark landing-watermark--tl">♣</span>
        <span className="landing-watermark landing-watermark--tr">♥</span>
        <span className="landing-watermark landing-watermark--bl">♦</span>
        <span className="landing-watermark landing-watermark--br">♠</span>
      </div>

      <div className="landing-header">
        <div className="landing-header-suits">
          <span className="suit-black">♣</span>
          <span className="suit-red">♥</span>
          <span className="suit-red">♦</span>
          <span className="suit-black">♠</span>
        </div>
        <h1 className="landing-title">{t.landingTitle}</h1>
      </div>

      <div className="landing-spacer" />

      <div className="landing-grid">
        <Tile suit={SUITS.kreuz} label={t.landingTileEintragen} sub={t.landingTileSubEintragen}
          expanded={open === 'kreuz'} onClick={() => toggle('kreuz')} />
        <Tile suit={SUITS.pik} label={t.landingTileSpielen} sub={t.landingTileSubSpielen}
          expanded={open === 'pik'} onClick={() => toggle('pik')} />
        <Tile suit={SUITS.herz} label={t.landingTileUbersicht} sub={t.landingTileSubUbersicht}
          expanded={open === 'herz'} onClick={() => toggle('herz')} />
        <Tile suit={SUITS.karo} label={t.rulesTitle} sub={t.landingTileSubRegeln}
          expanded={open === 'karo'} onClick={() => toggle('karo')} />
      </div>

      {/* grid-template-rows animates smoothly with no dead zone */}
      <div
        className="landing-drawer"
        style={{ gridTemplateRows: open ? '1fr' : '0fr', marginTop: open ? 12 : 0 }}
      >
        <div className="landing-drawer-inner">
          {displayKey === 'kreuz' && (
            <div className="landing-drawer-panel" style={{ borderColor: SUITS.kreuz.openBorder }}>
              <SubItem label={t.landingSpielEintragen} hint={t.landingHintSpielEintragen} hasDivider
                onClick={() => navigate('/analog/new')} />
              <SubItem label={t.analogPlayersTitle} hint={t.landingHintSpieler}
                onClick={() => navigate('/players')} />
            </div>
          )}
          {displayKey === 'pik' && (
            <div className="landing-drawer-panel" style={{ borderColor: SUITS.pik.openBorder }}>
              <SubItem label={t.multiplayer} hint={t.landingHintMultiplayer} hasDivider
                onClick={() => navigate('/lobby')} />
              <SubItem label={t.testGame} hint={t.landingHintTestGame}
                onClick={() => navigate('/hot-seat')} />
            </div>
          )}
          {displayKey === 'herz' && (
            <div className="landing-drawer-panel" style={{ borderColor: SUITS.herz.openBorder }}>
              <SubItem label={t.landingRundenubersicht} hint={t.landingHintRundenubersicht} hasDivider
                onClick={() => navigate('/history')} />
              <SubItem label={t.landingStats} hint={t.landingHintStats}
                onClick={() => navigate('/leaderboard')} />
            </div>
          )}
          {displayKey === 'karo' && (
            <div className="landing-drawer-panel" style={{ borderColor: SUITS.karo.openBorder }}>
              <SubItem label={t.rulesTitle} hint={t.landingHintRegeln} hasDivider
                onClick={() => navigate('/rules')} />
              <SubItem label={t.landingRegelsets} hint={t.landingHintRegelsets} disabled />
            </div>
          )}
        </div>
      </div>

      <div className="landing-spacer" />

      <div
        onClick={() => setShowReleaseNotes(true)}
        className={`version-pill${needRefresh ? ' update-pill' : ''}`}
      >
        {needRefresh && <span className="version-pill-dot update-dot" />}
        <span className="version-pill-version">v{appVersion}</span>
        <span className="version-pill-label">· {needRefresh ? t.landingUpdateAvailable : t.landingWhatsNew}</span>
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
