interface PlayerGridProps {
  top: React.ReactNode;
  left: React.ReactNode;
  center: React.ReactNode;
  right: React.ReactNode;
  bottom?: React.ReactNode;
}

/**
 * Pure layout wrapper for the board's compass positions.
 * Renders top/left/center/right player slots and an optional bottom slot (e.g. action errors).
 */
export function PlayerGrid({ top, left, center, right, bottom }: PlayerGridProps) {
  return (
    <div className="flex-1 relative flex flex-col items-center justify-between py-2">
      <div className="flex justify-center">{top}</div>
      <div className="grid grid-cols-[1fr_auto_1fr] items-center w-full px-2">
        <div className="flex justify-start">{left}</div>
        {center}
        <div className="flex justify-end">{right}</div>
      </div>
      {bottom}
    </div>
  );
}
