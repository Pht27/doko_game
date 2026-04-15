import { t } from '../../translations';
import '../../styles/AnnouncementButton.css';

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

  return (
    <div className="announcement-buttons">
      {legalAnnouncements.map((type) => (
        <button key={type} onClick={() => onAnnounce(type)} className="announcement-btn">
          {label(type)}
        </button>
      ))}
    </div>
  );
}
