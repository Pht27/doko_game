import { useState } from 'react';
import { t } from '@/utils/translations';
import { showTestFeatures } from '@/utils/env';
import { appVersion } from '@/utils/releaseNotes';
import { ReleaseNotesModal } from '@/components/ReleaseNotesModal/ReleaseNotesModal';

interface LandingPageProps {
  onMultiplayer: () => void;
  onTestGame: () => void;
  onRules: () => void;
}

export function LandingPage({ onMultiplayer, onTestGame, onRules }: LandingPageProps) {
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
        <button
          onClick={onMultiplayer}
          className="w-full text-xl font-semibold text-white bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 transition-colors"
          style={{ padding: '17px 0', borderRadius: 18, border: 'none' }}
        >
          {t.multiplayer}
        </button>

        {showTestFeatures && (
          <button
            onClick={onTestGame}
            className="w-full py-4 text-xl font-semibold rounded-2xl bg-white/10 hover:bg-white/20 active:bg-white/5 text-white/70 transition-colors"
          >
            {t.testGame}
          </button>
        )}

        <div style={{ height: 1, background: 'rgba(255,255,255,0.07)', margin: '2px 0' }} />

        <button
          onClick={onRules}
          className="w-full font-medium text-white/40 hover:text-white/65 bg-white/5 hover:bg-white/9 active:bg-white/4 transition-all"
          style={{ padding: '12px 0', borderRadius: 14, fontSize: 15, border: 'none' }}
        >
          {t.rulesTitle}
        </button>
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
