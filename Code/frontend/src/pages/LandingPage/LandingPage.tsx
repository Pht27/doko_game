import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRegisterSW } from 'virtual:pwa-register/react';
import { t } from '@/utils/translations';
import { appVersion } from '@/utils/releaseNotes';
import { ReleaseNotesModal } from '@/components/ReleaseNotesModal/ReleaseNotesModal';
import { BottomSheet } from '@/components/BottomSheet/BottomSheet';
import { useTheme } from '@/hooks/useTheme';
import { Tile } from './Tile/Tile';
import { SubItem } from './SubItem/SubItem';
import { ThemePicker } from './ThemePicker/ThemePicker';
import './LandingPage.css';

type DrawerKey = 'kreuz' | 'pik' | 'herz' | 'karo' | null;

export function LandingPage() {
  const navigate = useNavigate();
  const [open, setOpen] = useState<DrawerKey>(null);
  const [showReleaseNotes, setShowReleaseNotes] = useState(false);
  const [showThemePicker, setShowThemePicker] = useState(false);
  const { needRefresh: [needRefresh], updateServiceWorker } = useRegisterSW();
  const lastOpenRef = useRef<NonNullable<DrawerKey>>('kreuz');
  const { setPreset, activePreset } = useTheme();

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

      <div className="landing-bottom-left">
        <button
          className="landing-palette-btn"
          onClick={() => setShowThemePicker(true)}
          aria-label="Theme wählen"
        >
          🎨
        </button>
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
        <Tile suit="kreuz" label={t.landingTileEintragen} sub={t.landingTileSubEintragen}
          expanded={open === 'kreuz'} onClick={() => toggle('kreuz')} />
        <Tile suit="pik" label={t.landingTileSpielen} sub={t.landingTileSubSpielen}
          expanded={open === 'pik'} onClick={() => toggle('pik')} />
        <Tile suit="herz" label={t.landingTileUbersicht} sub={t.landingTileSubUbersicht}
          expanded={open === 'herz'} onClick={() => toggle('herz')} />
        <Tile suit="karo" label={t.rulesTitle} sub={t.landingTileSubRegeln}
          expanded={open === 'karo'} onClick={() => toggle('karo')} />
      </div>

      {/* grid-template-rows animates smoothly with no dead zone */}
      <div
        className="landing-drawer"
        style={{ gridTemplateRows: open ? '1fr' : '0fr', marginTop: open ? 12 : 0 }}
      >
        <div className="landing-drawer-inner">
          {displayKey === 'kreuz' && (
            <div className="landing-drawer-panel landing-drawer-panel--kreuz">
              <SubItem label={t.landingSpielEintragen} hint={t.landingHintSpielEintragen} hasDivider
                onClick={() => navigate('/analog/new')} />
              <SubItem label={t.analogPlayersTitle} hint={t.landingHintSpieler}
                onClick={() => navigate('/players')} />
            </div>
          )}
          {displayKey === 'pik' && (
            <div className="landing-drawer-panel landing-drawer-panel--pik">
              <SubItem label={t.multiplayer} hint={t.landingHintMultiplayer} hasDivider
                onClick={() => navigate('/lobby')} />
              <SubItem label={t.testGame} hint={t.landingHintTestGame}
                onClick={() => navigate('/hot-seat')} />
            </div>
          )}
          {displayKey === 'herz' && (
            <div className="landing-drawer-panel landing-drawer-panel--herz">
              <SubItem label={t.landingRundenubersicht} hint={t.landingHintRundenubersicht} hasDivider
                onClick={() => navigate('/history')} />
              <SubItem label={t.landingStats} hint={t.landingHintStats} hasDivider
                onClick={() => navigate('/leaderboard')} />
              <SubItem label={t.landingSpielstatistiken} hint={t.landingHintSpielstatistiken}
                onClick={() => navigate('/stats')} />
            </div>
          )}
          {displayKey === 'karo' && (
            <div className="landing-drawer-panel landing-drawer-panel--karo">
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

      {showThemePicker && (
        <BottomSheet title="Theme" onClose={() => setShowThemePicker(false)}>
          <ThemePicker
            activePreset={activePreset}
            onSelect={(preset) => { setPreset(preset); setShowThemePicker(false); }}
          />
        </BottomSheet>
      )}
    </div>
  );
}
