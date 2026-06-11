import { demoBreakdown } from './breakdown-fixture';
import type {
	Match,
	MatchOutcome,
	Player,
	PlayerDetail,
	RankingEntry,
	ReplayBreakdown,
	ScoreBreakdown
} from './types';

export class ApiError extends Error {
	constructor(
		readonly status: number,
		message: string
	) {
		super(message);
		this.name = 'ApiError';
	}
}

export interface ApiClientOptions {
	baseUrl: string;
	fetchFn?: typeof fetch;
}

export function createApiClient({ baseUrl, fetchFn = fetch }: ApiClientOptions) {
	const root = baseUrl.replace(/\/+$/, '');

	async function request<T>(path: string, init?: RequestInit): Promise<T> {
		const response = await fetchFn(`${root}${path}`, init);
		if (!response.ok) {
			throw new ApiError(response.status, `${init?.method ?? 'GET'} ${path} a echoue`);
		}
		if (response.status === 204) {
			return undefined as T;
		}
		return (await response.json()) as T;
	}

	function post(body?: unknown): RequestInit {
		return body === undefined
			? { method: 'POST' }
			: {
					method: 'POST',
					headers: { 'content-type': 'application/json' },
					body: JSON.stringify(body)
				};
	}

	return {
		listPlayers: () => request<Player[]>('/api/players'),
		createPlayer: (name: string) => request<Player>('/api/players', post({ name })),
		getPlayer: (id: number) => request<PlayerDetail>(`/api/players/${id}`),
		deletePlayer: (id: number) => request<void>(`/api/players/${id}`, { method: 'DELETE' }),
		recordMatch: (id: number, result: MatchOutcome) =>
			request<Match>(`/api/players/${id}/matches`, post({ result })),
		addPenalty: (id: number, points: number) =>
			request<Player>(`/api/players/${id}/penalties`, post({ points })),
		disqualify: (id: number) => request<Player>(`/api/players/${id}/disqualification`, post()),
		getScoreBreakdown: async (id: number): Promise<ReplayBreakdown> => {
			try {
				const breakdown = await request<ScoreBreakdown>(`/api/players/${id}/score-breakdown`);
				return { ...breakdown, demo: false };
			} catch (cause) {
				// Spec section 5: the endpoint is not live yet, a 404 falls back to demo data.
				if (cause instanceof ApiError && cause.status === 404) {
					return { ...demoBreakdown(id), demo: true };
				}
				throw cause;
			}
		},
		getRanking: () => request<RankingEntry[]>('/api/ranking'),
		getChampion: () => request<RankingEntry>('/api/ranking/champion')
	};
}

export type ApiClient = ReturnType<typeof createApiClient>;
