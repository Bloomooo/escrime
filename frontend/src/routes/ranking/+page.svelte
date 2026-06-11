<script lang="ts">
	import type { RankingEntry } from '$lib/api';
	import Crown from '$lib/components/Crown.svelte';
	import EmptyState from '$lib/components/EmptyState.svelte';
	import PageTitle from '$lib/components/PageTitle.svelte';
	import ResultDots from '$lib/components/ResultDots.svelte';
	import { initials } from '$lib/format/initials';
	import { toRoman } from '$lib/format/roman';

	let { data } = $props();

	const podium = $derived([data.ranking[1], data.ranking[0], data.ranking[2]].filter(Boolean));

	function isChampion(entry: RankingEntry): boolean {
		return data.champion?.playerId === entry.playerId;
	}

	function isTied(entry: RankingEntry): boolean {
		return data.ranking.filter((other) => other.rank === entry.rank).length > 1;
	}

	function numeral(entry: RankingEntry): string {
		return data.disqualified.has(entry.playerId) ? '—' : toRoman(entry.rank);
	}
</script>

<svelte:head>
	<title>Classement · Arène d'Acier &amp; d'Arcane</title>
</svelte:head>

<PageTitle title="Classement" subtitle="La hiérarchie des lames, du champion au vaincu." />

{#if data.ranking.length === 0}
	<EmptyState title="Aucun combattant classé." hint="L'arène attend ses premières lames." />
{:else}
	<div class="flex items-end justify-center gap-6">
		{#each podium as entry (entry.playerId)}
			{@const champion = isChampion(entry)}
			<div class="flex flex-col items-center gap-3">
				{#if champion}
					<Crown class="size-5 text-gold" />
				{/if}
				<div
					class={[
						'flex items-center justify-center rounded-full border font-display',
						champion
							? 'size-20 border-gold text-xl text-gold shadow-[0_0_24px_-6px] shadow-gold/50'
							: 'size-16 border-border bg-surface text-text-muted'
					]}
				>
					{initials(entry.name)}
				</div>
				<div
					class={[
						'flex items-center justify-center rounded-sm font-display',
						champion
							? 'h-14 w-32 bg-surface-raised text-gold'
							: 'h-10 w-28 bg-surface text-text-muted'
					]}
				>
					{numeral(entry)}
				</div>
			</div>
		{/each}
	</div>

	<ol class="mt-12 space-y-3">
		{#each data.ranking as entry (entry.playerId)}
			{@const champion = isChampion(entry)}
			{@const banned = data.disqualified.has(entry.playerId)}
			<li
				class={[
					'flex items-center gap-4 rounded-card border p-4 transition-[border-color,transform] duration-150 hover:-translate-y-[3px]',
					champion
						? 'border-gold/70 bg-surface-raised'
						: 'border-border bg-surface hover:border-gold'
				]}
			>
				<div class="flex w-14 shrink-0 flex-col items-center">
					<span
						class={[
							'flex items-center gap-1 font-display text-lg',
							champion ? 'text-gold' : 'text-text-muted'
						]}
					>
						{numeral(entry)}
						{#if champion}
							<Crown class="size-3.5" />
						{/if}
					</span>
					{#if isTied(entry)}
						<span class="font-mono text-[10px] text-text-muted">ex æquo</span>
					{/if}
				</div>
				<div
					class={[
						'flex size-11 shrink-0 items-center justify-center rounded-full border font-display text-sm',
						champion ? 'border-gold/60 text-gold' : 'border-border text-text-muted'
					]}
				>
					{initials(entry.name)}
				</div>
				<div class="min-w-0 flex-1">
					<p class={['truncate font-display', banned ? 'text-text-muted' : 'text-text']}>
						{entry.name}
					</p>
					<div class="mt-1.5">
						<ResultDots matches={data.matches.get(entry.playerId) ?? []} />
					</div>
				</div>
				{#if banned}
					<span
						class="-rotate-3 rounded border border-loss px-2 py-0.5 text-[11px] tracking-widest text-loss uppercase"
					>
						Disqualifié
					</span>
				{/if}
				<p
					class={[
						'font-mono',
						champion
							? 'text-2xl text-gold'
							: banned
								? 'text-xl text-text-muted'
								: 'text-xl text-text'
					]}
				>
					{entry.score}
				</p>
			</li>
		{/each}
	</ol>

	{#if !data.champion}
		<div
			class="mt-10 flex items-center gap-4 rounded-card border border-dashed border-border px-6 py-8"
		>
			<Crown class="size-6 shrink-0 text-text-muted" />
			<div>
				<p class="font-display text-text">Aucun champion</p>
				<p class="mt-1 text-sm text-text-muted">L'arène réclame des héros intègres.</p>
			</div>
		</div>
	{/if}
{/if}
