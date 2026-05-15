/** Read a CSS custom property from the root element as a hex color and return its RGB components. */
export function cssColor(prop: string): { r: number; g: number; b: number } {
  const h = getComputedStyle(document.documentElement).getPropertyValue(prop).trim().replace('#', '');
  const full = h.length === 3 ? h.split('').map((c) => c + c).join('') : h;
  return { r: parseInt(full.slice(0, 2), 16), g: parseInt(full.slice(2, 4), 16), b: parseInt(full.slice(4, 6), 16) };
}
