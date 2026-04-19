interface ReadyVoteButtonProps {
  hasVoted: boolean;
  voteCount: number;
  disabled?: boolean;
  onClick: () => void;
}

export function ReadyVoteButton({ hasVoted, voteCount, disabled, onClick }: ReadyVoteButtonProps) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`w-full py-2 text-base font-semibold rounded-lg transition-colors disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-between px-4 ${
        hasVoted
          ? 'bg-green-600 hover:bg-green-500 active:bg-green-600 text-white'
          : 'bg-green-800 hover:bg-green-700 active:bg-green-900 text-white'
      }`}
    >
      <span className="text-sm font-normal opacity-70 whitespace-nowrap w-12 text-left">
        {voteCount}/4 👤
      </span>
      <span className="flex-1 text-center">
        {hasVoted ? '✓' : 'Bereit'}
      </span>
      <span className="w-12" />
    </button>
  );
}
