export function SectionHeader({ label, count }: { label: string; count: number }) {
  return (
    <div className="ap-section-header">
      <span className="ap-section-label">{label}</span>
      <span className="ap-section-count">{count}</span>
    </div>
  );
}
