import { t } from '@/utils/translations';
import { releaseNotesContent } from '@/utils/releaseNotes';
import { Button } from '@/components/Button/Button';
import { CloseButton } from '@/components/CloseButton/CloseButton';

interface ReleaseNotesModalProps {
  onClose: () => void;
  needRefresh?: boolean;
  updateSW?: (reloadPage?: boolean) => void;
}

export function ReleaseNotesModal({ onClose, needRefresh, updateSW }: ReleaseNotesModalProps) {
  let firstVersionSeen = false;
  let inFirstVersion = false;

  return (
    <div
      className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div
        className="bg-(--app-surface) border border-(--app-border) rounded-2xl shadow-2xl flex flex-col w-full max-w-lg max-h-[80vh]"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between px-5 py-4 border-b border-(--app-border) shrink-0">
          <h2 className="text-(--app-text) font-bold text-base">{t.releaseNotesTitle}</h2>
          <CloseButton onClick={onClose} />
        </div>

        {needRefresh && updateSW && (
          <div className="mx-4 mt-4 shrink-0 rounded-xl bg-(--app-re-soft) border border-(--app-border-md) px-4 py-3 flex items-center justify-between gap-3">
            <div className="flex flex-col gap-0.5">
              <span className="text-(--app-re) font-semibold text-sm">Neue Version verfügbar</span>
              <span className="text-(--app-text-muted) text-xs">App neu laden, um das Update zu installieren</span>
            </div>
            <Button
              onClick={() => updateSW(true)}
              className="shrink-0 text-sm rounded-lg px-3 py-1.5"
            >
              Aktualisieren
            </Button>
          </div>
        )}

        <div className="overflow-y-auto px-5 py-4 flex flex-col gap-1">
          {releaseNotesContent.split('\n').map((line, i) => {
            if (line.startsWith('## ')) {
              if (!firstVersionSeen) {
                firstVersionSeen = true;
                inFirstVersion = true;
              } else {
                inFirstVersion = false;
              }
              return inFirstVersion ? (
                <div key={i} className="flex items-center gap-2 mt-4 first:mt-0">
                  <p className="text-(--app-text) font-bold text-sm">{line.slice(3)}</p>
                  <span className="text-[10px] font-bold uppercase tracking-wider bg-(--app-re-soft) text-(--app-re-label) rounded-full px-2 py-0.5">Neu</span>
                </div>
              ) : (
                <p key={i} className="text-(--app-text-muted) font-semibold text-sm mt-6">
                  {line.slice(3)}
                </p>
              );
            }
            if (line.startsWith('### ')) {
              return (
                <p key={i} className={`text-xs font-semibold uppercase tracking-wider mt-2 ${inFirstVersion ? 'text-(--app-text-sub)' : 'text-(--app-text-muted)'}`}>
                  {line.slice(4)}
                </p>
              );
            }
            if (line.startsWith('- ')) {
              return (
                <p key={i} className={`text-sm pl-3 ${inFirstVersion ? 'text-(--app-text)' : 'text-(--app-text-muted)'}`}>
                  · {line.slice(2)}
                </p>
              );
            }
            if (line.startsWith('# ') || line.trim() === '') {
              return null;
            }
            return (
              <p key={i} className="text-(--app-text-muted) text-sm">
                {line}
              </p>
            );
          })}
        </div>
      </div>
    </div>
  );
}
