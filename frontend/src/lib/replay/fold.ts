import type { ScoreEvent } from '$lib/api/types';

export interface ReplayFrame {
	score: number;
	delta: number | null;
	matchesPlayed: number;
	isDisqualified: boolean;
}

const initialFrame: ReplayFrame = {
	score: 0,
	delta: null,
	matchesPlayed: 0,
	isDisqualified: false
};

// Display fold over the runningScore values the API provides; scoring rules stay backend only.
export function foldEvents(events: readonly ScoreEvent[], cursor: number): ReplayFrame {
	return events.slice(0, cursor + 1).reduce<ReplayFrame>(
		(frame, event) => ({
			score: event.runningScore,
			delta: 'points' in event ? event.points : null,
			matchesPlayed: frame.matchesPlayed + (event.type === 'match' ? 1 : 0),
			isDisqualified: frame.isDisqualified || event.type === 'disqualification'
		}),
		initialFrame
	);
}
