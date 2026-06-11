import { ApiError, api, type RankingEntry } from '$lib/api';
import type { PageLoad } from './$types';

// The hero must render even when the API is down; stats are optional.
export const load: PageLoad = async () => {
	try {
		const players = await api.listPlayers();
		let champion: RankingEntry | null = null;
		try {
			champion = await api.getChampion();
		} catch (cause) {
			if (!(cause instanceof ApiError && cause.status === 404)) throw cause;
		}
		const fights = players.reduce((sum, p) => sum + p.matchesPlayed, 0);
		return { stats: { fighters: players.length, fights, champion } };
	} catch {
		return { stats: null };
	}
};
