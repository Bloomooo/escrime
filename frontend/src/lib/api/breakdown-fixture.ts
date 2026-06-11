import type { ScoreBreakdown } from './types';

// Demo stories served while GET /api/players/{id}/score-breakdown is not live (spec section 5).
const goldenRun: ScoreBreakdown = {
	finalScore: 15,
	isDisqualified: false,
	events: [
		{ type: 'match', index: 0, outcome: 'Win', points: 3, runningScore: 3 },
		{ type: 'match', index: 1, outcome: 'Win', points: 3, runningScore: 6 },
		{ type: 'match', index: 2, outcome: 'Win', points: 3, runningScore: 9 },
		{ type: 'streakBonus', afterMatchIndex: 2, points: 5, runningScore: 14 },
		{ type: 'match', index: 3, outcome: 'Draw', points: 1, runningScore: 15 }
	]
};

const clampedRun: ScoreBreakdown = {
	finalScore: 0,
	isDisqualified: false,
	events: [
		{ type: 'match', index: 0, outcome: 'Win', points: 3, runningScore: 3 },
		{ type: 'match', index: 1, outcome: 'Loss', points: 0, runningScore: 3 },
		{ type: 'match', index: 2, outcome: 'Draw', points: 1, runningScore: 4 },
		{ type: 'penalty', points: -6, runningScore: -2 },
		{ type: 'clampToZero', runningScore: 0 }
	]
};

const disqualifiedRun: ScoreBreakdown = {
	finalScore: 0,
	isDisqualified: true,
	events: [
		{ type: 'match', index: 0, outcome: 'Win', points: 3, runningScore: 3 },
		{ type: 'match', index: 1, outcome: 'Draw', points: 1, runningScore: 4 },
		{ type: 'match', index: 2, outcome: 'Win', points: 3, runningScore: 7 },
		{ type: 'disqualification', runningScore: 0 }
	]
};

export const breakdownFixtures: readonly ScoreBreakdown[] = [
	goldenRun,
	clampedRun,
	disqualifiedRun
];

// Stable pick: the same player keeps the same demo story across visits.
export function demoBreakdown(playerId: number): ScoreBreakdown {
	return breakdownFixtures[Math.abs(playerId) % breakdownFixtures.length];
}
