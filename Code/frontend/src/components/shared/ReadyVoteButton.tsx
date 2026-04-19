interface ReadyVoteButtonProps {
  hasVoted: boolean;
  voteCount: number;
  disabled?: boolean;
  onClick: () => void;
  className?: string;
}

export function ReadyVoteButton({ hasVoted, voteCount, disabled, onClick, className }: ReadyVoteButtonProps) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`py-2.5 text-xs font-bold uppercase tracking-wider rounded-lg transition-colors disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-between px-4 ${
        hasVoted
          ? 'bg-green-600 hover:bg-green-500 active:bg-green-600 text-white'
          : 'bg-green-800 hover:bg-green-700 active:bg-green-900 text-white'
      } ${className ?? 'w-full'}`}
    >
      <span className="font-normal opacity-70 whitespace-nowrap w-8 text-left">
        {voteCount}/4 👤
      </span>
      <span className="flex-1 text-center">
        {hasVoted ? '✓ Bereit' : 'Bereit'}
      </span>
      <span className="w-8" />
    </button>
  );
}
