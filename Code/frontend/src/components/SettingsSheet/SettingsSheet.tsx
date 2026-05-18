import { useState } from 'react';
import { BottomSheet } from '@/components/BottomSheet/BottomSheet';
import { useTheme } from '@/hooks/useTheme';
import { ThemePicker } from '@/pages/LandingPage/ThemePicker/ThemePicker';
import { PlayerPreferencePicker } from './PlayerPreferencePicker/PlayerPreferencePicker';
import { usePlayerPreference } from '@/context/PlayerPreferenceContext';
import { t } from '@/utils/translations';
import './SettingsSheet.css';

type View = 'main' | 'theme' | 'player';

interface SettingsSheetProps {
  onClose: () => void;
}

export function SettingsSheet({ onClose }: SettingsSheetProps) {
  const { setPreset, activePreset } = useTheme();
  const { selectedPlayer } = usePlayerPreference();
  const [view, setView] = useState<View>('main');

  const title =
    view === 'theme' ? t.settingsThemeSection
    : view === 'player' ? t.settingsPlayerSection
    : t.settingsTitle;

  const handleClose = view === 'main' ? onClose : () => setView('main');

  return (
    <BottomSheet title={title} onClose={handleClose} maxHeight="92vh">
      {view === 'main' && (
        <div className="ss-rows">
          <button className="ss-row" onClick={() => setView('theme')}>
            <span className="ss-row-label">{t.settingsThemeSection}</span>
            <span className="ss-row-value">{activePreset.name} ›</span>
          </button>
          <button className="ss-row" onClick={() => setView('player')}>
            <span className="ss-row-label">{t.settingsPlayerSection}</span>
            <span className="ss-row-value">{selectedPlayer?.name ?? t.settingsNoPlayer} ›</span>
          </button>
        </div>
      )}

      {view === 'theme' && (
        <ThemePicker
          activePreset={activePreset}
          onSelect={(preset) => { setPreset(preset); setView('main'); }}
        />
      )}

      {view === 'player' && (
        <PlayerPreferencePicker onSelect={() => setView('main')} />
      )}
    </BottomSheet>
  );
}
