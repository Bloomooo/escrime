<script lang="ts">
	let { class: extra = '' }: { class?: string } = $props();
</script>

<!-- Static stand-in for the P2 arena engine: two fencers frozen mid-exchange. -->
<svg viewBox="0 0 800 300" aria-hidden="true" class={['select-none', extra]}>
	<defs>
		<radialGradient id="glow" cx="50%" cy="50%" r="50%">
			<stop offset="0%" stop-color="#d4af37" stop-opacity="0.16" />
			<stop offset="100%" stop-color="#d4af37" stop-opacity="0" />
		</radialGradient>
	</defs>

	<circle cx="140" cy="170" r="120" fill="url(#glow)" />
	<circle cx="660" cy="170" r="120" fill="url(#glow)" />

	<ellipse cx="400" cy="252" rx="310" ry="36" fill="none" stroke="#d4af37" stroke-opacity="0.28" />
	<ellipse
		cx="400"
		cy="252"
		rx="240"
		ry="26"
		fill="none"
		stroke="#d4af37"
		stroke-opacity="0.12"
		stroke-dasharray="2 8"
	/>

	<!-- left fencer, lunging -->
	<g stroke="#3b3354" stroke-width="9" stroke-linecap="round">
		<circle cx="318" cy="138" r="13" fill="#3b3354" stroke="none" />
		<path d="M312 152 L294 192" fill="none" />
		<path d="M294 192 L330 236" fill="none" />
		<path d="M294 192 L252 234" fill="none" />
		<path d="M310 162 L352 152" fill="none" />
	</g>
	<path d="M250 132 Q282 108 312 124" fill="none" stroke="#d4af37" stroke-width="3" />
	<path d="M352 152 L448 144" stroke="#e8e6f0" stroke-width="3" stroke-linecap="round" />
	<circle cx="352" cy="152" r="5" fill="none" stroke="#e8e6f0" stroke-width="2" />

	<!-- right fencer, parrying -->
	<g stroke="#3b3354" stroke-width="9" stroke-linecap="round">
		<circle cx="512" cy="142" r="13" fill="#3b3354" stroke="none" />
		<path d="M518 156 L536 196" fill="none" />
		<path d="M536 196 L508 240" fill="none" />
		<path d="M536 196 L574 232" fill="none" />
		<path d="M522 166 L492 178" fill="none" />
	</g>
	<path d="M492 178 L462 110" stroke="#e8e6f0" stroke-width="3" stroke-linecap="round" />
	<circle cx="492" cy="178" r="5" fill="none" stroke="#e8e6f0" stroke-width="2" />
	<path d="M536 196 L508 240" stroke="#8b5cf6" stroke-width="3" stroke-linecap="round" />

	<!-- clash spark -->
	<rect x="448" y="138" width="12" height="12" fill="#d4af37" transform="rotate(45 454 144)" />
</svg>
