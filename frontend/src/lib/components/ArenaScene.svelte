<script lang="ts" module>
	export type SceneMode = 'static' | 'fighting' | 'left' | 'right' | 'draw';
</script>

<script lang="ts">
	let { mode = 'static', class: extra = '' }: { mode?: SceneMode; class?: string } = $props();
</script>

<!-- Static stand-in for the P2 arena engine. Fencer drawings come verbatim from the UI direction mockup. -->
<svg viewBox="-60 -20 530 200" aria-hidden="true" data-mode={mode} class={['select-none', extra]}>
	<defs>
		<radialGradient id="glow" cx="50%" cy="50%" r="50%">
			<stop offset="0%" stop-color="#d4af37" stop-opacity="0.12" />
			<stop offset="100%" stop-color="#d4af37" stop-opacity="0" />
		</radialGradient>
		<radialGradient id="spark-glow" cx="50%" cy="50%" r="50%">
			<stop offset="0%" stop-color="#f2d272" stop-opacity="0.85" />
			<stop offset="45%" stop-color="#d4af37" stop-opacity="0.35" />
			<stop offset="100%" stop-color="#d4af37" stop-opacity="0" />
		</radialGradient>
	</defs>

	<circle cx="-15" cy="85" r="115" fill="url(#glow)" />
	<circle cx="425" cy="85" r="115" fill="url(#glow)" />

	<path
		d="M-60 132 Q205 122 470 132"
		fill="none"
		stroke="#8b5cf6"
		stroke-opacity="0.3"
		stroke-dasharray="3 7"
	/>
	<path d="M-60 150 Q205 140 470 150" fill="none" stroke="#d4af37" stroke-opacity="0.25" />
	<path d="M-60 166 Q205 158 470 166" fill="none" stroke="#d4af37" stroke-opacity="0.08" />

	<g class="fencer-left">
		<ellipse cx="85" cy="142" rx="58" ry="6" fill="rgba(0,0,0,0.45)" />
		<path
			d="M74 98 L24 138"
			stroke="#1B1426"
			stroke-width="11"
			stroke-linecap="round"
			fill="none"
		/>
		<path
			d="M76 98 L116 106 L136 138"
			stroke="#1B1426"
			stroke-width="11"
			stroke-linecap="round"
			stroke-linejoin="round"
			fill="none"
		/>
		<path d="M73 99 L99 70" stroke="#1B1426" stroke-width="17" stroke-linecap="round" fill="none" />
		<path
			d="M96 73 L64 87"
			stroke="#1B1426"
			stroke-width="8.5"
			stroke-linecap="round"
			fill="none"
		/>
		<circle cx="113" cy="56" r="10.5" fill="#1B1426" />
		<path
			d="M113 46 Q103 37 91 38"
			stroke="rgba(212,175,55,0.75)"
			stroke-width="2.4"
			fill="none"
			stroke-linecap="round"
		/>
		<path
			d="M68 96 L94 68"
			stroke="rgba(212,175,55,0.55)"
			stroke-width="2"
			stroke-linecap="round"
			fill="none"
		/>
		<path
			d="M102 71 L140 64"
			stroke="#1B1426"
			stroke-width="9"
			stroke-linecap="round"
			fill="none"
		/>
		<circle cx="146" cy="62.5" r="4.5" fill="none" stroke="#7E8AA0" stroke-width="1.6" />
		<line x1="150" y1="61.8" x2="222" y2="54" stroke="#C8D2E2" stroke-width="2" />
	</g>

	<g transform="translate(180,0)">
		<g class="fencer-right">
			<g transform="translate(230,0) scale(-1,1)">
				<ellipse cx="75" cy="142" rx="50" ry="6" fill="rgba(0,0,0,0.45)" />
				<path
					d="M70 100 L46 138"
					stroke="#1B1426"
					stroke-width="11"
					stroke-linecap="round"
					fill="none"
				/>
				<path
					d="M72 100 L94 112 L102 138"
					stroke="#1B1426"
					stroke-width="11"
					stroke-linecap="round"
					stroke-linejoin="round"
					fill="none"
				/>
				<path
					d="M69 102 L85 68"
					stroke="#1B1426"
					stroke-width="17"
					stroke-linecap="round"
					fill="none"
				/>
				<path
					d="M81 72 L64 54"
					stroke="#1B1426"
					stroke-width="8.5"
					stroke-linecap="round"
					fill="none"
				/>
				<circle cx="98" cy="56" r="10.5" fill="#1B1426" />
				<path
					d="M64 99 L80 66"
					stroke="rgba(139,92,246,0.55)"
					stroke-width="2"
					stroke-linecap="round"
					fill="none"
				/>
				<path
					d="M88 70 L114 77"
					stroke="#1B1426"
					stroke-width="9"
					stroke-linecap="round"
					fill="none"
				/>
				<circle cx="119" cy="77" r="4.5" fill="none" stroke="#7E8AA0" stroke-width="1.6" />
				<line x1="122" y1="74.5" x2="156" y2="16" stroke="#C8D2E2" stroke-width="2" />
			</g>
		</g>
	</g>

	<g class="spark">
		<circle cx="228" cy="50" r="16" fill="url(#spark-glow)" />
		<rect x="222.5" y="44.5" width="11" height="11" fill="#d4af37" transform="rotate(45 228 50)" />
	</g>
</svg>

<style>
	.fencer-left,
	.fencer-right,
	.spark {
		transform-box: fill-box;
		transform-origin: center;
	}

	svg[data-mode='fighting'] .fencer-left {
		animation: advance 0.55s ease-in-out infinite alternate;
	}
	svg[data-mode='fighting'] .fencer-right {
		animation: retreat 0.55s ease-in-out infinite alternate;
	}
	svg[data-mode='fighting'] .spark {
		animation: pulse 0.55s ease-in-out infinite;
	}

	svg[data-mode='left'] .fencer-left {
		animation: strike 0.5s cubic-bezier(0.34, 1.3, 0.64, 1) both;
	}
	svg[data-mode='left'] .fencer-right {
		animation: fall-right 0.6s ease-out both;
	}

	svg[data-mode='right'] .fencer-right {
		animation: strike-back 0.5s cubic-bezier(0.34, 1.3, 0.64, 1) both;
	}
	svg[data-mode='right'] .fencer-left {
		animation: fall-left 0.6s ease-out both;
	}

	svg[data-mode='draw'] .fencer-left,
	svg[data-mode='draw'] .fencer-right {
		animation: salute 0.8s ease-out both;
	}
	svg[data-mode='left'] .spark,
	svg[data-mode='right'] .spark,
	svg[data-mode='draw'] .spark {
		animation: fade 0.6s ease-out both;
	}

	@keyframes advance {
		from {
			transform: translateX(-10px);
		}
		to {
			transform: translateX(24px);
		}
	}
	@keyframes retreat {
		from {
			transform: translateX(10px);
		}
		to {
			transform: translateX(-24px);
		}
	}
	@keyframes pulse {
		0%,
		100% {
			opacity: 0.35;
			transform: scale(0.7);
		}
		50% {
			opacity: 1;
			transform: scale(1.3);
		}
	}
	@keyframes strike {
		to {
			transform: translateX(66px);
		}
	}
	@keyframes strike-back {
		to {
			transform: translateX(-66px);
		}
	}
	@keyframes fall-right {
		to {
			transform: translateX(30px) rotate(9deg);
			opacity: 0.45;
		}
	}
	@keyframes fall-left {
		to {
			transform: translateX(-30px) rotate(-9deg);
			opacity: 0.45;
		}
	}
	@keyframes salute {
		to {
			transform: translateY(-8px);
		}
	}
	@keyframes fade {
		to {
			opacity: 0;
			transform: scale(0.4);
		}
	}

	@media (prefers-reduced-motion: reduce) {
		svg :is(.fencer-left, .fencer-right, .spark) {
			animation: none !important;
		}
	}
</style>
