/** Une ligne du classement (GET /api/leaderboard). */
export interface LeaderboardRow {
  rank: number;
  name?: string | null;
  avatarUrl?: string | null;
  hearts: number;
  isMe: boolean;
}
