import { t } from '@/utils/translations';
import './HealthCheckDialog.css';

interface HealthCheckDialogProps {
  playerId: number;
  onDeclare: (hasVorbehalt: boolean) => void;
}

export function HealthCheckDialog({ playerId, onDeclare }: HealthCheckDialogProps) {
  return (
    <div className="health-check-dialog">
      <h2 className="health-check-title">{t.healthCheckTitle(playerId)}</h2>

      <button
        onClick={() => onDeclare(false)}
        className="health-check-btn-gesund"
      >
        {t.gesund}
      </button>

      <button
        onClick={() => onDeclare(true)}
        className="health-check-btn-vorbehalt"
      >
        {t.vorbehalt}
      </button>
    </div>
  );
}
