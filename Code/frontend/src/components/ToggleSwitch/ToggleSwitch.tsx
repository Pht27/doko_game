import { useState } from 'react';
import './ToggleSwitch.css';

interface ToggleSwitchProps {
  on: boolean;
  onChange: () => void;
  size?: 'md' | 'sm';
  stopPropagation?: boolean;
  ariaLabel?: string;
}

export function ToggleSwitch({ on, onChange, size = 'md', stopPropagation = false, ariaLabel }: ToggleSwitchProps) {
  const [pressing, setPressing] = useState(false);

  const maybeStop = (e: React.PointerEvent | React.MouseEvent) => {
    if (stopPropagation) e.stopPropagation();
  };

  return (
    <button
      className={`ts-switch ts-switch--${size}${on ? ' ts-switch--on' : ''}${pressing ? ' ts-switch--pressing' : ''}`}
      onPointerDown={(e) => { maybeStop(e); setPressing(true); }}
      onPointerUp={(e) => { maybeStop(e); setPressing(false); onChange(); }}
      onPointerLeave={() => setPressing(false)}
      onPointerCancel={() => setPressing(false)}
      onClick={(e) => maybeStop(e)}
      aria-pressed={on}
      aria-label={ariaLabel}
      type="button"
    >
      <span className="ts-thumb" />
    </button>
  );
}
