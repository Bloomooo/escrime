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
