import { PRESETS } from '@/hooks/useTheme';
import type { Preset } from '@/hooks/useTheme';
import './ThemePicker.css';

const PRESET_DESCS: Record<string, string> = {
  default:  'Dunkles Navy-Blau',
  rose:     'Helles Rosa',
  aqua:     'Helles Türkis',
  classic:  'Klassisches Weiß',
  cherry:   'Helles Kirschrot',
  nautical: 'Helles Marine-Rot',
};

interface ThemePickerProps {
  activePreset: Preset;
  onSelect: (preset: Preset) => void;
}

export function ThemePicker({ activePreset, onSelect }: ThemePickerProps) {
  return (
    <div className="tp-body">
      {PRESETS.map(preset => (
        <button
          key={preset.id}
          className={`tp-card${preset.id === activePreset.id ? ' tp-card--active' : ''}`}
          onClick={() => onSelect(preset)}
        >
          <div className="tp-swatches">
            {preset.swatches.map((color, i) => (
              <div key={i} className="tp-swatch" style={{ background: color }} />
            ))}
          </div>
          <div className="tp-card-info">
            <span className="tp-card-name">{preset.name}</span>
            <span className="tp-card-desc">{PRESET_DESCS[preset.id]}</span>
          </div>
          <div className="tp-check">{preset.id === activePreset.id ? '✓' : ''}</div>
        </button>
      ))}
    </div>
  );
}
