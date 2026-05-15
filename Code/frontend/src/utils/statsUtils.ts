function lerp(a: number, b: number, t: number) {
  return Math.round(a + t * (b - a));
}

import { cssColor } from './themeUtils';

function statsGradient(pct: number): string {
  const p = Math.max(0, Math.min(1, pct));
  const lo = cssColor('--app-loss');
  const mid = cssColor('--app-warning');
  const hi = cssColor('--app-win');
  let c;
  if (p < 0.5) {
    const t = p * 2;
    c = { r: lerp(lo.r, mid.r, t), g: lerp(lo.g, mid.g, t), b: lerp(lo.b, mid.b, t) };
  } else {
    const t = (p - 0.5) * 2;
    c = { r: lerp(mid.r, hi.r, t), g: lerp(mid.g, hi.g, t), b: lerp(mid.b, hi.b, t) };
  }
  return `rgb(${c.r}, ${c.g}, ${c.b})`;
}

export function colorForRate(rate: number | null | undefined): string {
  if (rate == null) return 'var(--app-text-muted)';
  // 40 % → red, 50 % → yellow, 60 % → green (±10 % around the expected 50 %)
  const pct = Math.max(0, Math.min(1, (rate - 0.40) / 0.20));
  return statsGradient(pct);
}

export function colorForMean(m: number | null | undefined, clamp = 0.5): string {
  if (m == null) return 'var(--app-text-muted)';
  return statsGradient((Math.max(-clamp, Math.min(clamp, m)) + clamp) / (clamp * 2));
}

export function colorForTotal(total: number | null | undefined, clamp = 150): string {
  if (total == null) return 'var(--app-text-muted)';
  return statsGradient((Math.max(-clamp, Math.min(clamp, total)) + clamp) / (clamp * 2));
}

export function fmtRate(v: number | null | undefined): string {
  if (v == null) return '—';
  return `${(v * 100).toFixed(1)}%`;
}

export function fmtMean(v: number | null | undefined): string {
  if (v == null) return '—';
  return (v >= 0 ? '+' : '') + v.toFixed(2);
}

export function fmtInt(v: number | null | undefined): string {
  if (v == null) return '—';
  return v.toLocaleString('de-DE');
}
