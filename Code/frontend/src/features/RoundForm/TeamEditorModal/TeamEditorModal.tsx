import { t } from '@/utils/translations';
import type { TeamBlockState } from '@/hooks/useRoundForm';
import type { PlayerListItem, SpecialCard, ExtraPoint } from '@/types/analog';
import { BottomSheet } from '@/components/BottomSheet/BottomSheet';
import { PlayerPicker } from './PlayerPicker/PlayerPicker';
import { SpecialCardPicker } from './SpecialCardPicker/SpecialCardPicker';
import { ExtraPointsPicker } from './ExtraPointsPicker/ExtraPointsPicker';
import './TeamEditorModal.css';

interface Props {
  block: TeamBlockState;
  blockIndex: number;
  allPlayers: PlayerListItem[];
  specialCards: SpecialCard[];
  extraPoints: ExtraPoint[];
  assignedPlayerIds: number[];
  assignedSpecialCardIds: number[];
  onClose: () => void;
  onSetPlayers: (ids: number[]) => void;
  onAddSpecialCard: (id: number) => void;
  onRemoveSpecialCard: (id: number) => void;
  onAddExtraPoint: (id: number) => void;
  onUpdateExtraPointCount: (id: number, delta: number) => void;
  onRemoveExtraPoint: (id: number) => void;
}

export function TeamEditorModal({
  block,
  allPlayers,
  specialCards,
  extraPoints,
  assignedPlayerIds,
  assignedSpecialCardIds,
  onClose,
  onSetPlayers,
  onAddSpecialCard,
  onRemoveSpecialCard,
  onAddExtraPoint,
  onUpdateExtraPointCount,
  onRemoveExtraPoint,
}: Props) {
  return (
    <BottomSheet title={t.analogTeamEditTitle} onClose={onClose} maxHeight="92vh">
      <div className="tem-body">
        <PlayerPicker
          selectedIds={block.playerIds}
          allPlayers={allPlayers}
          assignedPlayerIds={assignedPlayerIds}
          onSetPlayers={onSetPlayers}
        />
        <SpecialCardPicker
          selectedIds={block.specialCardIds}
          specialCards={specialCards}
          assignedSpecialCardIds={assignedSpecialCardIds}
          onAdd={onAddSpecialCard}
          onRemove={onRemoveSpecialCard}
        />
        <ExtraPointsPicker
          activeExtraPoints={block.extraPoints}
          extraPoints={extraPoints}
          onAdd={onAddExtraPoint}
          onUpdateCount={onUpdateExtraPointCount}
          onRemove={onRemoveExtraPoint}
        />
      </div>
    </BottomSheet>
  );
}
