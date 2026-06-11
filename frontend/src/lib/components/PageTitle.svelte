<script lang="ts">
	let { title, subtitle }: { title: string; subtitle: string } = $props();
</script>

<header class="mb-12">
	<h1 class="font-display text-4xl text-text">{title}</h1>
	<div class="mt-4 h-px w-12 bg-gold" aria-hidden="true"></div>
	<p class="mt-4 max-w-prose text-text-muted">{subtitle}</p>
</header>
