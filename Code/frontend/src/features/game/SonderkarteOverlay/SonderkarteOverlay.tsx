import { useState } from 'react';
import type { SonderkarteInfoDto } from '@/types/api';
import { t } from '@/utils/translations';
import { usePlayerNameResolver } from '@/context/PlayerNamesContext';
import './SonderkarteOverlay.css';

const GENSCHER_TYPES = new Set(['Genscherdamen', 'Gegengenscherdamen']);

interface SonderkarteOverlayProps {
  sonderkarten: SonderkarteInfoDto[];
  activePlayer: number;
  onConfirm: (selected: string[], genscherPartnerId: number | null) => void;
  onCancel: () => void;
}

export function SonderkarteOverlay({
  sonderkarten,
  activePlayer,
  onConfirm,
  onCancel,
}: SonderkarteOverlayProps) {
  const getPlayerName = usePlayerNameResolver();
  const [skIdx, setSkIdx] = useState(0);
  const [selectedTypes, setSelectedTypes] = useState<string[]>([]);
  const [inPartnerSelect, setInPartnerSelect] = useState(false);
  const [partnerId, setPartnerId] = useState<number | null>(null);

  const sk = sonderkarten[skIdx];

  const others = [0, 1, 2, 3].filter((p) => p !== activePlayer);
  // position relative to activePlayer: 1=right, 2=top, 3=left
  const relPos = (p: number) => (p - activePlayer + 4) % 4;
  const currentPartner = partnerId ?? others[0] ?? null;

  function advanceOrFinish(types: string[], genscherPartnerId: number | null) {
    const nextIdx = skIdx + 1;
    if (nextIdx < sonderkarten.length) {
      setSkIdx(nextIdx);
      setSelectedTypes(types);
    } else {
      onConfirm(types, genscherPartnerId);
    }
  }

  function handleActivate() {
    const newTypes = [...selectedTypes, sk.type];
    if (GENSCHER_TYPES.has(sk.type)) {
      setSelectedTypes(newTypes);
      setInPartnerSelect(true);
    } else {
      advanceOrFinish(newTypes, null);
    }
  }

  function handleSkip() {
    advanceOrFinish(selectedTypes, null);
  }

  function handlePartnerConfirm() {
    setInPartnerSelect(false);
    advanceOrFinish(selectedTypes, currentPartner);
  }

  // ── Partner selection step ─────────────────────────────────────────────────
  if (inPartnerSelect) {
    const top = others.find((p) => relPos(p) === 2);
    const right = others.find((p) => relPos(p) === 1);
    const left = others.find((p) => relPos(p) === 3);

    return (
      <div className="sk-overlay">
        <div className="sk-card sk-card-wide">
          <div>
            <div className="sk-badge">{t.genscherBadge}</div>
            <h2 className="sk-title mt-1">{t.genscherPartnerWaehlen}</h2>
          </div>
          <div className="sk-divider" />
          <div className="sk-compass">
            <div className="sk-compass-top">
              {top !== undefined && (
                <button
                  className={`sk-partner-btn ${currentPartner === top ? 'sk-partner-btn-active' : ''}`}
                  onClick={() => setPartnerId(top)}
                >
                  {getPlayerName(top)}
                </button>
              )}
            </div>
            <div className="sk-compass-middle">
              <div className="sk-compass-left">
                {left !== undefined && (
                  <button
                    className={`sk-partner-btn ${currentPartner === left ? 'sk-partner-btn-active' : ''}`}
                    onClick={() => setPartnerId(left)}
                  >
                    {getPlayerName(left)}
                  </button>
                )}
              </div>
              <div className="sk-compass-center" />
              <div className="sk-compass-right">
                {right !== undefined && (
                  <button
                    className={`sk-partner-btn ${currentPartner === right ? 'sk-partner-btn-active' : ''}`}
                    onClick={() => setPartnerId(right)}
                  >
                    {getPlayerName(right)}
                  </button>
                )}
              </div>
            </div>
          </div>
          <button
            className="sk-btn-primary"
            onClick={handlePartnerConfirm}
            disabled={currentPartner === null}
          >
            {t.bestaetigen}
          </button>
        </div>
      </div>
    );
  }

  // ── Sonderkarte confirm step ───────────────────────────────────────────────
  return (
    <div className="sk-overlay">
      <div className="sk-card">
        <div>
          <div className="sk-badge">{t.sonderkarteBadge}</div>
          <h2 className="sk-title mt-1">{sk.name}</h2>
        </div>
        <div className="sk-divider" />
        <p className="sk-description">{sk.description}</p>
        <div className="sk-actions">
          <button className="sk-btn-primary" onClick={handleActivate}>
            {t.aktivieren}
          </button>
          <button className="sk-btn-secondary" onClick={handleSkip}>
            {t.nichtAktivieren}
          </button>
        </div>
        <button className="sk-btn-link" onClick={onCancel}>
          {t.abbrechen}
        </button>
      </div>
    </div>
  );
}
