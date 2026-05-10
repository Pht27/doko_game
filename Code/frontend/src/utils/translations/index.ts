import { shared } from './shared';
import { game } from './game';
import { analog } from './analog';
import { landing } from './landing';
import { lobby } from './lobby';
import { rules } from './rules';
import { notfound } from './notfound';

type T = typeof shared & typeof game & typeof analog & typeof landing & typeof lobby & typeof rules & typeof notfound;

// Double-cast prevents the LSP from looking through the spread and seeing the truncated inferred type.
// The compiler validates all property accesses against T (the intersection of all sub-modules).
export const t = {
  ...shared,
  ...game,
  ...analog,
  ...landing,
  ...lobby,
  ...rules,
  ...notfound,
} as unknown as T;
