<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import { ApiError, api } from '$lib/api';
	import Button from '$lib/components/Button.svelte';
	import ResultDots from '$lib/components/ResultDots.svelte';
	import EmptyState from '$lib/components/EmptyState.svelte';
	import PageTitle from '$lib/components/PageTitle.svelte';
	import Panel from '$lib/components/Panel.svelte';
	import TextField from '$lib/components/TextField.svelte';

	let { data } = $props();

	let name = $state('');
	let submitting = $state(false);
	let feedback = $state<string | null>(null);

	async function enroll(event: SubmitEvent) {
		event.preventDefault();
		if (!name.trim() || submitting) return;
		submitting = true;
		feedback = null;
		try {
			await api.createPlayer(name.trim());
			name = '';
			await invalidateAll();
		} catch (cause) {
			feedback =
				cause instanceof ApiError && cause.status === 400
					? 'Nom invalide : un combattant doit porter un nom.'
					: "L'inscription a échoué. L'API est-elle lancée ?";
		} finally {
			submitting = false;
		}
	}

	async function unenroll(id: number, playerName: string) {
		if (!confirm(`Désinscrire ${playerName} ? Ses combats seront effacés.`)) return;
		feedback = null;
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

<Panel class="mb-10">
	<form onsubmit={enroll} class="flex flex-wrap items-end gap-4">
		<div class="min-w-56 flex-1">
			<TextField
				label="Nom du combattant"
				placeholder="Sir Galahad"
				bind:value={name}
				required
				maxlength={60}
			/>
		</div>
		<Button type="submit" disabled={submitting}>Inscrire</Button>
	</form>
	{#if feedback}
		<p role="alert" class="mt-3 text-sm text-loss">{feedback}</p>
	{/if}
</Panel>

{#if data.players.length === 0}
	<EmptyState
		title="Aucun combattant dans l'arène."
		hint="Inscrivez le premier duelliste ci-dessus."
	/>
{:else}
	<ul class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
		{#each data.players as player (player.id)}
			<li
				class="rounded-card border border-border bg-surface p-6 transition-[border-color,transform] duration-150 hover:-translate-y-[3px] hover:border-gold"
			>
				<div class="flex items-start justify-between gap-3">
					<h2 class="font-display text-lg text-text">{player.name}</h2>
					{#if player.isDisqualified}
						<span
							class="rounded border border-loss px-2 py-0.5 text-[11px] tracking-widest text-loss uppercase"
						>
							Disqualifié
						</span>
					{/if}
				</div>

				<div class="mt-2">
					<ResultDots matches={data.matches.get(player.id) ?? []} />
				</div>

				<p class="mt-4 font-mono text-3xl text-gold">{player.score}</p>
				<p class="text-xs tracking-wide text-text-muted uppercase">points</p>

				<div class="mt-4 flex flex-wrap items-center justify-between gap-2 text-sm text-text-muted">
					<span>
						{player.matchesPlayed}
						{player.matchesPlayed > 1 ? 'combats' : 'combat'}
						{#if player.penaltyPoints > 0}
							<span class="text-loss"> · −{player.penaltyPoints} pénalité</span>
						{/if}
					</span>
					<Button variant="danger" onclick={() => unenroll(player.id, player.name)}>
						Désinscrire
					</Button>
				</div>
			</li>
		{/each}
	</ul>
{/if}
