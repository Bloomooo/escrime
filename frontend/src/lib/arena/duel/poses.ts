import type { Pose } from './skeleton';

// Battle-tested keyframes from the duel animation reference. Full poses move
// the whole body; partial poses compose over whatever the figure is doing,
// because the tween only interpolates the keys present in its target.
export const GUARD: Pose = {
	hx: 0,
	hy: 0,
	ft: 62,
	fs: 99,
	bt: 118,
	bs: 81,
	to: -80,
	su: -8,
	se: 4,
	bl: -12,
	ru: 150,
	re: 95
};

export const ANTICIP: Partial<Pose> = {
	hx: -10,
	hy: 2,
	to: -87,
	su: -26,
	se: -12,
	bl: -32,
	ru: 160,
	re: 108
};

export const LUNGE: Pose = {
	hx: 90,
	hy: 20,
	ft: 24,
	fs: 86,
	bt: 135,
	bs: 140,
	to: -58,
	su: -4,
	se: -2,
	bl: -2,
	ru: 192,
	re: 186
};

export const TENSE: Partial<Pose> = { su: -16, se: -4, bl: -26 };

export const PARRY: Partial<Pose> = { su: -2, se: 12, bl: 30 };

export const HIT: Pose = {
	hx: -30,
	hy: 4,
	ft: 60,
	fs: 108,
	bt: 110,
	bs: 68,
	to: -104,
	su: -42,
	se: -62,
	bl: -85,
	ru: 205,
	re: 235
};

export const SETTLE: Pose = {
	hx: -36,
	hy: 5,
	ft: 62,
	fs: 102,
	bt: 115,
	bs: 78,
	to: -95,
	su: -18,
	se: -26,
	bl: -45,
	ru: 165,
	re: 115
};

// Both fencers surge to mid piste for a draw: blades meet, nobody scores.
export const CLASH: Partial<Pose> = {
	hx: 36,
	hy: 8,
	ft: 38,
	fs: 96,
	bt: 128,
	bs: 110,
	to: -64,
	su: -6,
	se: 2,
	bl: 6
};
