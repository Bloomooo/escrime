<script lang="ts">
	let canvas: HTMLCanvasElement;

	interface Ember {
		x: number;
		y: number;
		speed: number;
		radius: number;
		phase: number;
		sway: number;
		alpha: number;
	}

	// Motion spec: <= 150 canvas particles, rising 12-30 px/s, sine twinkle.
	const COUNT = 150;

	$effect(() => {
		if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
		const ctx = canvas.getContext('2d');
		if (!ctx) return;

		const dpr = Math.min(window.devicePixelRatio || 1, 2);
		let width = 0;
		let height = 0;

		function resize() {
			width = canvas.clientWidth;
			height = canvas.clientHeight;
			canvas.width = width * dpr;
			canvas.height = height * dpr;
			ctx?.setTransform(dpr, 0, 0, dpr, 0, 0);
		}
		resize();
		const observer = new ResizeObserver(resize);
		observer.observe(canvas);

		function spawn(ember: Ember, anywhere = false) {
			ember.x = Math.random() * width;
			ember.y = anywhere ? Math.random() * height : height + 4;
			ember.speed = 12 + Math.random() * 18;
			ember.radius = 0.6 + Math.random() * 1.4;
			ember.phase = Math.random() * Math.PI * 2;
			ember.sway = 6 + Math.random() * 10;
			ember.alpha = 0.15 + Math.random() * 0.45;
		}

		const embers: Ember[] = Array.from({ length: COUNT }, () => {
			const ember = {} as Ember;
			spawn(ember, true);
			return ember;
		});

		let last = performance.now();
		let frame = requestAnimationFrame(function tick(now: number) {
			const dt = Math.min((now - last) / 1000, 0.05);
			last = now;
			ctx.clearRect(0, 0, width, height);
			ctx.fillStyle = '#d4af37';
			for (const ember of embers) {
				ember.y -= ember.speed * dt;
				ember.phase += dt;
				if (ember.y < -4) spawn(ember);
				ctx.globalAlpha = ember.alpha * (0.6 + 0.4 * Math.sin(ember.phase * 3));
				ctx.beginPath();
				ctx.arc(
					ember.x + Math.sin(ember.phase) * ember.sway,
					ember.y,
					ember.radius,
					0,
					Math.PI * 2
				);
				ctx.fill();
			}
			frame = requestAnimationFrame(tick);
		});

		return () => {
			cancelAnimationFrame(frame);
			observer.disconnect();
		};
	});
</script>

<canvas bind:this={canvas} class="pointer-events-none absolute inset-0 size-full" aria-hidden="true"
></canvas>
