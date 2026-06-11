import { describe, expect, it } from 'vitest';
import { ApiError, createApiClient } from './client';
import type { Player, PlayerDetail } from './types';

const galahad: Player = {
	id: 1,
	name: 'Sir Galahad',
	score: 17,
	isDisqualified: false,
	penaltyPoints: 0,
	matchesPlayed: 4
};

const galahadDetail: PlayerDetail = {
	id: 1,
	name: 'Sir Galahad',
	score: 17,
	isDisqualified: false,
	penaltyPoints: 0,
	matches: [
		{ id: 1, result: 'Win' },
		{ id: 2, result: 'Draw' }
	]
};

function fakeFetch(status: number, body?: unknown) {
	const calls: { url: string; init?: RequestInit }[] = [];
	const fetchFn = (async (input: RequestInfo | URL, init?: RequestInit) => {
		calls.push({ url: String(input), init });
		return new Response(body === undefined ? null : JSON.stringify(body), { status });
	}) as typeof fetch;
	return { calls, fetchFn };
}

function clientWith(status: number, body?: unknown) {
	const { calls, fetchFn } = fakeFetch(status, body);
	return { calls, client: createApiClient({ baseUrl: 'http://api.test', fetchFn }) };
}

describe('createApiClient', () => {
	it('should_list_players_when_calling_listPlayers', async () => {
		const { calls, client } = clientWith(200, [galahad]);

		const players = await client.listPlayers();

		expect(calls[0].url).toBe('http://api.test/api/players');
		expect(players).toEqual([galahad]);
	});

	it('should_post_the_name_as_json_when_creating_a_player', async () => {
		const { calls, client } = clientWith(201, galahad);

		const created = await client.createPlayer('Sir Galahad');

		expect(calls[0].url).toBe('http://api.test/api/players');
		expect(calls[0].init?.method).toBe('POST');
		expect(new Headers(calls[0].init?.headers).get('content-type')).toBe('application/json');
		expect(JSON.parse(String(calls[0].init?.body))).toEqual({ name: 'Sir Galahad' });
		expect(created).toEqual(galahad);
	});

	it('should_fetch_the_detail_with_matches_when_getting_a_player', async () => {
		const { calls, client } = clientWith(200, galahadDetail);

		const detail = await client.getPlayer(1);

		expect(calls[0].url).toBe('http://api.test/api/players/1');
		expect(detail).toEqual(galahadDetail);
	});

	it('should_send_a_delete_and_resolve_when_removing_a_player', async () => {
		const { calls, client } = clientWith(204);

		await client.deletePlayer(1);

		expect(calls[0].url).toBe('http://api.test/api/players/1');
		expect(calls[0].init?.method).toBe('DELETE');
	});

	it('should_post_the_result_when_recording_a_match', async () => {
		const { calls, client } = clientWith(201, { id: 5, result: 'Win' });

		const match = await client.recordMatch(1, 'Win');

		expect(calls[0].url).toBe('http://api.test/api/players/1/matches');
		expect(JSON.parse(String(calls[0].init?.body))).toEqual({ result: 'Win' });
		expect(match).toEqual({ id: 5, result: 'Win' });
	});

	it('should_post_the_points_when_adding_a_penalty', async () => {
		const { calls, client } = clientWith(200, { ...galahad, penaltyPoints: 3 });

		const updated = await client.addPenalty(1, 3);

		expect(calls[0].url).toBe('http://api.test/api/players/1/penalties');
		expect(JSON.parse(String(calls[0].init?.body))).toEqual({ points: 3 });
		expect(updated.penaltyPoints).toBe(3);
	});

	it('should_post_without_body_when_disqualifying', async () => {
		const { calls, client } = clientWith(200, { ...galahad, isDisqualified: true, score: 0 });

		const updated = await client.disqualify(1);

		expect(calls[0].url).toBe('http://api.test/api/players/1/disqualification');
		expect(calls[0].init?.method).toBe('POST');
		expect(updated.isDisqualified).toBe(true);
	});

	it('should_fetch_the_ranking_when_calling_getRanking', async () => {
		const entries = [{ rank: 1, playerId: 1, name: 'Sir Galahad', score: 17 }];
		const { calls, client } = clientWith(200, entries);

		const ranking = await client.getRanking();

		expect(calls[0].url).toBe('http://api.test/api/ranking');
		expect(ranking).toEqual(entries);
	});

	it('should_fetch_the_champion_when_calling_getChampion', async () => {
		const entry = { rank: 1, playerId: 1, name: 'Sir Galahad', score: 17 };
		const { calls, client } = clientWith(200, entry);

		const champion = await client.getChampion();

		expect(calls[0].url).toBe('http://api.test/api/ranking/champion');
		expect(champion).toEqual(entry);
	});

	it('should_throw_an_ApiError_with_the_status_when_the_response_is_not_ok', async () => {
		const { client } = clientWith(404);

		const failure = client.getPlayer(99);

		await expect(failure).rejects.toBeInstanceOf(ApiError);
		await expect(failure).rejects.toMatchObject({ status: 404 });
	});

	it('should_normalize_a_trailing_slash_when_building_urls', async () => {
		const { calls, fetchFn } = fakeFetch(200, []);
		const client = createApiClient({ baseUrl: 'http://api.test/', fetchFn });

		await client.listPlayers();

		expect(calls[0].url).toBe('http://api.test/api/players');
	});
});
