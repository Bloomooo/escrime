import { describe, expect, it } from 'vitest';
import { CLASH, GUARD, HIT, LUNGE, SETTLE } from './poses';
import { GROUND_DROP, footDrops, solve, type Pose } from './skeleton';
import { easeInOutCubic, easeOutCubic, easeOutQuart } from './tween';

// Only full poses carry legs and hip height; partial poses inherit them.
const FULL_POSES: [string, Pose][] = [
	['GUARD', GUARD],
	['LUNGE', LUNGE],
	['HIT', HIT],
	['SETTLE', SETTLE],
	['CLASH', { ...GUARD, ...CLASH }]
];

describe('poses', () => {
	it.each(FULL_POSES)('should_keep_both_feet_on_the_ground_in_%s', (_, pose) => {
		const { front, back } = footDrops(pose);
		const expected = GROUND_DROP - pose.hy;

		expect(Math.abs(front - expected)).toBeLessThanOrEqual(5);
		expect(Math.abs(back - expected)).toBeLessThanOrEqual(5);
	});

	it.each(FULL_POSES)('should_solve_finite_joints_in_%s', (_, pose) => {
		const joints = solve(pose);

		for (const point of Object.values(joints)) {
			expect(Number.isFinite(point.x)).toBe(true);
			expect(Number.isFinite(point.y)).toBe(true);
		}
	});
});

describe('easings', () => {
	it.each([
		['easeInOutCubic', easeInOutCubic],
		['easeOutQuart', easeOutQuart],
		['easeOutCubic', easeOutCubic]
	])('should_start_at_zero_and_end_at_one_for_%s', (_, ease) => {
		expect(ease(0)).toBe(0);
		expect(ease(1)).toBe(1);
		expect(ease(0.5)).toBeGreaterThan(0);
		expect(ease(0.5)).toBeLessThan(1);
	});
});
