// Mirror of the backend DTOs (Escrime.Api/Dtos): camelCase JSON, enums as strings.
export type MatchOutcome = 'Win' | 'Draw' | 'Loss';

export interface Player {
	id: number;
	name: string;
	score: number;
	isDisqualified: boolean;
	penaltyPoints: number;
	matchesPlayed: number;
}

export interface Match {
	id: number;
	result: MatchOutcome;
}

export interface PlayerDetail {
	id: number;
	name: string;
	score: number;
	isDisqualified: boolean;
	penaltyPoints: number;
	matches: Match[];
}

export interface RankingEntry {
	rank: number;
	playerId: number;
	name: string;
	score: number;
}

// Spec section 5: replay contract of GET /api/players/{id}/score-breakdown.
export type ScoreEvent =
	| { type: 'match'; index: number; outcome: MatchOutcome; points: number; runningScore: number }
	| { type: 'streakBonus'; afterMatchIndex: number; points: number; runningScore: number }
	| { type: 'penalty'; points: number; runningScore: number }
	| { type: 'clampToZero'; runningScore: number }
	| { type: 'disqualification'; runningScore: number };

export interface ScoreBreakdown {
	finalScore: number;
	isDisqualified: boolean;
	events: ScoreEvent[];
}

// demo marks a breakdown served from the local fixture while the endpoint is not live.
export type ReplayBreakdown = ScoreBreakdown & { demo: boolean };
