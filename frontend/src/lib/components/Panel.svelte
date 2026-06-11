<script lang="ts">
	import type { Snippet } from 'svelte';

	let { children, class: extra = '' }: { children: Snippet; class?: string } = $props();
</script>

<div class={['rounded-card border border-border bg-surface p-6', extra]}>
	{@render children()}
</div>
