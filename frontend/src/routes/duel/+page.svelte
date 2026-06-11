<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import { api, type MatchOutcome } from '$lib/api';
	import Button from '$lib/components/Button.svelte';
	import DuelSlot from '$lib/components/DuelSlot.svelte';
	import DuelStage, { type DuelOutcome } from '$lib/components/DuelStage.svelte';
	import EmptyState from '$lib/components/EmptyState.svelte';
	import PageTitle from '$lib/components/PageTitle.svelte';

	let { data } = $props();

	// (left, right) recorded outcomes for each arena verdict.
	const outcomes: Record<DuelOutcome, [MatchOutcome, MatchOutcome]> = {
		left: ['Win', 'Loss'],
		draw: ['Draw', 'Draw'],
		right: ['Loss', 'Win']
	};

	let leftId = $state<number | null>(null);
	let rightId = $state<number | null>(null);
	let submitting = $state(false);
	let phase = $state<'idle' | 'fighting' | 'sealed'>('idle');
	let sealed = $state<string | null>(null);
	let feedback = $state<string | null>(null);
	let stage = $state<DuelStage | null>(null);

	const left = $derived(data.players.find((p) => p.id === leftId) ?? null);
	const right = $derived(data.players.find((p) => p.id === rightId) ?? null);
	const ready = $derived(left !== null && right !== null);

	function fate(): DuelOutcome {
		const roll = Math.random();
		if (roll < 0.42) return 'left';
		if (roll < 0.58) return 'draw';
		return 'right';
	}

	function verdictSentence(outcome: DuelOutcome, leftName: string, rightName: string): string {
		if (outcome === 'left') return `victoire de ${leftName} face à ${rightName}`;
		if (outcome === 'right') return `victoire de ${rightName} face à ${leftName}`;
		return `match nul entre ${leftName} et ${rightName}`;
	}

	async function engage() {
		if (!left || !right || !stage || submitting) return;
		submitting = true;
		feedback = null;
		sealed = null;

		const outcome = fate();
		const [leftOutcome, rightOutcome] = outcomes[outcome];

		phase = 'fighting';
		const saving = (async () => {
			await api.recordMatch(left.id, leftOutcome);
			await api.recordMatch(right.id, rightOutcome);
		})();
		const failure = saving.then(
			() => null,
			(cause: unknown) => cause
		);

		await stage.play(outcome);

		if (await failure) {
			phase = 'idle';
			feedback =
				"Le verdict n'a pas pu être scellé. Vérifiez l'API et les annales des deux duellistes.";
		} else {
			sealed = verdictSentence(outcome, left.name, right.name);
			phase = 'sealed';
			await invalidateAll();
		}
		submitting = false;
	}
</script>

<svelte:head>
	<title>Duel · Arène d'Acier &amp; d'Arcane</title>
</svelte:head>

<PageTitle title="Duel" subtitle="Deux lames entrent, l'arène rend son verdict." />

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

	<p class="mt-6 text-center font-mono text-xs text-text-muted">
		nul ne choisit le vainqueur : les lames parlent, l'arène tranche
	</p>

	<div class="mt-6 text-center">
		<Button onclick={engage} disabled={!ready || submitting}>Engager le duel</Button>
		{#if feedback}
			<p role="alert" class="mt-3 text-sm text-loss">{feedback}</p>
		{/if}
	</div>

	<div class="mt-10 rounded-card border border-border bg-arena px-6 py-6">
		<DuelStage bind:this={stage} class="mx-auto w-full max-w-2xl" />
		<p class="mt-2 text-center font-mono text-xs text-text-muted" aria-live="polite">
			{#if phase === 'fighting'}
				le fer chante, l'arène retient son souffle
			{:else if phase === 'sealed' && sealed}
				Duel enregistré · {sealed} · scellé aux annales
			{:else}
				les duellistes attendent, en garde
			{/if}
		</p>
	</div>
{/if}
