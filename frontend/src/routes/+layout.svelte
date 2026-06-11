<script lang="ts">
	import { resolve } from '$app/paths';
	import { page } from '$app/state';
	import './layout.css';
	import blason from '$lib/assets/blason-or.svg';

	let { children } = $props();

	const links = [
		{ href: resolve('/'), label: 'Arène' },
		{ href: resolve('/players'), label: 'Combattants' },
		{ href: resolve('/duel'), label: 'Duel' },
		{ href: resolve('/replay'), label: 'Reconstitution' },
		{ href: resolve('/ranking'), label: 'Classement' }
	];

	const isActive = (href: string) =>
		href === '/' ? page.url.pathname === '/' : page.url.pathname.startsWith(href);
</script>

<svelte:head>
	<link rel="icon" href="/favicon.ico" sizes="any" />
	<link rel="icon" type="image/svg+xml" href="/blason-simplifie-favicon.svg" />
	<link rel="apple-touch-icon" href="/apple-touch-icon-180.png" />
</svelte:head>

<div class="flex min-h-dvh flex-col">
	<a
		href="#contenu"
		class="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-10 focus:bg-surface-raised focus:px-4 focus:py-2 focus:text-text"
	>
		Aller au contenu
	</a>

	<header class="border-b border-border">
		<div class="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-4 px-6 py-4">
			<a href={resolve('/')} class="flex items-center gap-3">
				<img src={blason} alt="" class="h-8 w-auto" />
				<span class="font-display text-sm font-semibold tracking-[0.25em] text-gold uppercase">
					Arène d'Acier &amp; d'Arcane
				</span>
			</a>
			<nav aria-label="Navigation principale">
				<ul class="flex flex-wrap gap-1">
					{#each links as link (link.href)}
						<li>
							<a
								href={link.href}
								aria-current={isActive(link.href) ? 'page' : undefined}
								class={[
									'px-3 py-2 text-sm transition-colors',
									isActive(link.href)
										? 'text-text underline decoration-gold decoration-1 underline-offset-8'
										: 'text-text-muted hover:text-text'
								]}
							>
								{link.label}
							</a>
						</li>
					{/each}
				</ul>
			</nav>
		</div>
	</header>

	<main id="contenu" class="mx-auto w-full max-w-5xl flex-1 px-6 py-12">
		{@render children()}
	</main>
</div>
