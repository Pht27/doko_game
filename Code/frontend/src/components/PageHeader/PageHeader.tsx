import { BackButton } from '@/components/BackButton/BackButton';

interface PageHeaderProps {
  title: string;
  backTo?: string | number;
  onBack?: () => void;
  right?: React.ReactNode;
  className?: string;
}

export function PageHeader({ title, backTo, onBack, right, className = '' }: PageHeaderProps) {
  const hasBack = backTo !== undefined || onBack !== undefined;

  return (
    <div
      className={`flex items-center gap-2 px-4 py-[14px] bg-(--app-header-bg) border-b border-(--app-border) shrink-0 ${className}`}
    >
      {hasBack && <BackButton to={backTo} onClick={onBack} />}
      <h1 className="flex-1 min-w-0 text-2xl font-bold tracking-tight text-(--app-header-text) m-0 truncate">
        {title}
      </h1>
      {right}
    </div>
  );
}
