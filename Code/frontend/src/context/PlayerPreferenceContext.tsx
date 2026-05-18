import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';

interface PlayerPreference {
  id: number;
  name: string;
}

interface PlayerPreferenceContextValue {
  selectedPlayer: PlayerPreference | null;
  setSelectedPlayer: (id: number, name: string) => void;
  clearSelectedPlayer: () => void;
}

const STORAGE_KEY = 'doko-selected-player';

function readFromStorage(): PlayerPreference | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as unknown;
    if (
      parsed &&
      typeof parsed === 'object' &&
      'id' in parsed &&
      'name' in parsed &&
      typeof (parsed as { id: unknown }).id === 'number' &&
      typeof (parsed as { name: unknown }).name === 'string'
    ) {
      return parsed as PlayerPreference;
    }
    return null;
  } catch {
    return null;
  }
}

const PlayerPreferenceContext = createContext<PlayerPreferenceContextValue>({
  selectedPlayer: null,
  setSelectedPlayer: () => {},
  clearSelectedPlayer: () => {},
});

export function PlayerPreferenceProvider({ children }: { children: ReactNode }) {
  const [selectedPlayer, setSelectedPlayerState] = useState<PlayerPreference | null>(readFromStorage);

  const setSelectedPlayer = useCallback((id: number, name: string) => {
    const player = { id, name };
    setSelectedPlayerState(player);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(player));
  }, []);

  const clearSelectedPlayer = useCallback(() => {
    setSelectedPlayerState(null);
    localStorage.removeItem(STORAGE_KEY);
  }, []);

  return (
    <PlayerPreferenceContext.Provider value={{ selectedPlayer, setSelectedPlayer, clearSelectedPlayer }}>
      {children}
    </PlayerPreferenceContext.Provider>
  );
}

export function usePlayerPreference() {
  return useContext(PlayerPreferenceContext);
}
