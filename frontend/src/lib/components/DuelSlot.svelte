<script lang="ts">
	import type { Player } from '$lib/api';
	import { initials } from '$lib/format/initials';

	interface Props {
		label: string;
		players: Player[];
		excludeId?: number | null;
		selectedId: number | null;
		highlighted?: boolean;
	}

	let {
		label,
		players,
		excludeId = null,
		selectedId = $bindable(null),
		highlighted = false
	}: Props = $props();

	const id = $props.id();

	const selected = $derived(players.find((p) => p.id === selectedId) ?? null);
	// Disqualified fighters stay visible but cannot enter a duel (back rule to come).
	const choices = $derived(players.filter((p) => p.id !== excludeId));
</script>

<div
	class={[
		'flex-1 rounded-card border bg-surface p-5 transition-colors',
		highlighted ? 'border-gold/70' : 'border-border'
	]}
>
	<label for={id} class="sr-only">{label}</label>
	<div class="flex items-center gap-4">
		<div
			class={[
				'flex size-14 shrink-0 items-center justify-center rounded-full border font-display text-lg',
				selected ? 'border-gold/60 text-gold' : 'border-border text-text-muted'
			]}
		>
			{selected ? initials(selected.name) : '?'}
		</div>
		<div class="min-w-0 flex-1">
			<select
				{id}
				bind:value={selectedId}
				class="w-full rounded-[10px] border border-border bg-surface-raised px-3 py-2 text-text"
			>
				<option value={null}>{label}</option>
				{#each choices as player (player.id)}
					<option value={player.id} disabled={player.isDisqualified}>
						{player.name}{player.isDisqualified ? ' · disqualifié' : ''}
					</option>
				{/each}
			</select>
			<p class="mt-2 font-mono text-xs text-text-muted">
				{#if selected}
					score actuel · <span class="text-gold">{selected.score}</span>
					{#if selected.isDisqualified}
						<span class="text-loss"> · disqualifié</span>
					{/if}
				{:else}
					en attente d'un duelliste
				{/if}
			</p>
		</div>
	</div>
</div>
