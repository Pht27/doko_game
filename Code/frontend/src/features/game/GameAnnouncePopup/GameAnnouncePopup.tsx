import './GameAnnouncePopup.css';

interface GameAnnouncePopupProps {
  message: string;
  onDismiss: () => void;
}

export function GameAnnouncePopup({ message, onDismiss }: GameAnnouncePopupProps) {
  return (
    <div className="game-announce-popup">
      <span className="game-announce-message">{message}</span>
      <button className="game-announce-close" onClick={onDismiss} aria-label="Schließen">
        ✕
      </button>
      <div className="game-announce-progress" />
    </div>
  );
}
