import { t } from '@/utils/translations';
import './SeatCard.css';

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
  const cardClass = isMe
    ? 'seat-card--me'
    : (isOpa || occupied)
      ? 'seat-card--occupied'
      : canInteract
        ? 'seat-card--interactable'
        : 'seat-card--disabled';

  return (
    <div
      className={`seat-card flex items-center gap-2 px-3 py-3 rounded-xl ${cardClass}`}
      onClick={!isOpa ? onClick : undefined}
      role={!isOpa && canInteract ? 'button' : undefined}
    >
      <div
        className={`w-2.5 h-2.5 rounded-full shrink-0 ${
          occupied ? 'bg-(--app-win)' : 'bg-(--app-border-md)'
        }`}
      />
      <span className="text-sm font-medium truncate flex-1 min-w-0">
        {isBusy ? (
          t.joiningLobby
        ) : isOpa ? (
          <>
            Opa
            <span className="text-(--app-text-muted) text-xs ml-1">🤖</span>
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
              className="bg-transparent border-b border-(--app-border-md) outline-none text-(--app-text) text-sm w-full"
              onClick={(e) => e.stopPropagation()}
            />
          ) : (
            <>
              <span className="truncate">{playerName ?? t.playerSlot(seatIndex)}</span>
              {isMe && <span className="text-(--app-text-muted) text-xs shrink-0">{t.youSuffix}</span>}
            </>
          )
        ) : (
          t.seatLabel(seatIndex)
        )}
      </span>
      {isMe && !isEditingName && !canRemoveOpa && !canAddOpa && (
        <button
          onClick={(e) => { e.stopPropagation(); onStartEditingName(); }}
          className="ml-auto text-(--app-text-muted) hover:text-(--app-text) text-xs px-1 shrink-0"
          title={t.nameChange}
        >
          ✏️
        </button>
      )}
      {isReady && !isMe && !canRemoveOpa && !canAddOpa && (
        <span className="ml-auto text-(--app-win) text-sm shrink-0" title={t.readyTooltip}>✓</span>
      )}
      {isReady && isMe && !isEditingName && !canRemoveOpa && !canAddOpa && (
        <span className="text-(--app-win) text-sm shrink-0" title={t.readyTooltip}>✓</span>
      )}
      {canRemoveOpa && (
        <button
          onClick={(e) => { e.stopPropagation(); onRemoveOpa(); }}
          className="ml-auto text-(--app-loss) hover:text-(--app-loss) text-xs px-1 shrink-0"
          title={t.opaRemove}
        >
          ✕
        </button>
      )}
      {canAddOpa && (
        <button
          onClick={(e) => { e.stopPropagation(); onAddOpa(); }}
          className="ml-auto text-(--app-text-muted) hover:text-(--app-text-sub) text-xs px-1 shrink-0"
          title={t.opaAdd}
        >
          🤖
        </button>
      )}
    </div>
  );
}
