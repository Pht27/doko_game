import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { t } from '@/utils/translations';
import { showTestFeatures } from '@/utils/env';
import { appVersion } from '@/utils/releaseNotes';
import { ReleaseNotesModal } from '@/components/ReleaseNotesModal/ReleaseNotesModal';

const divider = (
  <div style={{ height: 1, background: 'rgba(255,255,255,0.07)', margin: '2px 0' }} />
);

function PrimaryBtn({ onClick, label }: { onClick: () => void; label: string }) {
  return (
    <button
      onClick={onClick}
      className="w-full text-xl font-semibold text-white bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 transition-colors"
      style={{ padding: '17px 0', borderRadius: 18, border: 'none' }}
    >
      {label}
    </button>
  );
}

function SecondaryBtn({ onClick, label }: { onClick: () => void; label: string }) {
  return (
    <button
      onClick={onClick}
      className="w-full font-medium text-white/50 hover:text-white/70 bg-white/5 hover:bg-white/9 active:bg-white/4 transition-all"
      style={{ padding: '13px 0', borderRadius: 14, fontSize: 15, border: 'none' }}
    >
      {label}
    </button>
  );
}

function DisabledBtn({ label }: { label: string }) {
  return (
    <button
      disabled
      className="w-full font-medium text-white/20 bg-white/3 cursor-not-allowed"
      style={{ padding: '13px 0', borderRadius: 14, fontSize: 15, border: 'none' }}
    >
      {label}
    </button>
  );
}

export function LandingPage() {
  const navigate = useNavigate();
  const [showReleaseNotes, setShowReleaseNotes] = useState(false);

  return (
    <div className="relative w-full h-full flex flex-col items-center justify-center gap-8 px-6 overflow-hidden">

      {/* Decorative corner suit symbols */}
      <div className="absolute inset-0 pointer-events-none select-none" style={{ fontFamily: 'serif' }}>
        <span className="absolute text-white" style={{ top: '8%', left: '6%', fontSize: 120, opacity: 0.035, lineHeight: 1 }}>♠</span>
        <span className="absolute" style={{ top: '5%', right: '4%', fontSize: 120, opacity: 0.035, lineHeight: 1, color: '#e55' }}>♥</span>
        <span className="absolute" style={{ bottom: '10%', left: '4%', fontSize: 120, opacity: 0.035, lineHeight: 1, color: '#e55' }}>♦</span>
        <span className="absolute text-white" style={{ bottom: '8%', right: '5%', fontSize: 120, opacity: 0.035, lineHeight: 1 }}>♣</span>
      </div>

      {/* Title block */}
      <div className="text-center z-10">
        <h1 className="font-bold text-white" style={{ fontSize: 'clamp(38px, 12vw, 52px)', letterSpacing: '-0.01em', lineHeight: 1.05 }}>
          {t.landingTitle}
        </h1>
        <div className="flex justify-center gap-3 mt-2.5" style={{ fontFamily: 'serif', fontSize: 18, opacity: 0.22 }}>
          <span style={{ color: '#fff' }}>♣</span>
          <span style={{ color: '#e55' }}>♥</span>
          <span style={{ color: '#e55' }}>♦</span>
          <span style={{ color: '#fff' }}>♠</span>
        </div>
      </div>

      <div className="flex flex-col gap-3 w-full z-10" style={{ maxWidth: 280 }}>
        <DisabledBtn label={t.landingSpielEintragen} />
        <PrimaryBtn onClick={() => navigate('/lobby')} label={t.multiplayer} />
        {showTestFeatures && (
          <SecondaryBtn onClick={() => navigate('/hot-seat')} label={t.testGame} />
        )}

        {divider}

        <SecondaryBtn onClick={() => navigate('/analog/players')} label={t.analogPlayersTitle} />
        <DisabledBtn label={t.landingRundenubersicht} />

        {divider}

        <DisabledBtn label={t.landingStats} />
        <SecondaryBtn onClick={() => navigate('/rules')} label={t.rulesTitle} />
      </div>

      <button
        onClick={() => setShowReleaseNotes(true)}
        className="absolute bottom-3 right-4 text-white/20 text-xs hover:text-white/40 transition-colors z-10"
      >
        v{appVersion}
      </button>

      {showReleaseNotes && <ReleaseNotesModal onClose={() => setShowReleaseNotes(false)} />}
    </div>
  );
}
