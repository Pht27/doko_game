import { useState, useEffect } from 'react';
import { getStaticData } from '@/api/analog';
import type { StaticData } from '@/types/analog';

export function useStaticData() {
  const [data, setData] = useState<StaticData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getStaticData()
      .then((d) => {
        if (cancelled) return;
        setData(d);
        setLoading(false);
      })
      .catch((err: unknown) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Fehler');
        setLoading(false);
      });
    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}
