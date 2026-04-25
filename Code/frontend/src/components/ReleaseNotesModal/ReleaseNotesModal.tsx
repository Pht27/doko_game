import { t } from '@/utils/translations';
import { releaseNotesContent } from '@/utils/releaseNotes';

interface ReleaseNotesModalProps {
  onClose: () => void;
}

export function ReleaseNotesModal({ onClose }: ReleaseNotesModalProps) {
  return (
    <div
      className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div
        className="bg-gray-800/95 border border-white/10 rounded-2xl shadow-2xl flex flex-col w-full max-w-lg max-h-[80vh]"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between px-5 py-4 border-b border-white/10 shrink-0">
          <h2 className="text-white font-bold text-base">{t.releaseNotesTitle}</h2>
          <button
            onClick={onClose}
            className="text-white/40 hover:text-white/70 text-xl leading-none transition-colors px-1"
          >
            ×
          </button>
        </div>

        <div className="overflow-y-auto px-5 py-4 flex flex-col gap-1">
          {releaseNotesContent.split('\n').map((line, i) => {
            if (line.startsWith('## ')) {
              return (
                <p key={i} className="text-indigo-300 font-semibold text-sm mt-4 first:mt-0">
                  {line.slice(3)}
                </p>
              );
            }
            if (line.startsWith('### ')) {
              return (
                <p key={i} className="text-white/60 text-xs font-semibold uppercase tracking-wider mt-2">
                  {line.slice(4)}
                </p>
              );
            }
            if (line.startsWith('- ')) {
              return (
                <p key={i} className="text-white/75 text-sm pl-3">
                  · {line.slice(2)}
                </p>
              );
            }
            if (line.startsWith('# ') || line.trim() === '') {
              return null;
            }
            return (
              <p key={i} className="text-white/50 text-sm">
                {line}
              </p>
            );
          })}
        </div>
      </div>
    </div>
  );
}
