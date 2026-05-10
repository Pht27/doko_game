import { t } from '@/utils/translations';

interface SeatCardProps {
  seatIndex: number;
  occupied: boolean;
  isOpa: boolean;
  isMe: boolean;
  isReady: boolean;
  isBusy: boolean;
  canInteract: boolean;
  canAddOpa: boolean;
  canRemoveOpa: boolean;
  playerName: string | null | undefined;
  isEditingName: boolean;
  nameInput: string;
  nameInputRef: React.RefObject<HTMLInputElement | null>;
  onNameChange: (value: string) => void;
  onNameBlur: () => void;
  onNameKeyDown: (e: React.KeyboardEvent) => void;
  onStartEditingName: () => void;
  onClick: () => void;
  onAddOpa: () => void;
  onRemoveOpa: () => void;
}

export function SeatCard({
  seatIndex,
  occupied,
  isOpa,
  isMe,
  isReady,
  isBusy,
  canInteract,
  canAddOpa,
  canRemoveOpa,
  playerName,
  isEditingName,
  nameInput,
  nameInputRef,
  onNameChange,
  onNameBlur,
  onNameKeyDown,
  onStartEditingName,
  onClick,
  onAddOpa,
  onRemoveOpa,
}: SeatCardProps) {
  return (
    <div
      className={`flex items-center gap-2 px-3 py-3 rounded-xl transition-colors ${
        isMe
          ? 'bg-indigo-600/50 text-white ring-1 ring-indigo-400'
          : isOpa
            ? 'bg-white/15 text-white'
            : occupied
              ? 'bg-white/15 text-white'
              : canInteract
                ? 'bg-white/5 text-white/50 hover:bg-white/10 hover:text-white/80 cursor-pointer'
                : 'bg-white/5 text-white/20'
      }`}
      onClick={!isOpa ? onClick : undefined}
      role={!isOpa && canInteract ? 'button' : undefined}
    >
      <div
        className={`w-2.5 h-2.5 rounded-full shrink-0 ${
          occupied ? 'bg-green-400' : 'bg-white/20'
        }`}
      />
      <span className="text-sm font-medium truncate flex-1 min-w-0">
        {isBusy ? (
          t.joiningLobby
        ) : isOpa ? (
          <>
            Opa
            <span className="text-white/40 text-xs ml-1">🤖</span>
          </>
        ) : occupied ? (
          isEditingName ? (
            <input
              ref={nameInputRef}
              value={nameInput}
              onChange={(e) => onNameChange(e.target.value)}
              onBlur={onNameBlur}
              onKeyDown={onNameKeyDown}
              maxLength={16}
              placeholder={t.playerSlot(seatIndex)}
              className="bg-transparent border-b border-white/40 outline-none text-white text-sm w-full"
              onClick={(e) => e.stopPropagation()}
            />
          ) : (
            <>
              <span className="truncate">{playerName ?? t.playerSlot(seatIndex)}</span>
              {isMe && <span className="text-white/50 text-xs shrink-0">{t.youSuffix}</span>}
            </>
          )
        ) : (
          t.seatLabel(seatIndex)
        )}
      </span>
      {isMe && !isEditingName && !canRemoveOpa && !canAddOpa && (
        <button
          onClick={(e) => { e.stopPropagation(); onStartEditingName(); }}
          className="ml-auto text-white/30 hover:text-white/70 text-xs px-1 shrink-0"
          title={t.nameChange}
        >
          ✏️
        </button>
      )}
      {isReady && !isMe && !canRemoveOpa && !canAddOpa && (
        <span className="ml-auto text-green-400 text-sm shrink-0" title={t.readyTooltip}>✓</span>
      )}
      {isReady && isMe && !isEditingName && !canRemoveOpa && !canAddOpa && (
        <span className="text-green-400 text-sm shrink-0" title={t.readyTooltip}>✓</span>
      )}
      {canRemoveOpa && (
        <button
          onClick={(e) => { e.stopPropagation(); onRemoveOpa(); }}
          className="ml-auto text-red-400 hover:text-red-300 text-xs px-1 shrink-0"
          title={t.opaRemove}
        >
          ✕
        </button>
      )}
      {canAddOpa && (
        <button
          onClick={(e) => { e.stopPropagation(); onAddOpa(); }}
          className="ml-auto text-white/30 hover:text-white/60 text-xs px-1 shrink-0"
          title={t.opaAdd}
        >
          🤖
        </button>
      )}
    </div>
  );
}
