<script lang="ts">
	let { title, hint }: { title: string; hint?: string } = $props();
</script>

<div class="rounded-card border border-dashed border-border px-6 py-14 text-center">
	<p class="font-display text-lg text-text-muted">{title}</p>
	{#if hint}
		<p class="mt-2 text-sm text-text-muted/70">{hint}</p>
	{/if}
</div>
