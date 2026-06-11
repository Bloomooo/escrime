<script lang="ts">
	import { resolve } from '$app/paths';
	import { page } from '$app/state';
</script>

<div class="flex flex-col items-center py-24 text-center">
	<p class="font-display text-5xl text-loss">{page.status}</p>
	<p class="mt-6 max-w-md text-text-muted">
		{#if page.status === 404}
			Cette salle de l'arène n'existe pas.
		{:else}
			L'arène ne répond plus. Vérifiez que l'API est lancée, puis réessayez.
		{/if}
	</p>
	<a href={resolve('/')} class="mt-8 text-sm text-gold underline-offset-4 hover:underline">
		Retour à l'arène
	</a>
</div>
