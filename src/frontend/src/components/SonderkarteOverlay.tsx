import type { SonderkarteInfoDto } from '../types/api';
import { t } from '../translations';
import '../styles/SonderkarteOverlay.css';

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
    <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50">
      <form
        onSubmit={handleSubmit}
        className="bg-gray-800 rounded-2xl p-6 w-80 shadow-2xl flex flex-col gap-4"
      >
        <h2 className="text-white font-bold text-lg">{t.aktiviereSonderkarten}</h2>
        <p className="text-white/60 text-sm">{t.sonderkartenDescription}</p>

        {sonderkarten.map((sk) => (
          <label key={sk.type} className="flex items-start gap-3 cursor-pointer">
            <input type="checkbox" name={`sk_${sk.type}`} className="mt-1" />
            <div>
              <div className="text-white font-semibold text-sm">{sk.name}</div>
              <div className="text-white/50 text-xs">{sk.description}</div>
            </div>
          </label>
        ))}

        {needsGenscher && (
          <div>
            <label className="text-white/70 text-sm block mb-1">{t.genscherPartnerLabel}</label>
            <select name="genscherPartner" className="w-full rounded bg-gray-700 text-white p-2 text-sm">
              {[0, 1, 2, 3].map((p) => (
                <option key={p} value={p}>{t.playerLabel(p)}</option>
              ))}
            </select>
          </div>
        )}

        <div className="flex gap-3 mt-2">
          <button
            type="submit"
            className="flex-1 bg-indigo-500 hover:bg-indigo-600 text-white rounded-lg py-2 font-semibold transition-colors"
          >
            {t.karteAusspielen}
          </button>
          <button
            type="button"
            onClick={onCancel}
            className="flex-1 bg-white/10 hover:bg-white/20 text-white rounded-lg py-2 transition-colors"
          >
            {t.abbrechen}
          </button>
        </div>
      </form>
    </div>
  );
}
