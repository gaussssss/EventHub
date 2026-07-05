/** Réglages de gamification (GET /api/admin/settings/gamification). */
export interface GamificationSettingsDto {
  bronzeThreshold: number;
  silverThreshold: number;
  goldThreshold: number;
  defaultAttendanceReward: number;
}

/** Payload PATCH (bronze est fixe à 0, non modifiable). */
export interface GamificationRequest {
  silverThreshold: number;
  goldThreshold: number;
  defaultAttendanceReward: number;
}
