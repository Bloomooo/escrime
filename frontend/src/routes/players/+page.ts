import { api, type Match } from '$lib/api';
import type { PageLoad } from './$types';

export const load: PageLoad = async () => {
	const players = await api.listPlayers();
	const details = await Promise.all(players.map((p) => api.getPlayer(p.id)));
	const matches = new Map<number, Match[]>(details.map((detail) => [detail.id, detail.matches]));
	return { players, matches };
};
