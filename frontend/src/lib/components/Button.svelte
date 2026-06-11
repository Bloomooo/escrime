<script lang="ts">
	import type { Snippet } from 'svelte';
	import type { HTMLButtonAttributes } from 'svelte/elements';

	interface Props extends HTMLButtonAttributes {
		variant?: 'primary' | 'ghost' | 'danger';
		children: Snippet;
	}

	let { variant = 'primary', children, ...rest }: Props = $props();

	const styles = {
		primary: 'bg-gold font-semibold text-bg hover:bg-gold-bright',
		ghost: 'border border-border text-text-muted hover:border-gold hover:text-text',
		danger: 'border border-border text-loss hover:border-loss'
	};
</script>

<button
	{...rest}
	class={[
		'rounded-[10px] px-4 py-2 text-sm transition-colors disabled:pointer-events-none disabled:opacity-50',
		styles[variant]
	]}
>
	{@render children()}
</button>
