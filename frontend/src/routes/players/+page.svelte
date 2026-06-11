<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import { ApiError, api } from '$lib/api';
	import Button from '$lib/components/Button.svelte';
	import EmptyState from '$lib/components/EmptyState.svelte';
	import PageTitle from '$lib/components/PageTitle.svelte';
	import Panel from '$lib/components/Panel.svelte';
	import ResultDots from '$lib/components/ResultDots.svelte';
	import TextField from '$lib/components/TextField.svelte';
	import { initials } from '$lib/format/initials';

	let { data } = $props();

	let query = $state('');
	let forging = $state(false);
	let forgeName = $state('');
	let penalty = $state(0);
	let disqualify = $state(false);
	let submitting = $state(false);
	let feedback = $state<string | null>(null);

	const searchId = $props.id();

	const filtered = $derived(
		data.players.filter((p) => p.name.toLowerCase().includes(query.trim().toLowerCase()))
	);

	function duelsLabel(count: number): string {
		if (count === 0) return 'aucun duel livré';
		return count === 1 ? '1 duel livré' : `${count} duels livrés`;
	}

	function resetForge() {
		forgeName = '';
		penalty = 0;
		disqualify = false;
		feedback = null;
	}

	async function seal(event: SubmitEvent) {
		event.preventDefault();
		if (!forgeName.trim() || submitting) return;
		submitting = true;
		feedback = null;
		try {
			const created = await api.createPlayer(forgeName.trim());
			if (penalty > 0) await api.addPenalty(created.id, penalty);
			if (disqualify) await api.disqualify(created.id);
			resetForge();
			forging = false;
			await invalidateAll();
		} catch (cause) {
			feedback =
				cause instanceof ApiError && cause.status === 400
					? 'Nom invalide : un combattant doit porter un nom.'
					: "La forge a échoué. L'API est-elle lancée ?";
		} finally {
			submitting = false;
		}
	}

	async function unenroll(id: number, playerName: string) {
		if (!confirm(`Désinscrire ${playerName} ? Ses combats seront effacés.`)) return;
		try {
			await api.deletePlayer(id);
			await invalidateAll();
		} catch {
			feedback = 'La désinscription a échoué.';
		}
	}
</script>

<svelte:head>
	<title>Combattants · Arène d'Acier &amp; d'Arcane</title>
</svelte:head>

<PageTitle title="Combattants" subtitle="Inscription et historique des duellistes du tournoi." />

<div class="flex flex-wrap items-center justify-between gap-4">
	<div>
		<label for={searchId} class="sr-only">Chercher un combattant</label>
		<input
			id={searchId}
			type="search"
			placeholder="Chercher un combattant..."
			bind:value={query}
			class="w-72 max-w-full rounded-[10px] border border-border bg-surface-raised px-3 py-2 text-text placeholder:text-text-muted/50"
		/>
	</div>
	{#if !forging}
		<Button onclick={() => (forging = true)}>Ajouter un combattant</Button>
	{/if}
</div>

<div class="mt-8 flex flex-col gap-6 lg:flex-row lg:items-start">
	<div class="min-w-0 flex-1">
		{#if data.players.length === 0}
			<EmptyState title="Aucun combattant" hint="Forgez votre premier héros." />
		{:else if filtered.length === 0}
			<EmptyState title="Aucune lame ne répond à ce nom." hint="Essayez un autre nom." />
		{:else}
			<ul
				class="grid content-start gap-4 [grid-template-columns:repeat(auto-fill,minmax(260px,1fr))]"
			>
				{#each filtered as player (player.id)}
					{@const banned = player.isDisqualified}
					<li
						class="relative flex flex-col rounded-card border border-border bg-surface p-5 transition-[border-color,transform] duration-150 hover:-translate-y-[3px] hover:border-gold"
					>
						<div class={['flex items-center gap-3', banned && 'opacity-60']}>
							<div
								class="flex size-12 shrink-0 items-center justify-center rounded-full border border-border font-display text-sm text-text-muted"
							>
								{initials(player.name)}
							</div>
							<div class="min-w-0">
								<h2 class="truncate font-display text-text">{player.name}</h2>
								<p class="text-xs text-text-muted">{duelsLabel(player.matchesPlayed)}</p>
							</div>
						</div>

						<div class={['mt-4', banned && 'opacity-40 saturate-50']}>
							<ResultDots matches={data.matches.get(player.id) ?? []} />
						</div>

						<div class="mt-5 flex items-end justify-between gap-3">
							<div class="flex flex-wrap gap-2">
								{#if player.penaltyPoints > 0}
									<span class="rounded-full border border-loss/60 px-2.5 py-0.5 text-xs text-loss">
										Pénalité −{player.penaltyPoints}
									</span>
								{/if}
							</div>
							<p class={['font-display text-4xl', banned ? 'text-text-muted' : 'text-gold']}>
								{player.score}
							</p>
						</div>

						<div class="mt-4 flex justify-end border-t border-border pt-3">
							<Button variant="danger" onclick={() => unenroll(player.id, player.name)}>
								Désinscrire
							</Button>
						</div>

						{#if banned}
							<span
								class="pointer-events-none absolute top-14 left-1/2 w-max -translate-x-1/2 -rotate-[8deg] border-2 border-loss bg-bg/60 px-3 py-1 font-display text-sm tracking-[0.3em] text-loss uppercase"
							>
								Disqualifié
							</span>
						{/if}
					</li>
				{/each}
			</ul>
		{/if}
	</div>

	{#if forging}
		<Panel class="w-full shrink-0 lg:max-w-xs">
			<h2 class="font-display text-lg text-text">Forger un combattant</h2>
			<form onsubmit={seal} class="mt-5 space-y-5">
				<TextField
					label="Nom"
					placeholder="Sérane Vael"
					bind:value={forgeName}
					required
					maxlength={60}
				/>

				<div>
					<p class="text-sm text-text-muted">Points de pénalité</p>
					<div class="mt-1.5 flex w-fit items-center rounded-[10px] border border-border">
						<button
							type="button"
							aria-label="Réduire la pénalité"
							onclick={() => (penalty = Math.max(0, penalty - 1))}
							class="px-3 py-1.5 text-text-muted transition-colors hover:text-text"
						>
							−
						</button>
						<span class="w-10 text-center font-mono text-sm text-text">{penalty}</span>
						<button
							type="button"
							aria-label="Augmenter la pénalité"
							onclick={() => (penalty += 1)}
							class="px-3 py-1.5 text-text-muted transition-colors hover:text-text"
						>
							+
						</button>
					</div>
				</div>

				<div class="flex items-center justify-between gap-3">
					<div>
						<p class="text-sm text-text">Disqualifié</p>
						<p class="text-xs text-text-muted">le sceau annule le score</p>
					</div>
					<button
						type="button"
						role="switch"
						aria-checked={disqualify}
						aria-label="Sceau de disqualification"
						onclick={() => (disqualify = !disqualify)}
						class={[
							'flex size-9 items-center justify-center rounded-full border text-lg transition-colors',
							disqualify
								? 'border-loss bg-loss/15 text-loss'
								: 'border-border text-text-muted hover:border-text-muted'
						]}
					>
						×
					</button>
				</div>

				{#if feedback}
					<p role="alert" class="text-sm text-loss">{feedback}</p>
				{/if}

				<div class="flex justify-end gap-3">
					<Button
						variant="ghost"
						type="button"
						onclick={() => {
							resetForge();
							forging = false;
						}}
					>
						Annuler
					</Button>
					<Button type="submit" disabled={submitting}>Sceller</Button>
				</div>
			</form>
		</Panel>
	{/if}
</div>
