import type { Party } from './resultDisplay.utils';

interface WinnerBannerProps {
  winner: Party;
  headerLabel: string;
}

export function WinnerBanner({ winner, headerLabel }: WinnerBannerProps) {
  return (
    <div className={winner === 'Re' ? 'rd-winner-banner rd-winner-re' : 'rd-winner-banner rd-winner-kontra'}>
      {headerLabel}
    </div>
  );
}
