<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import { api, type MatchOutcome } from '$lib/api';
	import ArenaScene, { type SceneMode } from '$lib/components/ArenaScene.svelte';
	import Button from '$lib/components/Button.svelte';
	import DuelSlot from '$lib/components/DuelSlot.svelte';
	import EmptyState from '$lib/components/EmptyState.svelte';
	import PageTitle from '$lib/components/PageTitle.svelte';

	let { data } = $props();

	type Verdict = 'victory' | 'draw' | 'defeat';

	const verdicts: { value: Verdict; label: string; tone: string; selected: string }[] = [
		{ value: 'victory', label: 'Victoire', tone: 'text-win', selected: 'border-win' },
		{ value: 'draw', label: 'Match nul', tone: 'text-draw', selected: 'border-draw' },
		{ value: 'defeat', label: 'Défaite', tone: 'text-loss', selected: 'border-loss' }
	];

	// (left, right) outcomes, stated from the left fighter's point of view.
	const outcomes: Record<Verdict, [MatchOutcome, MatchOutcome]> = {
		victory: ['Win', 'Loss'],
		draw: ['Draw', 'Draw'],
		defeat: ['Loss', 'Win']
	};

	const finales: Record<Verdict, SceneMode> = {
		victory: 'left',
		draw: 'draw',
		defeat: 'right'
	};

	let leftId = $state<number | null>(null);
	let rightId = $state<number | null>(null);
	let verdict = $state<Verdict | null>(null);
	let submitting = $state(false);
	let phase = $state<'idle' | 'fighting' | 'sealed'>('idle');
	let scene = $state<SceneMode>('static');
	let sealed = $state<string | null>(null);
	let feedback = $state<string | null>(null);

	const left = $derived(data.players.find((p) => p.id === leftId) ?? null);
	const right = $derived(data.players.find((p) => p.id === rightId) ?? null);
	const ready = $derived(left !== null && right !== null);

	const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

	function fateVerdict(): Verdict {
		const roll = Math.random();
		if (roll < 0.42) return 'victory';
		if (roll < 0.58) return 'draw';
		return 'defeat';
	}

	function verdictSentence(fate: Verdict, leftName: string, rightName: string): string {
		if (fate === 'victory') return `victoire de ${leftName} face à ${rightName}`;
		if (fate === 'defeat') return `victoire de ${rightName} face à ${leftName}`;
		return `match nul entre ${leftName} et ${rightName}`;
	}

	async function engage() {
		if (!left || !right || submitting) return;
		submitting = true;
		feedback = null;
		sealed = null;

		const fate = verdict ?? fateVerdict();
		const [leftOutcome, rightOutcome] = outcomes[fate];
		const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

		phase = 'fighting';
		scene = reduced ? finales[fate] : 'fighting';

		try {
			const recording = (async () => {
				await api.recordMatch(left.id, leftOutcome);
				await api.recordMatch(right.id, rightOutcome);
			})();

			if (!reduced) await wait(2400);
			scene = finales[fate];
			await recording;
			if (!reduced) await wait(800);

			sealed = verdictSentence(fate, left.name, right.name);
			phase = 'sealed';
			verdict = null;
			await invalidateAll();
		} catch {
			phase = 'idle';
			scene = 'static';
			feedback =
				"Le verdict n'a pas pu être scellé. Vérifiez l'API et les annales des deux duellistes.";
		} finally {
			submitting = false;
		}
	}
</script>

<svelte:head>
	<title>Duel · Arène d'Acier &amp; d'Arcane</title>
</svelte:head>

<PageTitle title="Duel" subtitle="Deux lames entrent, un seul verdict est rendu." />

{#if data.players.length < 2}
	<EmptyState
		title="Il faut deux combattants pour engager un duel."
		hint="Inscrivez des duellistes depuis l'écran Combattants."
	/>
{:else}
	<div class="flex flex-col items-stretch gap-4 md:flex-row md:items-center">
		<DuelSlot
			label="Choisir le premier duelliste"
			players={data.players}
			excludeId={rightId}
			bind:selectedId={leftId}
			highlighted={leftId !== null}
		/>
		<p class="shrink-0 text-center font-display text-3xl text-gold" aria-hidden="true">vs</p>
		<DuelSlot
			label="Choisir le second duelliste"
			players={data.players}
			excludeId={leftId}
			bind:selectedId={rightId}
		/>
	</div>

	<p class="mt-8 text-center font-mono text-xs text-text-muted">
		{#if left}
			le verdict s'énonce du point de vue de {left.name}, à gauche
		{:else}
			le verdict s'énonce du point de vue du duelliste de gauche
		{/if}
	</p>

	<div class="mt-4 flex justify-center gap-3" role="radiogroup" aria-label="Verdict du duel">
		{#each verdicts as option (option.value)}
			<button
				type="button"
				role="radio"
				aria-checked={verdict === option.value}
				onclick={() => (verdict = verdict === option.value ? null : option.value)}
				class={[
					'rounded-[10px] border px-5 py-2.5 text-sm transition-colors',
					option.tone,
					verdict === option.value ? option.selected : 'border-border hover:border-text-muted/40'
				]}
			>
				{option.label}
			</button>
		{/each}
	</div>
	<p class="mt-3 text-center font-mono text-xs text-text-muted">
		sans verdict choisi, le destin tranchera dans l'arène
	</p>

	<div class="mt-8 text-center">
		<Button onclick={engage} disabled={!ready || submitting}>Engager le duel</Button>
		{#if feedback}
			<p role="alert" class="mt-3 text-sm text-loss">{feedback}</p>
		{/if}
	</div>

	<div class="mt-10 rounded-card border border-border bg-arena px-6 py-8">
		{#if phase === 'idle'}
			<p class="text-center font-mono text-xs text-text-muted">
				le duel se joue ici, sous vos yeux
			</p>
		{:else}
			<ArenaScene mode={scene} class="mx-auto w-full max-w-2xl" />
			{#if phase === 'fighting'}
				<p class="mt-4 text-center font-mono text-xs text-text-muted" aria-live="polite">
					le fer chante, l'arène retient son souffle
				</p>
			{:else if sealed}
				<div class="mt-4 flex items-center justify-center gap-3" aria-live="polite">
					<span class="text-win" aria-hidden="true">✓</span>
					<div>
						<p class="text-sm text-text">Duel enregistré · {sealed}</p>
						<p class="font-mono text-xs text-text-muted">scellé aux annales</p>
					</div>
				</div>
			{/if}
		{/if}
	</div>
{/if}
