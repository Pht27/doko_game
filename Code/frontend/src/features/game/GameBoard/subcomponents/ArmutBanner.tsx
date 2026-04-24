import { t } from '@/utils/translations';

interface ArmutBannerProps {
  exchangeCardCount: number;
  returnedTrump: boolean;
}

export function ArmutBanner({ exchangeCardCount, returnedTrump }: ArmutBannerProps) {
  return (
    <div className="bg-orange-900/40 text-orange-200 text-xs text-center py-1 px-4">
      {t.armutInfoBanner(exchangeCardCount, returnedTrump)}
    </div>
  );
}
