import { useState } from 'react';
import { BottomSheet } from '@/components/BottomSheet/BottomSheet';
import { t } from '@/utils/translations';
import './PointTypeToggle.css';

export type PointType = 'value' | 'wonlost' | 'earned';

function PointTypeInfoSheet({ onClose }: { onClose: () => void }) {
  return (
    <BottomSheet title={t.playerPointTypeSheetTitle} onClose={onClose}>
      <div className="ps-pt-info">
        <div className="ps-pt-info-item">
          <span className="ps-pt-info-name">{t.playerPointTypeValue}</span>
          <span className="ps-pt-info-desc">{t.playerPointTypeValueDesc}</span>
        </div>
        <div className="ps-pt-info-item">
          <span className="ps-pt-info-name">{t.playerPointTypeGross}</span>
          <span className="ps-pt-info-desc">{t.playerPointTypeGrossDesc}</span>
        </div>
        <div className="ps-pt-info-item">
          <span className="ps-pt-info-name">{t.playerPointTypeNet}</span>
          <span className="ps-pt-info-desc">{t.playerPointTypeNetDesc}</span>
        </div>
      </div>
    </BottomSheet>
  );
}

interface PointTypeToggleProps {
  value: PointType;
  onChange: (v: PointType) => void;
  disabledOptions?: PointType[];
}

export function PointTypeToggle({ value, onChange, disabledOptions = [] }: PointTypeToggleProps) {
  const [showInfo, setShowInfo] = useState(false);
  const opts: { key: PointType; label: string }[] = [
    { key: 'value', label: t.playerPointTypeValue },
    { key: 'wonlost', label: t.playerPointTypeGross },
    { key: 'earned', label: t.playerPointTypeNet },
  ];

  return (
    <>
      <div className="ps-pt-row">
        <button className="ps-pt-info-btn" onClick={() => setShowInfo(true)} aria-label={t.playerPointTypeSheetTitle}>?</button>
        <span className="ps-pt-label">{t.playerPointTypeLabel}</span>
        <div className="ps-pt-toggle">
          {opts.map((o) => {
            const isDisabled = disabledOptions.includes(o.key);
            return (
              <button
                key={o.key}
                className={`ps-pt-btn${value === o.key ? ' ps-pt-active' : ''}${isDisabled ? ' ps-pt-locked' : ''}`}
                disabled={isDisabled}
                onClick={() => onChange(o.key)}
              >
                {o.label}
              </button>
            );
          })}
        </div>
      </div>
      {showInfo && <PointTypeInfoSheet onClose={() => setShowInfo(false)} />}
    </>
  );
}
