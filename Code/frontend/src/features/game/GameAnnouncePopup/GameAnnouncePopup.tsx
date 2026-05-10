import { CloseButton } from '@/components/CloseButton/CloseButton';
import './GameAnnouncePopup.css';

interface GameAnnouncePopupProps {
  message: string;
  onDismiss: () => void;
}

export function GameAnnouncePopup({ message, onDismiss }: GameAnnouncePopupProps) {
  return (
    <div className="game-announce-popup">
      <span className="game-announce-message">{message}</span>
      <CloseButton onClick={onDismiss} className="absolute top-1.5 right-2 text-sm py-0" />
      <div className="game-announce-progress" />
    </div>
  );
}
