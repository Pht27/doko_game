interface AugenRowProps {
  reAugen: number;
  reStiche?: number | null;
  kontraAugen: number;
  kontraStiche?: number | null;
  isReWinner: boolean;
}

export function AugenRow({ reAugen, reStiche, kontraAugen, kontraStiche, isReWinner }: AugenRowProps) {
  return (
    <div className="rd-augen-row">
      <span className={isReWinner ? 'rd-augen-party rd-augen-winner' : 'rd-augen-party'}>Re</span>
      <span className={isReWinner ? 'rd-augen-score rd-augen-winner' : 'rd-augen-score'}>
        {reAugen}
        {reStiche != null && <span className="rd-augen-stiche"> ({reStiche})</span>}
      </span>
      <span className="rd-augen-divider">|</span>
      <span className={!isReWinner ? 'rd-augen-score rd-augen-winner' : 'rd-augen-score'}>
        {kontraAugen}
        {kontraStiche != null && <span className="rd-augen-stiche"> ({kontraStiche})</span>}
      </span>
      <span className={!isReWinner ? 'rd-augen-party rd-augen-winner' : 'rd-augen-party'}>Kontra</span>
    </div>
  );
}
