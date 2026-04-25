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
    <div className="relative w-full h-full flex flex-col items-center justify-center gap-10 px-6">
      <h1 className="text-5xl font-bold tracking-wide text-white">{t.landingTitle}</h1>

      <div className="flex flex-col gap-4 w-full max-w-xs">
        <button
          onClick={onMultiplayer}
          className="w-full py-4 text-xl font-semibold rounded-2xl bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 text-white transition-colors"
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

        <button
          onClick={onRules}
          className="w-full py-3 text-base font-medium rounded-2xl bg-white/5 hover:bg-white/10 active:bg-white/5 text-white/50 hover:text-white/70 transition-colors"
        >
          {t.rulesTitle}
        </button>
      </div>

      <button
        onClick={() => setShowReleaseNotes(true)}
        className="absolute bottom-3 right-4 text-white/25 text-xs hover:text-white/45 transition-colors"
      >
        v{appVersion}
      </button>

      {showReleaseNotes && <ReleaseNotesModal onClose={() => setShowReleaseNotes(false)} />}
    </div>
  );
}
