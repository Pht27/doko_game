import { t } from '../../translations';

interface LandingPageProps {
  onCreateLobby: () => void;
  onTestGame: () => void;
}

export function LandingPage({ onCreateLobby, onTestGame }: LandingPageProps) {
  return (
    <div className="w-full h-full flex flex-col items-center justify-center gap-10 px-6">
      <h1 className="text-5xl font-bold tracking-wide text-white">{t.landingTitle}</h1>

      <div className="flex flex-col gap-4 w-full max-w-xs">
        <button
          onClick={onCreateLobby}
          className="w-full py-4 text-xl font-semibold rounded-2xl bg-indigo-600 hover:bg-indigo-500 active:bg-indigo-700 text-white transition-colors"
        >
          {t.createLobby}
        </button>

        {import.meta.env.DEV && (
          <button
            onClick={onTestGame}
            className="w-full py-4 text-xl font-semibold rounded-2xl bg-white/10 hover:bg-white/20 active:bg-white/5 text-white/70 transition-colors"
          >
            {t.testGame}
          </button>
        )}
      </div>
    </div>
  );
}
