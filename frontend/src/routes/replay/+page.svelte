<script lang="ts">
	import { onDestroy } from 'svelte';
	import { api, type MatchOutcome, type ScoreBreakdown, type ScoreEvent } from '$lib/api';
	import ArenaScene, { type SceneMode } from '$lib/components/ArenaScene.svelte';
	import Button from '$lib/components/Button.svelte';
	import EmptyState from '$lib/components/EmptyState.svelte';
	import PageTitle from '$lib/components/PageTitle.svelte';
	import { foldEvents } from '$lib/replay/fold';

	let { data } = $props();

	type Phase = 'idle' | 'fighting' | 'finale' | 'event' | 'paused' | 'verdict';

	// Beat lengths of the replay, in ms; every displayed value comes from the fold.
	const FIGHT_MS = 1800;
	const FINALE_MS = 700;
	const EVENT_MS = 1200;

	const selectId = $props.id();

	let selectedId = $state<number | null>(null);
	let breakdown = $state<ScoreBreakdown | null>(null);
	let loadFailed = $state(false);
	let cursor = $state(-1);
	let phase = $state<Phase>('idle');
	let playing = $state(false);
	let timer: ReturnType<typeof setTimeout> | undefined;

	const selected = $derived(data.players.find((p) => p.id === selectedId) ?? null);
	const events = $derived(breakdown?.events ?? []);
	const totalMatches = $derived(events.filter((e) => e.type === 'match').length);
	const current = $derived(cursor >= 0 && cursor < events.length ? events[cursor] : null);
	// During a fight the previous score holds; it lands with the finale.
	const frame = $derived(foldEvents(events, phase === 'fighting' ? cursor - 1 : cursor));
	const displayedScore = $derived(
		phase === 'verdict' && breakdown ? breakdown.finalScore : frame.score
	);
	const settledOnEvent = $derived(phase === 'finale' || phase === 'event' || phase === 'paused');
	const displayedDelta = $derived(settledOnEvent ? frame.delta : null);
	const streakFlash = $derived(phase === 'event' && current?.type === 'streakBonus');
	const penaltyHit = $derived(settledOnEvent && current?.type === 'penalty');
	const clampHit = $derived(settledOnEvent && current?.type === 'clampToZero');

	const finaleModes: Record<MatchOutcome, SceneMode> = { Win: 'left', Loss: 'right', Draw: 'draw' };

	const arenaMode: SceneMode = $derived.by(() => {
		if (phase === 'fighting') return 'fighting';
		if ((phase === 'finale' || phase === 'paused') && current?.type === 'match') {
			return finaleModes[current.outcome];
		}
		return 'static';
	});

	const duelCaption = $derived.by(() => {
		if (totalMatches === 0) return 'aucun duel aux annales';
		const n = current?.type === 'match' ? current.index + 1 : Math.max(1, frame.matchesPlayed);
		return `duel ${n} sur ${totalMatches}`;
	});

	function clearTimer() {
		clearTimeout(timer);
		timer = undefined;
	}

	function stepForward() {
		if (cursor >= events.length - 1) {
			goToVerdict();
			return;
		}
		cursor += 1;
		if (events[cursor].type === 'match') {
			phase = 'fighting';
			timer = setTimeout(() => {
				phase = 'finale';
				timer = setTimeout(stepForward, FINALE_MS);
			}, FIGHT_MS);
		} else {
			phase = 'event';
			timer = setTimeout(stepForward, EVENT_MS);
		}
	}

	function play() {
		if (!breakdown) return;
		if (phase === 'verdict') cursor = -1;
		playing = true;
		stepForward();
	}

	function pause() {
		clearTimer();
		playing = false;
		if (phase !== 'idle' && phase !== 'verdict') phase = cursor === -1 ? 'idle' : 'paused';
	}

	function togglePlay() {
		if (playing) pause();
		else play();
	}

	function goToVerdict() {
		clearTimer();
		playing = false;
		cursor = events.length - 1;
		phase = 'verdict';
	}

	function jumpTo(index: number) {
		clearTimer();
		playing = false;
		if (index >= events.length) {
			goToVerdict();
			return;
		}
		cursor = Math.max(-1, index);
		phase = cursor === -1 ? 'idle' : 'paused';
	}

	async function loadBreakdown(id: number) {
		clearTimer();
		playing = false;
		breakdown = null;
		loadFailed = false;
		cursor = -1;
		phase = 'idle';
		try {
			const loaded = await api.getScoreBreakdown(id);
			if (selectedId !== id) return;
			breakdown = loaded;
			if (loaded.events.length === 0) goToVerdict();
			else play();
		} catch {
			if (selectedId === id) loadFailed = true;
		}
	}

	// Deep link from the players and ranking screens: /replay?player={id}.
	let preselectConsumed = false;
	$effect(() => {
		if (preselectConsumed || data.preselected === null) return;
		preselectConsumed = true;
		if (data.players.some((p) => p.id === data.preselected)) {
			selectedId = data.preselected;
			onSelect();
		}
	});

	function onSelect() {
		if (selectedId === null) {
			clearTimer();
			playing = false;
			breakdown = null;
			loadFailed = false;
			cursor = -1;
			phase = 'idle';
			return;
		}
		void loadBreakdown(selectedId);
	}

	function onKeydown(keyEvent: KeyboardEvent) {
		if (!breakdown) return;
		const target = keyEvent.target;
		if (
			target instanceof HTMLElement &&
			['SELECT', 'INPUT', 'TEXTAREA', 'BUTTON'].includes(target.tagName)
		) {
			return;
		}
		if (keyEvent.key === ' ') {
			keyEvent.preventDefault();
			togglePlay();
		} else if (keyEvent.key === 'ArrowRight') {
			keyEvent.preventDefault();
			jumpTo(cursor + 1);
		} else if (keyEvent.key === 'ArrowLeft') {
			keyEvent.preventDefault();
			jumpTo(cursor - 1);
		} else if (keyEvent.key === 'v' || keyEvent.key === 'V') {
			goToVerdict();
		}
	}

	onDestroy(clearTimer);

	const outcomeLetters: Record<MatchOutcome, string> = { Win: 'V', Draw: 'N', Loss: 'D' };

	function signed(points: number): string {
		return points >= 0 ? `+${points}` : String(points);
	}

	function tokenLabel(event: ScoreEvent): string {
		switch (event.type) {
			case 'match':
				return `${outcomeLetters[event.outcome]} ${signed(event.points)}`;
			case 'streakBonus':
				return `${signed(event.points)} série`;
			case 'penalty':
				return `${signed(event.points)} pénalité`;
			case 'clampToZero':
				return 'plancher : 0';
			case 'disqualification':
				return 'disqualifié';
		}
	}

	function tokenTone(event: ScoreEvent): { text: string; edge: string } {
		if (event.type === 'streakBonus') return { text: 'text-arcane', edge: 'border-arcane' };
		if (event.type === 'clampToZero') return { text: 'text-text-muted', edge: 'border-text-muted' };
		if (event.type === 'match' && event.outcome === 'Win')
			return { text: 'text-win', edge: 'border-win' };
		if (event.type === 'match' && event.outcome === 'Draw')
			return { text: 'text-draw', edge: 'border-draw' };
		return { text: 'text-loss', edge: 'border-loss' };
	}
</script>

{#snippet ornament()}
	<svg viewBox="0 0 120 32" aria-hidden="true" class="hidden h-7 w-28 text-gold/50 sm:block">
		<path
			d="M0 16 H120 M28 4 L66 28 M28 28 L66 4"
			stroke="currentColor"
			stroke-width="1.5"
			fill="none"
		/>
	</svg>
{/snippet}

<svelte:window onkeydown={onKeydown} />

<svelte:head>
	<title>Reconstitution · Arène d'Acier &amp; d'Arcane</title>
</svelte:head>

<PageTitle
	title="Reconstitution"
	subtitle="Revivez chaque assaut, le score se forge sous vos yeux."
/>

{#if data.players.length === 0}
	<EmptyState
		title="Aucun combattant à rejouer."
		hint="Inscrivez des duellistes depuis l'écran Combattants."
	/>
{:else}
	<div class="flex flex-wrap items-center gap-x-4 gap-y-2">
		<label for={selectId} class="text-sm text-text-muted">Combattant à rejouer</label>
		<select
			id={selectId}
			bind:value={selectedId}
			onchange={onSelect}
			class="rounded-[10px] border border-border bg-surface-raised px-3 py-2 text-text"
		>
			<option value={null}>Choisir un combattant</option>
			{#each data.players as player (player.id)}
				<option value={player.id}>{player.name}</option>
			{/each}
		</select>
	</div>

	{#if loadFailed}
		<p role="alert" class="mt-8 text-sm text-loss">
			Les annales restent closes. Vérifiez l'API, puis choisissez le combattant à nouveau.
		</p>
	{:else if !breakdown || !selected}
		<div class="mt-8 rounded-card border border-dashed border-border px-6 py-10 text-center">
			<p class="font-display text-text">L'arène attend son récit.</p>
			<p class="mt-1 text-sm text-text-muted">Choisissez un combattant pour rouvrir ses annales.</p>
		</div>
	{:else}
		<section
			class="stage relative mt-8 flex flex-col overflow-hidden rounded-card border border-border md:aspect-[16/7]"
			aria-label="Scène de reconstitution"
		>
			<div
				class="pointer-events-none absolute inset-x-10 top-4 flex justify-between"
				aria-hidden="true"
			>
				{#each [0, 1, 2, 3, 4, 5] as arch (arch)}
					<div class="arch h-24 w-14 rounded-t-full sm:h-28 sm:w-16"></div>
				{/each}
			</div>

			<header class="relative pt-7 text-center">
				<p class="font-mono text-[11px] tracking-[0.3em] text-text-muted uppercase">
					Reprise des annales
				</p>
				<h2 class="mt-2 font-display text-2xl text-text sm:text-3xl">{selected.name}</h2>
				<p class="mt-1 font-mono text-xs text-text-muted">{duelCaption}</p>
			</header>

			<div
				class={[
					'relative flex flex-1 items-end justify-center transition-[filter] duration-700',
					frame.isDisqualified && 'dimmed'
				]}
			>
				<ArenaScene mode={arenaMode} class="w-full max-w-2xl" />
			</div>
		</section>

		<section class="mt-8" aria-label="Forge du score">
			<div class="flex items-center justify-center gap-5">
				{@render ornament()}
				{#key cursor}
					<div
						class={[
							'relative min-w-56 rounded-card border border-border bg-surface px-10 py-6 text-center',
							penaltyHit && 'forge-shake',
							clampHit && 'forge-bounce'
						]}
					>
						<p class="font-mono text-[11px] tracking-[0.3em] text-text-muted uppercase">
							Forge du score
						</p>
						<div class="mt-2 flex items-baseline justify-center gap-3">
							<p
								class={[
									'font-display text-6xl leading-none',
									penaltyHit ? 'text-loss' : 'text-gold'
								]}
							>
								{displayedScore}
							</p>
							{#if displayedDelta !== null}
								<p
									class={[
										'font-mono text-xl',
										displayedDelta < 0
											? 'text-loss'
											: current?.type === 'streakBonus'
												? 'text-arcane'
												: 'text-text-muted'
									]}
								>
									{signed(displayedDelta)}
								</p>
							{/if}
						</div>
						{#if streakFlash && displayedDelta !== null}
							<div class="pointer-events-none absolute inset-0 flex items-center justify-center">
								<p class="streak-flash font-display text-3xl text-arcane">
									{signed(displayedDelta)} SÉRIE
								</p>
							</div>
						{/if}
						{#if frame.isDisqualified}
							<div class="pointer-events-none absolute inset-0 flex items-center justify-center">
								<p
									class="stamp border-2 border-loss bg-bg/70 px-4 py-1 font-display text-lg tracking-[0.3em] text-loss uppercase"
								>
									Disqualifié
								</p>
							</div>
						{/if}
					</div>
				{/key}
				{@render ornament()}
			</div>

			{#if phase === 'verdict'}
				<p class="mt-5 text-center text-sm text-text-muted">
					{#if breakdown.isDisqualified}
						Les annales se ferment sur une disqualification, score scellé à {breakdown.finalScore}.
					{:else}
						Les annales se ferment sur un score de {breakdown.finalScore}.
					{/if}
				</p>
			{/if}
		</section>

		<ol class="mt-8 flex flex-wrap justify-center gap-2" aria-label="Chronologie des événements">
			{#each events as event, i (i)}
				{@const tone = tokenTone(event)}
				<li>
					<button
						type="button"
						onclick={() => jumpTo(i)}
						aria-current={i === cursor ? 'step' : undefined}
						aria-label={`Aller à l'événement ${i + 1} : ${tokenLabel(event)}`}
						class={[
							'border px-3 py-1 font-mono text-xs transition-opacity',
							event.type === 'disqualification' ? 'tracking-widest uppercase' : 'rounded-full',
							tone.text,
							i === cursor ? `${tone.edge} bg-surface-raised` : 'border-border',
							i < cursor && 'opacity-60',
							i > cursor && 'opacity-30'
						]}
					>
						{tokenLabel(event)}
					</button>
				</li>
			{/each}
		</ol>

		<div class="mt-8 flex flex-wrap items-center justify-center gap-3">
			<Button variant="ghost" onclick={() => jumpTo(cursor - 1)} disabled={cursor <= -1}>
				Précédent
			</Button>
			<Button onclick={togglePlay}>
				{playing ? 'Pause' : phase === 'verdict' ? 'Rejouer' : 'Lecture'}
			</Button>
			<Button variant="ghost" onclick={() => jumpTo(cursor + 1)} disabled={phase === 'verdict'}>
				Suivant
			</Button>
			<Button variant="ghost" onclick={goToVerdict} disabled={phase === 'verdict'}>Verdict</Button>
		</div>
		<p class="mt-3 text-center font-mono text-xs text-text-muted">
			espace lecture et pause · flèches précédent et suivant · v verdict
		</p>
	{/if}
{/if}

<style>
	/* Maquette S4: the stage is a darker obsidian pit than the page background. */
	.stage {
		background-color: #0b0912;
	}

	.arch {
		background-color: rgba(139, 92, 246, 0.045);
	}

	.dimmed {
		filter: saturate(0.15) brightness(0.85);
	}

	/* The stamp stays slanted even when the print animation is skipped. */
	.stamp {
		transform: rotate(-8deg);
	}

	@media (prefers-reduced-motion: no-preference) {
		.forge-shake {
			animation: forge-shake 0.45s ease-in-out;
		}

		.forge-bounce {
			animation: forge-bounce 0.55s cubic-bezier(0.34, 1.56, 0.64, 1);
		}

		.streak-flash {
			animation: streak-flash 1.1s ease-out both;
		}

		.stamp {
			animation: stamp-print 0.4s cubic-bezier(0.2, 1.2, 0.4, 1) both;
		}
	}

	@keyframes forge-shake {
		0%,
		100% {
			transform: translateX(0);
		}
		20% {
			transform: translateX(-7px);
		}
		40% {
			transform: translateX(6px);
		}
		60% {
			transform: translateX(-4px);
		}
		80% {
			transform: translateX(2px);
		}
	}

	@keyframes forge-bounce {
		0% {
			transform: translateY(-10px);
		}
		45% {
			transform: translateY(4px);
		}
		75% {
			transform: translateY(-2px);
		}
		100% {
			transform: translateY(0);
		}
	}

	@keyframes streak-flash {
		0% {
			opacity: 0;
			transform: scale(0.6);
		}
		25% {
			opacity: 1;
			transform: scale(1.1);
		}
		100% {
			opacity: 0;
			transform: scale(1.5);
		}
	}

	@keyframes stamp-print {
		from {
			opacity: 0;
			transform: rotate(-8deg) scale(1.9);
		}
		to {
			opacity: 1;
			transform: rotate(-8deg) scale(1);
		}
	}
</style>
