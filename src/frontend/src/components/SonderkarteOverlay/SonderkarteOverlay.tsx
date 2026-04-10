import type { SonderkarteInfoDto } from '../../types/api';
import { t } from '../../translations';
import '../../styles/SonderkarteOverlay.css';

interface SonderkarteOverlayProps {
  sonderkarten: SonderkarteInfoDto[];
  onConfirm: (selected: string[], genscherPartnerId: number | null) => void;
  onCancel: () => void;
}

export function SonderkarteOverlay({ sonderkarten, onConfirm, onCancel }: SonderkarteOverlayProps) {
  const needsGenscher = sonderkarten.some(
    (s) => s.type === 'Genscherdamen' || s.type === 'Gegengenscherdamen',
  );

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const form = e.currentTarget;
    const data = new FormData(form);
    const selected = sonderkarten
      .filter((s) => data.get(`sk_${s.type}`) === 'on')
      .map((s) => s.type);
    const partnerRaw = data.get('genscherPartner');
    const genscherPartnerId =
      needsGenscher && partnerRaw ? parseInt(partnerRaw as string, 10) : null;
    onConfirm(selected, genscherPartnerId);
  }

  return (
    <div className="sonderkarte-overlay">
      <form
        onSubmit={handleSubmit}
        className="sonderkarte-form"
      >
        <h2 className="sonderkarte-title">{t.aktiviereSonderkarten}</h2>
        <p className="sonderkarte-description">{t.sonderkartenDescription}</p>

        {sonderkarten.map((sk) => (
          <label key={sk.type} className="sonderkarte-option">
            <input type="checkbox" name={`sk_${sk.type}`} className="sonderkarte-option-checkbox" />
            <div>
              <div className="sonderkarte-option-name">{sk.name}</div>
              <div className="sonderkarte-option-description">{sk.description}</div>
            </div>
          </label>
        ))}

        {needsGenscher && (
          <div>
            <label className="sonderkarte-partner-label">{t.genscherPartnerLabel}</label>
            <select name="genscherPartner" className="sonderkarte-partner-select">
              {[0, 1, 2, 3].map((p) => (
                <option key={p} value={p}>{t.playerLabel(p)}</option>
              ))}
            </select>
          </div>
        )}

        <div className="sonderkarte-actions">
          <button
            type="submit"
            className="sonderkarte-btn-confirm"
          >
            {t.karteAusspielen}
          </button>
          <button
            type="button"
            onClick={onCancel}
            className="sonderkarte-btn-cancel"
          >
            {t.abbrechen}
          </button>
        </div>
      </form>
    </div>
  );
}
