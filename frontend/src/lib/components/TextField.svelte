<script lang="ts">
	import type { HTMLInputAttributes } from 'svelte/elements';

	interface Props extends HTMLInputAttributes {
		label: string;
		value?: string;
	}

	let { label, value = $bindable(''), ...rest }: Props = $props();

	const id = $props.id();
</script>

<div class="flex flex-col gap-1.5">
	<label for={id} class="text-sm text-text-muted">{label}</label>
	<input
		{id}
		bind:value
		{...rest}
		class="rounded-[10px] border border-border bg-surface-raised px-3 py-2 text-text placeholder:text-text-muted/50"
	/>
</div>
