/** Maximum total spread of the fan in degrees. */
export const FAN_SPREAD_DEG = 30;

/** Maximum rotation step between adjacent cards — limits spread when few cards remain. */
export const MAX_CARD_ANGLE_DEG = 4;

/** Vertical drop (rem) at the outermost card. Controls arc curvature. */
export const ARC_DEPTH_REM = 1;

/** How much (rem) a selected card lifts above the arc. */
export const SELECTED_LIFT_REM = 1.25;

// ─── Card dimensions (must stay in sync with HandDisplay.css) ────────────────

/** SVG intrinsic aspect ratio: viewBox 110 × 170. */
const CARD_ASPECT_RATIO = 110 / 170;

/** Card heights matching .card CSS rules (mobile h-32, tablet 12rem). */
const MOBILE_CARD_HEIGHT_REM = 8;
const TABLET_CARD_HEIGHT_REM = 12;

/** Card overlaps matching CSS --card-overlap variable. */
const MOBILE_CARD_OVERLAP_REM = 3.5;
const TABLET_CARD_OVERLAP_REM = 5.3;

/** Horizontal step between adjacent card centers (card width − overlap). */
export const MOBILE_CARD_STEP_REM =
  MOBILE_CARD_HEIGHT_REM * CARD_ASPECT_RATIO - MOBILE_CARD_OVERLAP_REM;
export const TABLET_CARD_STEP_REM =
  TABLET_CARD_HEIGHT_REM * CARD_ASPECT_RATIO - TABLET_CARD_OVERLAP_REM;

/** CSS breakpoint matching the tablet media query in HandDisplay.css. */
export const TABLET_BREAKPOINT_PX = 640;
