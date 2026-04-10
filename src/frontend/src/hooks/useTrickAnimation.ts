import { useState, useEffect, useRef } from 'react';
import type { PlayerGameViewResponse, TrickSummaryDto } from '../types/api';
import type { AnimPhase } from '../components/TrickArea/TrickArea';

const NEXT: Record<string, AnimPhase> = {
  winner: 'flip',
  flip:   'stack',
  stack:  'fly',
  fly:    null,
};

const DURATION: Record<string, number> = {
  winner: 1000,
  flip:   50,
  stack:  600,
  fly:    900,
};

export interface TrickAnimationResult {
  animTrick: TrickSummaryDto | null;
  animPhase: AnimPhase;
}

/**
 * Watches completedTricks in the game view and drives a multi-phase trick animation:
 * winner (1s) → flip (50ms) → stack (600ms) → fly (900ms) → done
 */
export function useTrickAnimation(view: PlayerGameViewResponse | null): TrickAnimationResult {
  const [animTrick, setAnimTrick] = useState<TrickSummaryDto | null>(null);
  const [animPhase, setAnimPhase] = useState<AnimPhase>(null);
  const prevCompletedCountRef = useRef<number | null>(null);

  // Detect when a new trick is added to completedTricks and start the animation.
  useEffect(() => {
    if (!view) {
      prevCompletedCountRef.current = null;
      return;
    }
    const count = view.completedTricks.length;
    if (prevCompletedCountRef.current === null) {
      prevCompletedCountRef.current = count;
      return;
    }
    if (count > prevCompletedCountRef.current) {
      const justCompleted = view.completedTricks[count - 1];
      if (justCompleted?.winner != null) {
        setAnimTrick(justCompleted);
        setAnimPhase('winner');
      }
    }
    prevCompletedCountRef.current = count;
  }, [view]);

  // Drive the animation phase sequence.
  useEffect(() => {
    if (!animPhase) return;
    const tid = setTimeout(() => {
      const next = NEXT[animPhase];
      setAnimPhase(next);
      if (next === null) setAnimTrick(null);
    }, DURATION[animPhase]);
    return () => clearTimeout(tid);
  }, [animPhase]);

  return { animTrick, animPhase };
}
