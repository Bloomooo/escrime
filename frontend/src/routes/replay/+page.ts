import { api } from '$lib/api';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ url }) => {
	const wanted = Number(url.searchParams.get('player'));
	return {
		players: await api.listPlayers(),
		preselected: Number.isInteger(wanted) && wanted > 0 ? wanted : null
	};
};
