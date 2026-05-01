import { createContext, useContext, useState, useCallback, type ReactNode } from 'react';

interface PlayerNamesContextValue {
  getPlayerName: (id: number) => string;
  setPlayerNames: (names: (string | null)[]) => void;
}

const PlayerNamesContext = createContext<PlayerNamesContextValue>({
  getPlayerName: (id) => `S${id + 1}`,
  setPlayerNames: () => {},
});

export function PlayerNamesProvider({ children }: { children: ReactNode }) {
  const [playerNames, setPlayerNamesState] = useState<(string | null)[]>(Array(4).fill(null));

  const setPlayerNames = useCallback((names: (string | null)[]) => {
    setPlayerNamesState(names);
  }, []);

  const getPlayerName = useCallback(
    (id: number) => {
      const custom = playerNames[id];
      return custom && custom.trim() ? custom.trim() : `S${id + 1}`;
    },
    [playerNames],
  );

  return (
    <PlayerNamesContext.Provider value={{ getPlayerName, setPlayerNames }}>
      {children}
    </PlayerNamesContext.Provider>
  );
}

export function usePlayerName(id: number): string {
  const { getPlayerName } = useContext(PlayerNamesContext);
  return getPlayerName(id);
}

export function usePlayerNameResolver(): (id: number) => string {
  const { getPlayerName } = useContext(PlayerNamesContext);
  return getPlayerName;
}

export function useSetPlayerNames(): (names: (string | null)[]) => void {
  const { setPlayerNames } = useContext(PlayerNamesContext);
  return setPlayerNames;
}
