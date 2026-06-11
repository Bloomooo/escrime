<script lang="ts" module>
	export type DuelOutcome = 'left' | 'right' | 'draw';
</script>

<script lang="ts">
	import { onDestroy } from 'svelte';
	import { ANTICIP, CLASH, GUARD, HIT, LUNGE, PARRY, SETTLE, TENSE } from '$lib/arena/duel/poses';
	import { HEAD_R, solve, type Point, type Pose } from '$lib/arena/duel/skeleton';
	import {
		Tweener,
		easeInOutCubic,
		easeOutCubic,
		easeOutQuart,
		linear,
		type Ease
	} from '$lib/arena/duel/tween';

	let { class: extra = '' }: { class?: string } = $props();

	const LEFT_X = 255;
	const RIGHT_X = 505;
	const ANCHOR_Y = 224;

	interface Figure {
		pose: Pose;
		mirrored: boolean;
		nodes: Record<string, SVGElement>;
	}

	const mkFigure = (mirrored: boolean): Figure => ({ pose: { ...GUARD }, mirrored, nodes: {} });
	const left = mkFigure(false);
	const right = mkFigure(true);

	let stageGroup: SVGGElement;
	let ring: SVGCircleElement;

	const tweener = new Tweener();
	let running = false;

	const setLine = (el: SVGElement, a: Point, b: Point) => {
		el.setAttribute('x1', String(a.x));
		el.setAttribute('y1', String(a.y));
		el.setAttribute('x2', String(b.x));
		el.setAttribute('y2', String(b.y));
	};

	function draw(figure: Figure) {
		const n = figure.nodes;
		if (!n.torso) return;
		const j = solve(figure.pose);
		setLine(n.thighB, j.hip, j.kneeB);
		setLine(n.shinB, j.kneeB, j.footB);
		setLine(n.rearUpper, j.shoulder, j.rearElbow);
		setLine(n.rearFore, j.rearElbow, j.rearHand);
		setLine(n.torso, j.hip, j.shoulder);
		setLine(n.thighF, j.hip, j.kneeF);
		setLine(n.shinF, j.kneeF, j.footF);
		setLine(n.upper, j.shoulder, j.elbow);
		setLine(n.fore, j.elbow, j.hand);
		setLine(n.blade, j.hand, j.bladeTip);
		n.head.setAttribute('cx', String(j.head.x));
		n.head.setAttribute('cy', String(j.head.y));
	}

	const drawBoth = () => {
		draw(left);
		draw(right);
	};

	// Absolute blade tip, mirror aware: where the impact ring lands.
	function tipAbs(figure: Figure): Point {
		const tip = solve(figure.pose).bladeTip;
		return {
			x: figure.mirrored ? RIGHT_X - tip.x : LEFT_X + tip.x,
			y: ANCHOR_Y + tip.y
		};
	}

	const pose = (figure: Figure, target: Partial<Pose>, ms: number, ease: Ease) =>
		tweener.tween(figure.pose, target, ms, ease, () => draw(figure));

	function impact(at: Point) {
		ring.setAttribute('cx', String(at.x));
		ring.setAttribute('cy', String(at.y));
		const state = { r: 4, opacity: 1 };
		return tweener.tween(state, { r: 28, opacity: 0 }, 300, easeOutCubic, () => {
			ring.setAttribute('r', String(state.r));
			ring.setAttribute('opacity', String(state.opacity));
		});
	}

	function shake(ms: number, amplitude = 4) {
		const state = { t: 0 };
		return tweener.tween(state, { t: 1 }, ms, linear, () => {
			if (state.t >= 1) {
				stageGroup.removeAttribute('transform');
				return;
			}
			const amp = (1 - state.t) * amplitude;
			const dx = (Math.random() * 2 - 1) * amp;
			const dy = (Math.random() * 2 - 1) * amp;
			stageGroup.setAttribute('transform', `translate(${dx} ${dy})`);
		});
	}

	// One generic sequence; swap argument order to make either side win.
	async function attackSequence(attacker: Figure, defender: Figure) {
		await Promise.all([
			pose(attacker, GUARD, 300, easeInOutCubic),
			pose(defender, GUARD, 300, easeInOutCubic)
		]);
		await Promise.all([
			pose(attacker, ANTICIP, 380, easeInOutCubic),
			pose(defender, TENSE, 380, easeInOutCubic)
		]);
		await Promise.all([
			pose(attacker, LUNGE, 230, easeOutQuart),
			pose(defender, PARRY, 230, easeOutQuart)
		]);
		impact(tipAbs(attacker));
		await Promise.all([shake(200), pose(defender, HIT, 300, easeOutCubic)]);
		await tweener.wait(220);
		await Promise.all([
			pose(attacker, GUARD, 620, easeInOutCubic),
			pose(defender, SETTLE, 620, easeInOutCubic)
		]);
	}

	async function clashSequence(a: Figure, b: Figure) {
		await Promise.all([pose(a, GUARD, 300, easeInOutCubic), pose(b, GUARD, 300, easeInOutCubic)]);
		await Promise.all([
			pose(a, ANTICIP, 380, easeInOutCubic),
			pose(b, ANTICIP, 380, easeInOutCubic)
		]);
		await Promise.all([pose(a, CLASH, 230, easeOutQuart), pose(b, CLASH, 230, easeOutQuart)]);
		const tips = [tipAbs(a), tipAbs(b)];
		impact({ x: (tips[0].x + tips[1].x) / 2, y: (tips[0].y + tips[1].y) / 2 });
		await shake(140, 2.5);
		await tweener.wait(180);
		await Promise.all([pose(a, GUARD, 620, easeInOutCubic), pose(b, GUARD, 620, easeInOutCubic)]);
	}

	function settle(figure: Figure, target: Partial<Pose>) {
		Object.assign(figure.pose, GUARD, target);
		draw(figure);
	}

	export async function play(outcome: DuelOutcome): Promise<void> {
		if (running) return;
		running = true;
		try {
			if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
				settle(left, outcome === 'right' ? SETTLE : {});
				settle(right, outcome === 'left' ? SETTLE : {});
				return;
			}
			if (outcome === 'draw') await clashSequence(left, right);
			else if (outcome === 'left') await attackSequence(left, right);
			else await attackSequence(right, left);
		} finally {
			running = false;
		}
	}

	$effect(drawBoth);

	onDestroy(() => tweener.cancelAll());
</script>

{#snippet fencer(figure: Figure, x: number, bodyColor: string, bladeColor: string)}
	<g
		stroke-linecap="round"
		fill="none"
		transform="translate({x},{ANCHOR_Y}) scale({figure.mirrored ? -1 : 1},1)"
	>
		<line bind:this={figure.nodes.thighB} stroke={bodyColor} stroke-width="6" />
		<line bind:this={figure.nodes.shinB} stroke={bodyColor} stroke-width="6" />
		<line bind:this={figure.nodes.rearUpper} stroke={bodyColor} stroke-width="5" />
		<line bind:this={figure.nodes.rearFore} stroke={bodyColor} stroke-width="5" />
		<line bind:this={figure.nodes.torso} stroke={bodyColor} stroke-width="7" />
		<line bind:this={figure.nodes.thighF} stroke={bodyColor} stroke-width="6" />
		<line bind:this={figure.nodes.shinF} stroke={bodyColor} stroke-width="6" />
		<line bind:this={figure.nodes.upper} stroke={bodyColor} stroke-width="5" />
		<line bind:this={figure.nodes.fore} stroke={bodyColor} stroke-width="5" />
		<line bind:this={figure.nodes.blade} stroke={bladeColor} stroke-width="2.5" />
		<circle bind:this={figure.nodes.head} r={HEAD_R} fill={bodyColor} stroke="none" />
	</g>
{/snippet}

<svg
	viewBox="195 130 420 188"
	role="img"
	aria-label="Deux escrimeurs s'affrontent dans l'arène"
	class={['select-none', extra]}
>
	<g bind:this={stageGroup}>
		<line x1="200" y1="300" x2="610" y2="300" stroke="#2e2542" stroke-width="1.5" />
		<ellipse cx="255" cy="302" rx="44" ry="4" fill="rgba(0,0,0,0.45)" />
		<ellipse cx="505" cy="302" rx="44" ry="4" fill="rgba(0,0,0,0.45)" />
		{@render fencer(left, LEFT_X, '#8b5cf6', '#d4af37')}
		{@render fencer(right, RIGHT_X, '#6e6694', '#c8d2e2')}
		<circle bind:this={ring} r="0" fill="none" stroke="#d4af37" stroke-width="3" opacity="0" />
	</g>
</svg>
