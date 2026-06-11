import { describe, expect, it } from 'vitest';
import { Tweener, linear } from './tween';

// Node has no rAF; a setTimeout shim reproduces its timing contract.
globalThis.requestAnimationFrame ??= (cb: FrameRequestCallback) =>
	setTimeout(() => cb(performance.now()), 1) as unknown as number;
globalThis.cancelAnimationFrame ??= (id: number) => clearTimeout(id);

describe('Tweener', () => {
	it('should_reach_only_the_target_keys_when_the_tween_completes', async () => {
		const tweener = new Tweener();
		const target = { a: 0, b: 10 };
		let frames = 0;

		await tweener.tween(target, { a: 100 }, 40, linear, () => (frames += 1));

		expect(target.a).toBe(100);
		expect(target.b).toBe(10);
		expect(frames).toBeGreaterThan(0);
	});

	it('should_settle_in_flight_tweens_when_cancelled', async () => {
		const tweener = new Tweener();
		const target = { a: 0 };

		const pending = tweener.tween(target, { a: 100 }, 5000, linear, () => {});
		tweener.cancelAll();

		await expect(pending).resolves.toBeUndefined();
		expect(target.a).toBeLessThan(100);
	});

	it('should_settle_pending_waits_when_cancelled', async () => {
		const tweener = new Tweener();

		const pending = tweener.wait(5000);
		tweener.cancelAll();

		await expect(pending).resolves.toBeUndefined();
	});

	it('should_resolve_immediately_when_already_cancelled', async () => {
		const tweener = new Tweener();
		tweener.cancelAll();

		await expect(tweener.tween({ a: 0 }, { a: 1 }, 50, linear, () => {})).resolves.toBeUndefined();
		await expect(tweener.wait(50)).resolves.toBeUndefined();
	});

	it('should_complete_a_wait_after_the_delay', async () => {
		const tweener = new Tweener();

		await expect(tweener.wait(10)).resolves.toBeUndefined();
	});
});
