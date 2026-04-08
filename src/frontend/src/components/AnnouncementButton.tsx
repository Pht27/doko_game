import { t } from '../translations';
import './AnnouncementButton.css';

interface AnnouncementButtonProps {
  legalAnnouncements: string[];
  onAnnounce: (type: string) => void;
}

export function AnnouncementButton({ legalAnnouncements, onAnnounce }: AnnouncementButtonProps) {
  if (legalAnnouncements.length === 0) return null;

  return (
    <div className="flex gap-2 justify-center">
      {legalAnnouncements.map((type) => (
        <button
          key={type}
          onClick={() => onAnnounce(type)}
          className="bg-yellow-500 hover:bg-yellow-400 text-black font-bold rounded-lg px-4 py-2 text-sm shadow transition-colors"
        >
          {t.announcementLabel(type)}
        </button>
      ))}
    </div>
  );
}
