import { ApiError, api, type Match, type RankingEntry } from '$lib/api';
import type { PageLoad } from './$types';

export const load: PageLoad = async () => {
	const [players, ranking] = await Promise.all([api.listPlayers(), api.getRanking()]);

	let champion: RankingEntry | null = null;
	try {
		champion = await api.getChampion();
	} catch (cause) {
		if (!(cause instanceof ApiError && cause.status === 404)) throw cause;
	}

	const details = await Promise.all(ranking.map((entry) => api.getPlayer(entry.playerId)));
	const matches = new Map<number, Match[]>(details.map((detail) => [detail.id, detail.matches]));

	const disqualified = new Set(players.filter((p) => p.isDisqualified).map((p) => p.id));
	return { ranking, champion, disqualified, matches };
};
