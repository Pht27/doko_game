import type React from 'react';
import { PRESETS } from '@/hooks/useTheme';
import type { Preset } from '@/hooks/useTheme';
import './ThemePicker.css';

interface ThemePickerProps {
  activePreset: Preset;
  onSelect: (preset: Preset) => void;
}

export function ThemePicker({ activePreset, onSelect }: ThemePickerProps) {
  return (
    <div className="tp-strip">
      {PRESETS.map(preset => (
        <button
          key={preset.id}
          className={`tp-card${preset.id === activePreset.id ? ' tp-card--active' : ''}`}
          onClick={() => onSelect(preset)}
          style={{ '--tp-accent': preset.swatches[2] } as React.CSSProperties}
        >
          <div className="tp-preview" style={{ background: preset.swatches[0] }}>
            <div className="tp-preview-top" style={{ background: preset.swatches[2] }}>
              <div className="tp-preview-dot" />
            </div>
            <div className="tp-preview-body" style={{ background: preset.swatches[1] }}>
              <div className="tp-preview-row" />
              <div className="tp-preview-row" />
              <div className="tp-preview-row tp-preview-row--accent" />
            </div>
          </div>
          <div className="tp-name">
            <span>{preset.name}</span>
            {preset.id === activePreset.id && <div className="tp-check">✓</div>}
          </div>
        </button>
      ))}
    </div>
  );
}
