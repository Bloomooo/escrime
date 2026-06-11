// Forward kinematics for a stick fencer. Angles in degrees, y axis down (SVG),
// authored facing +x; the right fencer is mirrored by the stage, never re-authored.
export interface Point {
	x: number;
	y: number;
}

export interface Pose {
	hx: number; // hip offset from the figure anchor
	hy: number; // positive: hip sinks toward the ground
	ft: number; // front thigh
	fs: number; // front shin
	bt: number; // back thigh
	bs: number; // back shin
	to: number; // torso (around -90 = upright)
	su: number; // sword arm, upper
	se: number; // sword arm, forearm
	bl: number; // blade
	ru: number; // rear arm, upper
	re: number; // rear arm, forearm
}

export interface Joints {
	hip: Point;
	kneeF: Point;
	footF: Point;
	kneeB: Point;
	footB: Point;
	shoulder: Point;
	head: Point;
	elbow: Point;
	hand: Point;
	bladeTip: Point;
	rearElbow: Point;
	rearHand: Point;
}

export const THIGH = 40;
export const SHIN = 40;
export const TORSO = 44;
export const UPPER_ARM = 27;
export const FOREARM = 25;
export const BLADE = 78;
export const HEAD_R = 10;

// Distance from the anchor down to the ground: every pose must keep
// thigh + shin drop equal to GROUND_DROP - hy, or feet slide.
export const GROUND_DROP = 76;

const rad = (deg: number) => (deg * Math.PI) / 180;

const seg = (from: Point, angle: number, length: number): Point => ({
	x: from.x + Math.cos(rad(angle)) * length,
	y: from.y + Math.sin(rad(angle)) * length
});

export function solve(pose: Pose): Joints {
	const hip = { x: pose.hx, y: pose.hy };
	const kneeF = seg(hip, pose.ft, THIGH);
	const footF = seg(kneeF, pose.fs, SHIN);
	const kneeB = seg(hip, pose.bt, THIGH);
	const footB = seg(kneeB, pose.bs, SHIN);
	const shoulder = seg(hip, pose.to, TORSO);
	const head = seg(shoulder, pose.to, HEAD_R + 8);
	const elbow = seg(shoulder, pose.su, UPPER_ARM);
	const hand = seg(elbow, pose.se, FOREARM);
	const bladeTip = seg(hand, pose.bl, BLADE);
	const rearElbow = seg(shoulder, pose.ru, UPPER_ARM);
	const rearHand = seg(rearElbow, pose.re, FOREARM);
	return {
		hip,
		kneeF,
		footF,
		kneeB,
		footB,
		shoulder,
		head,
		elbow,
		hand,
		bladeTip,
		rearElbow,
		rearHand
	};
}

// Vertical drop from hip to each foot; feet stay planted when both
// equal GROUND_DROP - hy.
export function footDrops(pose: Pose): { front: number; back: number } {
	return {
		front: THIGH * Math.sin(rad(pose.ft)) + SHIN * Math.sin(rad(pose.fs)),
		back: THIGH * Math.sin(rad(pose.bt)) + SHIN * Math.sin(rad(pose.bs))
	};
}
