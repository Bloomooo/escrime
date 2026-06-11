<script lang="ts">
	import { resolve } from '$app/paths';
	import blason from '$lib/assets/blason-or-etincelle-arcane.svg';
	import ArenaScene from '$lib/components/ArenaScene.svelte';
	import Embers from '$lib/components/Embers.svelte';

	let { data } = $props();
</script>

<svelte:head>
	<title>Arène d'Acier &amp; d'Arcane</title>
</svelte:head>

<section class="relative">
	<Embers />

	<div class="relative flex flex-col items-center pt-12 text-center">
		<img
			src={blason}
			alt=""
			class="h-24 w-auto animate-[hero-rise_0.8s_cubic-bezier(0.4,0,0.2,1)_both]"
		/>
		<h1
			class="mt-10 max-w-3xl font-display text-5xl leading-tight text-balance text-text animate-[hero-rise_0.8s_cubic-bezier(0.4,0,0.2,1)_0.15s_both] sm:text-6xl"
		>
			Le tournoi se regarde, il ne se consulte pas
		</h1>
		<p
			class="mt-8 max-w-xl text-text-muted animate-[hero-rise_0.8s_cubic-bezier(0.4,0,0.2,1)_0.3s_both]"
		>
			Chaque duel des annales se rejoue dans l'arène, et le score se forge sous vos yeux, règle
			après règle : la victoire, la série, la sanction.
		</p>
		<div
			class="mt-10 flex flex-wrap justify-center gap-4 animate-[hero-rise_0.8s_cubic-bezier(0.4,0,0.2,1)_0.45s_both]"
		>
			<a
				href={resolve('/duel')}
				class="rounded-[10px] bg-gold px-5 py-2.5 text-sm font-semibold text-bg transition-colors hover:bg-gold-bright"
			>
				Entrer dans l'arène
			</a>
			<a
				href={resolve('/ranking')}
				class="rounded-[10px] border border-border px-5 py-2.5 text-sm text-text-muted transition-colors hover:border-gold hover:text-text"
			>
				Voir le classement
			</a>
		</div>
	</div>

	<ArenaScene class="mx-auto mt-6 w-full max-w-3xl" />
</section>

{#if data.stats}
	<div class="mt-6 grid border-t border-border sm:grid-cols-3">
		<div class="border-b border-border px-6 py-6 sm:border-r sm:border-b-0">
			<p class="font-mono text-3xl text-text">{data.stats.fighters}</p>
			<p class="mt-1 text-sm text-text-muted">combattants</p>
		</div>
		<div class="border-b border-border px-6 py-6 sm:border-r sm:border-b-0">
			<p class="font-mono text-3xl text-text">{data.stats.fights}</p>
			<p class="mt-1 text-sm text-text-muted">combats consignés</p>
		</div>
		<div class="px-6 py-6">
			<p class="truncate font-mono text-3xl text-text">
				{data.stats.champion?.name ?? 'trône vide'}
			</p>
			<p class="mt-1 text-sm text-text-muted">
				{data.stats.champion ? 'champion en titre' : "l'arène réclame des héros intègres"}
			</p>
		</div>
	</div>
{/if}
