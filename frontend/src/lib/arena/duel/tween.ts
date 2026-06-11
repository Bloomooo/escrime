export type Ease = (t: number) => number;

// Differentiated easing is what sells the realism:
// slow coil, brutal strike, sharp recoil, calm recovery.
export const easeInOutCubic: Ease = (t) =>
	t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
export const easeOutQuart: Ease = (t) => 1 - Math.pow(1 - t, 4);
export const easeOutCubic: Ease = (t) => 1 - Math.pow(1 - t, 3);
export const linear: Ease = (t) => t;

// Interpolates only the keys present in the target, so partial poses compose.
// One Tweener per stage: cancelAll also settles every in-flight promise,
// otherwise awaiting callers would hang after the component is destroyed.
export class Tweener {
	#frames = new Set<number>();
	#timeouts = new Set<ReturnType<typeof setTimeout>>();
	#resolvers = new Set<() => void>();
	#cancelled = false;

	tween<K extends string>(
		target: Record<K, number>,
		to: Partial<Record<K, number>>,
		ms: number,
		ease: Ease,
		onFrame: () => void
	): Promise<void> {
		if (this.#cancelled) return Promise.resolve();
		const keys = Object.keys(to) as K[];
		const from = new Map(keys.map((key) => [key, target[key]]));
		return new Promise((resolve) => {
			const done = this.#register(resolve);
			const start = performance.now();
			const step = (now: number) => {
				if (this.#cancelled) return;
				const t = Math.min((now - start) / ms, 1);
				const k = ease(t);
				for (const key of keys) {
					const a = from.get(key) as number;
					target[key] = a + ((to[key] as number) - a) * k;
				}
				onFrame();
				if (t < 1) this.#schedule(step);
				else done();
			};
			this.#schedule(step);
		});
	}

	wait(ms: number): Promise<void> {
		if (this.#cancelled) return Promise.resolve();
		return new Promise((resolve) => {
			const done = this.#register(resolve);
			const id = setTimeout(() => {
				this.#timeouts.delete(id);
				done();
			}, ms);
			this.#timeouts.add(id);
		});
	}

	cancelAll() {
		this.#cancelled = true;
		for (const id of this.#frames) cancelAnimationFrame(id);
		for (const id of this.#timeouts) clearTimeout(id);
		this.#frames.clear();
		this.#timeouts.clear();
		for (const done of [...this.#resolvers]) done();
	}

	#register(resolve: () => void): () => void {
		const done = () => {
			this.#resolvers.delete(done);
			resolve();
		};
		this.#resolvers.add(done);
		return done;
	}

	#schedule(step: FrameRequestCallback) {
		const id = requestAnimationFrame((now) => {
			this.#frames.delete(id);
			step(now);
		});
		this.#frames.add(id);
	}
}
