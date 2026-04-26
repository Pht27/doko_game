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

  function btnClass(type: string): string {
    const lbl = label(type);
    if (lbl === 'Re') return 'announcement-btn announcement-btn-re';
    if (lbl === 'Kontra') return 'announcement-btn announcement-btn-kontra';
    return 'announcement-btn announcement-btn-other';
  }

  return (
    <div className="announcement-buttons">
      {legalAnnouncements.map((type) => (
        <button key={type} onClick={() => onAnnounce(type)} className={btnClass(type)}>
          {label(type)}
        </button>
      ))}
    </div>
  );
}
