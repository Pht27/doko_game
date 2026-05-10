import { t } from '@/utils/translations';
import { StatusState } from '@/components/StatusState/StatusState';

interface GameLoaderProps {
  loading: boolean;
  error: string | null;
  onRetry: () => void;
}

export function GameLoader({ loading, error, onRetry }: GameLoaderProps) {
  return (
    <div className="w-full h-full flex flex-col items-center justify-center gap-4">
      {loading && <StatusState type="loading" message={t.startingGame} />}
      {error && (
        <>
          <StatusState type="error" message={error} />
          <button onClick={onRetry} className="bg-indigo-500 text-white px-6 py-2 rounded-lg">{t.retry}</button>
        </>
      )}
    </div>
  );
}
