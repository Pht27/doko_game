import { t } from '../../translations';
import '../../styles/AnnouncementButton.css';

interface AnnouncementButtonProps {
  legalAnnouncements: string[];
  onAnnounce: (type: string) => void;
}

export function AnnouncementButton({ legalAnnouncements, onAnnounce }: AnnouncementButtonProps) {
  if (legalAnnouncements.length === 0) return null;

  return (
    <div className="announcement-buttons">
      {legalAnnouncements.map((type) => (
        <button
          key={type}
          onClick={() => onAnnounce(type)}
          className="announcement-btn"
        >
          {t.announcementLabel(type)}
        </button>
      ))}
    </div>
  );
}
