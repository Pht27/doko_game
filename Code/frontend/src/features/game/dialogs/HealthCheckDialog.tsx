import { t } from '@/utils/translations';
import { usePlayerName } from '@/context/PlayerNamesContext';
import './HealthCheckDialog.css';

interface HealthCheckDialogProps {
  playerId: number;
  onDeclare: (hasVorbehalt: boolean) => void;
}

export function HealthCheckDialog({ playerId, onDeclare }: HealthCheckDialogProps) {
  const playerName = usePlayerName(playerId);
  return (
    <div className="health-check-dialog">
      <div className="health-check-who">
        <span className="health-check-who-name">{playerName}</span>
        <span className="health-check-who-you">Du</span>
      </div>

      <div className="health-check-question">{t.healthCheckTitle}</div>

      <div className="health-check-divider" />

      <div className="health-check-actions">
        <button className="health-check-btn health-check-btn-gesund" onClick={() => onDeclare(false)}>
          {t.gesund}
        </button>
        <button className="health-check-btn health-check-btn-vorbehalt" onClick={() => onDeclare(true)}>
          {t.vorbehalt}
        </button>
      </div>
    </div>
  );
}
