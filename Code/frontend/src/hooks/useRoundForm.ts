import { useState, useCallback } from 'react';
import type { RoundDetail, RoundRequest, TeamRequest } from '@/types/analog';

export type Party = 'Re' | 'Kontra';

export interface ExtraPointEntry {
  extraPointId: number;
  count: number;
}

export interface TeamBlockState {
  party: Party;
  playerIds: number[];
  specialCardIds: number[];
  extraPoints: ExtraPointEntry[];
}

export interface RoundFormState {
  blocks: [TeamBlockState, TeamBlockState, TeamBlockState, TeamBlockState];
  points: number | '';
  gameModeId: number | null;
  winningParty: Party | null;
  comment: string;
  playedAt: string | null;
}

const emptyBlock = (party: Party): TeamBlockState => ({
  party,
  playerIds: [],
  specialCardIds: [],
  extraPoints: [],
});

const initialState = (): RoundFormState => ({
  blocks: [emptyBlock('Re'), emptyBlock('Re'), emptyBlock('Kontra'), emptyBlock('Kontra')],
  points: '',
  gameModeId: null,
  winningParty: null,
  comment: '',
  playedAt: null,
});

export function useRoundForm() {
  const [form, setForm] = useState<RoundFormState>(initialState);
  const [lastSwitchedBlock, setLastSwitchedBlock] = useState<number | null>(null);

  const updateBlock = useCallback(
    (index: number, updater: (b: TeamBlockState) => TeamBlockState) => {
      setForm((prev) => {
        const blocks = prev.blocks.map((b, i) =>
          i === index ? updater(b) : b,
        ) as RoundFormState['blocks'];
        return { ...prev, blocks };
      });
    },
    [],
  );

  const setGameMode = useCallback((id: number | null) => {
    setForm((prev) => ({ ...prev, gameModeId: id }));
  }, []);

  const setPoints = useCallback((v: number | '') => {
    setForm((prev) => ({ ...prev, points: v }));
  }, []);

  const setWinningParty = useCallback((party: Party | null) => {
    setForm((prev) => ({ ...prev, winningParty: party }));
  }, []);

  const setComment = useCallback((comment: string) => {
    setForm((prev) => ({ ...prev, comment }));
  }, []);

  const switchParty = useCallback((index: number) => {
    updateBlock(index, (b) => ({
      ...b,
      party: b.party === 'Re' ? 'Kontra' : 'Re',
    }));
    setLastSwitchedBlock(index);
  }, [updateBlock]);

  const setBlockPlayers = useCallback(
    (index: number, playerIds: number[]) => {
      updateBlock(index, (b) => ({ ...b, playerIds }));
    },
    [updateBlock],
  );

  const addSpecialCard = useCallback(
    (index: number, cardId: number) => {
      updateBlock(index, (b) =>
        b.specialCardIds.includes(cardId)
          ? b
          : { ...b, specialCardIds: [...b.specialCardIds, cardId] },
      );
    },
    [updateBlock],
  );

  const removeSpecialCard = useCallback(
    (index: number, cardId: number) => {
      updateBlock(index, (b) => ({
        ...b,
        specialCardIds: b.specialCardIds.filter((id) => id !== cardId),
      }));
    },
    [updateBlock],
  );

  const addExtraPoint = useCallback(
    (index: number, extraPointId: number) => {
      updateBlock(index, (b) =>
        b.extraPoints.some((ep) => ep.extraPointId === extraPointId)
          ? b
          : { ...b, extraPoints: [...b.extraPoints, { extraPointId, count: 1 }] },
      );
    },
    [updateBlock],
  );

  const updateExtraPointCount = useCallback(
    (index: number, extraPointId: number, delta: number) => {
      updateBlock(index, (b) => ({
        ...b,
        extraPoints: b.extraPoints.map((ep) =>
          ep.extraPointId === extraPointId
            ? { ...ep, count: Math.max(1, ep.count + delta) }
            : ep,
        ),
      }));
    },
    [updateBlock],
  );

  const removeExtraPoint = useCallback(
    (index: number, extraPointId: number) => {
      updateBlock(index, (b) => ({
        ...b,
        extraPoints: b.extraPoints.filter((ep) => ep.extraPointId !== extraPointId),
      }));
    },
    [updateBlock],
  );

  const importTeams = useCallback((detail: RoundDetail) => {
    const sorted = [
      ...detail.teams.filter((t) => t.party === 'Re'),
      ...detail.teams.filter((t) => t.party === 'Kontra'),
    ];
    const defaultParties: Party[] = ['Re', 'Re', 'Kontra', 'Kontra'];
    setForm((prev) => ({
      ...prev,
      blocks: [0, 1, 2, 3].map((i) => {
        const team = sorted[i];
        return {
          party: defaultParties[i],
          playerIds: team ? team.players.map((p) => p.id) : [],
          specialCardIds: [],
          extraPoints: [],
        };
      }) as unknown as RoundFormState['blocks'],
    }));
    setLastSwitchedBlock(null);
  }, []);

  const loadFromDetail = useCallback((detail: RoundDetail) => {
    // Sort teams: Re first, then Kontra, preserving order within each party
    const sorted = [
      ...detail.teams.filter((t) => t.party === 'Re'),
      ...detail.teams.filter((t) => t.party === 'Kontra'),
    ];
    // Fill exactly 4 blocks from sorted teams (pad with empty blocks if needed)
    const defaultParties: Party[] = ['Re', 'Re', 'Kontra', 'Kontra'];
    const blocks = [0, 1, 2, 3].map((i) => {
      const team = sorted[i];
      if (!team) return emptyBlock(defaultParties[i]);
      return {
        party: team.party,
        playerIds: team.players.map((p) => p.id),
        specialCardIds: team.specialCards.map((sc) => sc.id),
        extraPoints: team.extraPoints.map((ep) => ({
          extraPointId: ep.id,
          count: ep.count,
        })),
      };
    }) as RoundFormState['blocks'];

    setForm({
      blocks,
      points: detail.points,
      gameModeId: detail.gameMode.id,
      winningParty: detail.winningParty,
      comment: detail.comment ?? '',
      playedAt: detail.playedAt,
    });
    setLastSwitchedBlock(null);
  }, []);

  const resetForNew = useCallback(() => {
    setForm((prev) => ({
      blocks: prev.blocks.map((b) => ({
        ...b,
        specialCardIds: [] as number[],
        extraPoints: [] as ExtraPointEntry[],
      })) as RoundFormState['blocks'],
      points: '',
      gameModeId: null,
      winningParty: null,
      comment: '',
      playedAt: null,
    }));
    setLastSwitchedBlock(null);
  }, []);

  const toApiRequest = useCallback((): RoundRequest => {
    const { blocks, points, gameModeId, winningParty, comment, playedAt } = form;
    const teams: TeamRequest[] = blocks.map((b) => ({
      party: b.party,
      playerIds: b.playerIds,
      specialCardIds: b.specialCardIds,
      extraPoints: b.extraPoints,
    }));
    return {
      playedAt: playedAt ?? new Date().toISOString(),
      winningParty: winningParty!,
      points: points as number,
      gameModeId: gameModeId!,
      teams,
      comment: comment || null,
    };
  }, [form]);

  return {
    form,
    lastSwitchedBlock,
    setGameMode,
    setPoints,
    setWinningParty,
    setComment,
    switchParty,
    setBlockPlayers,
    addSpecialCard,
    removeSpecialCard,
    addExtraPoint,
    updateExtraPointCount,
    removeExtraPoint,
    importTeams,
    loadFromDetail,
    resetForNew,
    toApiRequest,
  };
}
