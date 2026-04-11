import { useState } from 'react';
import { playCard, declareHealth, makeReservation, respondToArmut, exchangeArmutCards, makeAnnouncement } from '../api/game';
import type { CardDto, PlayerGameViewResponse, SonderkarteInfoDto } from '../types/api';
import type { HotSeatSession } from './useHotSeat';

export interface GameActions {
  pendingCard: { card: CardDto; sonderkarten: SonderkarteInfoDto[] } | null;
  setPendingCard: (val: { card: CardDto; sonderkarten: SonderkarteInfoDto[] } | null) => void;
  actionError: string | null;
  armutReturnSelected: Set<number>;
  playingCardId: number | null;
  handleCardClick: (card: CardDto) => void;
  submitPlayCard: (cardId: number, activateSonderkarten: string[], genscherPartnerId: number | null) => Promise<void>;
  handleHealthCheck: (hasVorbehalt: boolean) => Promise<void>;
  handleReservation: (reservation: string | null, hochzeitCondition: string | null, armutPartner: number | null) => Promise<void>;
  handleArmutResponse: (accepts: boolean) => Promise<void>;
  handleArmutExchange: (cardIds: number[]) => Promise<void>;
  handleAnnouncement: (type: string) => Promise<void>;
}

/**
 * Manages all game action handlers, pending card state, and armut card selection.
 */
export function useGameActions(
  session: HotSeatSession | null,
  activePlayer: number,
  setActivePlayer: (player: number) => void,
  view: PlayerGameViewResponse | null,
  refetch: () => void,
): GameActions {
  const [pendingCard, setPendingCard] = useState<{ card: CardDto; sonderkarten: SonderkarteInfoDto[] } | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [armutReturnSelected, setArmutReturnSelected] = useState<Set<number>>(new Set());
  const [playingCardId, setPlayingCardId] = useState<number | null>(null);

  const token = session?.tokens[activePlayer] ?? '';
  const gameId = session?.gameId ?? '';

  function handleCardClick(card: CardDto) {
    if (!view) return;

    if (view.shouldReturnArmutCards) {
      setArmutReturnSelected((prev) => {
        const next = new Set(prev);
        if (next.has(card.id)) {
          next.delete(card.id);
        } else if (next.size < (view.armutCardReturnCount ?? 0)) {
          next.add(card.id);
        }
        return next;
      });
      return;
    }

    const eligibleSk = view.eligibleSonderkartenPerCard[card.id] ?? [];
    if (eligibleSk.length > 0) {
      setPendingCard({ card, sonderkarten: eligibleSk });
      return;
    }
    void submitPlayCard(card.id, [], null);
  }

  async function submitPlayCard(cardId: number, activateSonderkarten: string[], genscherPartnerId: number | null) {
    setPendingCard(null);
    setActionError(null);
    setPlayingCardId(cardId);
    try {
      await playCard(token, gameId, { cardId, activateSonderkarten, genscherPartnerId });
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    } finally {
      setPlayingCardId(null);
    }
  }

  async function handleHealthCheck(hasVorbehalt: boolean) {
    setActionError(null);
    try {
      await declareHealth(token, gameId, { hasVorbehalt });
      setActivePlayer((activePlayer + 1) % 4);
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function handleReservation(reservation: string | null, hochzeitCondition: string | null, armutPartner: number | null) {
    setActionError(null);
    try {
      await makeReservation(token, gameId, { reservation, hochzeitCondition, armutPartner });
      setActivePlayer((activePlayer + 1) % 4);
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function handleArmutResponse(accepts: boolean) {
    setActionError(null);
    try {
      await respondToArmut(token, gameId, { accepts });
      setActivePlayer((activePlayer + 1) % 4);
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function handleArmutExchange(cardIds: number[]) {
    setActionError(null);
    setArmutReturnSelected(new Set());
    try {
      await exchangeArmutCards(token, gameId, { cardIds });
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  async function handleAnnouncement(type: string) {
    setActionError(null);
    try {
      await makeAnnouncement(token, gameId, { type });
      refetch();
    } catch (e) {
      setActionError(e instanceof Error ? e.message : String(e));
    }
  }

  return {
    pendingCard,
    setPendingCard,
    actionError,
    armutReturnSelected,
    playingCardId,
    handleCardClick,
    submitPlayCard,
    handleHealthCheck,
    handleReservation,
    handleArmutResponse,
    handleArmutExchange,
    handleAnnouncement,
  };
}
