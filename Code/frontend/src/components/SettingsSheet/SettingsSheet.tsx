import { BottomSheet } from '@/components/BottomSheet/BottomSheet';
import { useTheme } from '@/hooks/useTheme';
import { ThemePicker } from '@/pages/LandingPage/ThemePicker/ThemePicker';
import { PlayerPreferencePicker } from './PlayerPreferencePicker/PlayerPreferencePicker';
import { usePlayerPreference } from '@/context/PlayerPreferenceContext';
import { t } from '@/utils/translations';
import './SettingsSheet.css';

interface SettingsSheetProps {
  onClose: () => void;
}

export function SettingsSheet({ onClose }: SettingsSheetProps) {
  const { setPreset, activePreset } = useTheme();
  const { selectedPlayer } = usePlayerPreference();

  return (
    <BottomSheet title={t.settingsTitle} onClose={onClose} maxHeight="92vh">
      <div className="ss-body">
        <div className="ss-section">
          <div className="ss-section-head">
            <span className="ss-section-title">{t.settingsPlayerSection}</span>
            <span className="ss-section-active">{selectedPlayer?.name ?? t.settingsNoPlayer}</span>
          </div>
          <PlayerPreferencePicker />
        </div>

        <div className="ss-divider" />

        <div className="ss-section">
          <div className="ss-section-head">
            <span className="ss-section-title">{t.settingsThemeSection}</span>
            <span className="ss-section-active">{activePreset.name}</span>
          </div>
          <ThemePicker activePreset={activePreset} onSelect={setPreset} />
        </div>
      </div>
    </BottomSheet>
  );
}
