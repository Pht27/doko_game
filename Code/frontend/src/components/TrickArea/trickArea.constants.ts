/** Max random tilt angle in degrees (±). Must match --trick-max-tilt-deg in CSS. */
export const MAX_TILT_DEG = 15;

/** Card offset from pile centre as a fraction of card width (horizontal) or height (vertical). */
export const OFFSET_FACTOR = 0.33;

/** Per-seat offsets as dimensionless factors (multiplied by card-w / card-h in CSS calc). */
export const SEAT_OFFSET: Record<string, { ox: number; oy: number }> = {
  top:    { ox: 0,              oy: -OFFSET_FACTOR },
  bottom: { ox: 0,              oy:  OFFSET_FACTOR },
  left:   { ox: -OFFSET_FACTOR, oy: 0 },
  right:  { ox:  OFFSET_FACTOR, oy: 0 },
};

/** Base rotation per seat so cards look like they came from that direction. */
export const SEAT_BASE_ROT: Record<string, number> = {
  bottom:  0,
  top:   180,
  left:   90,
  right: -90,
};

/** Direction the pile flies per winner's seat (in px, large enough to leave the viewport). */
export const FLY_TRANSLATE: Record<string, { x: string; y: string }> = {
  bottom: { x: '0px',    y: '280px'  },
  top:    { x: '0px',    y: '-280px' },
  left:   { x: '-280px', y: '0px'    },
  right:  { x: '280px',  y: '0px'    },
};

/** Rotation of the stacked pile so it faces the winner's seat. */
export const PILE_ROT: Record<string, number> = {
  bottom:   0,
  top:    180,
  left:    90,
  right:  -90,
};
