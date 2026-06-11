import { describe, expect, it } from 'vitest';
import { breakdownFixtures } from '../api/breakdown-fixture';
import type { ScoreEvent } from '../api/types';
import { foldEvents } from './fold';

const events: ScoreEvent[] = [
	{ type: 'match', index: 0, outcome: 'Win', points: 3, runningScore: 3 },
	{ type: 'streakBonus', afterMatchIndex: 0, points: 5, runningScore: 8 },
	{ type: 'penalty', points: -10, runningScore: -2 },
	{ type: 'clampToZero', runningScore: 0 }
];

describe('foldEvents', () => {
	it('should_start_from_zero_when_the_cursor_is_before_the_first_event', () => {
		expect(foldEvents(events, -1)).toEqual({
			score: 0,
			delta: null,
			matchesPlayed: 0,
			isDisqualified: false
		});
	});

	it('should_display_the_running_score_and_points_of_the_last_folded_event_when_stepping', () => {
		const frame = foldEvents(events, 1);

		expect(frame.score).toBe(8);
		expect(frame.delta).toBe(5);
	});

	it('should_clear_the_delta_when_the_current_event_carries_no_points', () => {
		const frame = foldEvents(events, 3);

		expect(frame.score).toBe(0);
		expect(frame.delta).toBeNull();
	});

	it('should_count_matches_and_flag_disqualification_when_folding', () => {
		const run: ScoreEvent[] = [
			{ type: 'match', index: 0, outcome: 'Loss', points: 0, runningScore: 0 },
			{ type: 'disqualification', runningScore: 0 }
		];

		const frame = foldEvents(run, 1);

		expect(frame.matchesPlayed).toBe(1);
		expect(frame.isDisqualified).toBe(true);
	});

	it('should_reach_the_final_score_when_folding_every_fixture_event', () => {
		for (const fixture of breakdownFixtures) {
			const frame = foldEvents(fixture.events, fixture.events.length - 1);

			expect(frame.score).toBe(fixture.finalScore);
		}
	});
});
