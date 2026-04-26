import { t } from '@/utils/translations';
import './AnnouncementButton.css';

interface AnnouncementButtonProps {
  legalAnnouncements: string[];
  ownParty: string | null;
  onAnnounce: (type: string) => void;
}

export function AnnouncementButton({
  legalAnnouncements,
  ownParty,
  onAnnounce,
}: AnnouncementButtonProps) {
  if (legalAnnouncements.length === 0) return null;

  function label(type: string): string {
    if (type === 'Win') return ownParty === 'Re' ? 'Re' : 'Kontra';
    return t.announcementLabel(type);
  }

  // Use same logic as label('Win'): Re when known Re, Kontra otherwise (preserves
  // the illusion in Kontrasolo where ownParty may be null)
  const partyClass = ownParty === 'Re' ? 'announcement-btn-re' : 'announcement-btn-kontra';

  return (
    <div className="announcement-buttons">
      {legalAnnouncements.map((type) => (
        <button key={type} onClick={() => onAnnounce(type)} className={`announcement-btn ${partyClass}`}>
          {label(type)}
        </button>
      ))}
    </div>
  );
}
