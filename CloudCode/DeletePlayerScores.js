// Cloud Code script "DeletePlayerScores": deletes the calling player's scores from all live leaderboards.
// AccountService calls it before deleting the account. Only the Leaderboards Admin API can delete scores, so it
// authenticates with a service account (Leaderboards Admin role) whose key is in Secret Manager.
// Kept here for version control; paste it into the Unity Dashboard (Cloud Code → Scripts) and deploy it to each
// environment.
const axios = require("axios-0.21");

module.exports = async ({ context, logger, secretManager }) => {
  // The player ID comes from the caller's session, so a player can only delete their own scores
  if (!context.playerId) {
    throw new Error("DeletePlayerScores must be called by a signed-in player");
  }

  const keyId = await secretManager.getSecret("LEADERBOARDS_SA_KEY_ID");
  const secret = await secretManager.getSecret("LEADERBOARDS_SA_SECRET");

  const url = `https://services.api.unity.com/leaderboards/v1/projects/${context.projectId}` +
    `/environments/${context.environmentId}/leaderboards/scores/players/${context.playerId}/purge`;

  try {
    await axios.delete(url, { auth: { username: keyId.value, password: secret.value } });
  } catch (error) {
    const status = error.response ? error.response.status : undefined;

    // No scores to delete
    if (status === 404) {
      return;
    }

    // Log only the status: the error object holds the request, including the key
    logger.error(`Deleting leaderboard scores failed with status ${status}`);
    throw new Error(`Deleting leaderboard scores failed (${status})`);
  }
};
