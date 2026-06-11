import { env } from '$env/dynamic/public';
import { createApiClient } from './client';

export const api = createApiClient({ baseUrl: env.PUBLIC_API_BASE || 'http://localhost:5000' });

export { ApiError, createApiClient, type ApiClient, type ApiClientOptions } from './client';
export type { Match, MatchOutcome, Player, PlayerDetail, RankingEntry } from './types';
