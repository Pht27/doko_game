import { t } from '../../translations';

interface GameLoaderProps {
  loading: boolean;
  error: string | null;
  onRetry: () => void;
}

export function GameLoader({ loading, error, onRetry }: GameLoaderProps) {
  return (
    <div className="w-full h-full flex items-center justify-center">
      {loading && <p className="text-white/60 text-lg">{t.startingGame}</p>}
      {error && (
        <div className="text-center">
          <p className="text-red-400 mb-4">{error}</p>
          <button onClick={onRetry} className="bg-indigo-500 text-white px-6 py-2 rounded-lg">{t.retry}</button>
        </div>
      )}
    </div>
  );
}
