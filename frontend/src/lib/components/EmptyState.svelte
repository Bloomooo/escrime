<script lang="ts">
	let { title, hint }: { title: string; hint?: string } = $props();
</script>

<div class="flex items-center gap-4 rounded-card border border-dashed border-border px-6 py-8">
	<svg
		viewBox="0 0 24 24"
		fill="none"
		stroke="currentColor"
		stroke-width="1.5"
		stroke-linecap="round"
		aria-hidden="true"
		class="size-6 shrink-0 text-text-muted"
	>
		<path d="M5 5l14 14M19 5L5 19M8.5 5.5l-3 3M15.5 5.5l3 3M5.5 15.5l3 3M18.5 15.5l-3 3" />
	</svg>
	<div>
		<p class="font-display text-text">{title}</p>
		{#if hint}
			<p class="mt-1 text-sm text-text-muted">{hint}</p>
		{/if}
	</div>
</div>
