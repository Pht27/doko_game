import { t } from '../../translations';

export function GeschmissenDisplay() {
  return (
    <div className="rd-geschmissen">
      <h2 className="rd-geschmissen-title">{t.geschmissenTitle}</h2>
      <p className="rd-geschmissen-subtitle">{t.geschmissenSubtitle}</p>
    </div>
  );
}
