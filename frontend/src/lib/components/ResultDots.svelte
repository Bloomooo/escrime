<script lang="ts">
	import type { Match, MatchOutcome } from '$lib/api';

	let { matches, max = 12 }: { matches: Match[]; max?: number } = $props();

	const tone: Record<MatchOutcome, string> = { Win: 'bg-win', Draw: 'bg-draw', Loss: 'bg-loss' };
	const label: Record<MatchOutcome, string> = { Win: 'victoire', Draw: 'nul', Loss: 'défaite' };

	const shown = $derived(matches.slice(-max));
</script>

<ol class="flex gap-1" aria-label="Historique des combats, ordre chronologique">
	{#each shown as match (match.id)}
		<li class={['size-1.5 rounded-[2px]', tone[match.result]]}>
			<span class="sr-only">{label[match.result]}</span>
		</li>
	{/each}
</ol>
