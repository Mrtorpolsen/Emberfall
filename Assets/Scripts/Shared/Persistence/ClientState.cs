// Player state the game writes itself (docs/cloud-save.md): nothing here is worth cheating, so it doesn't
// need the server's checks
public class ClientState
{
    public PlayerLoadoutCollection Loadouts = new();

    public bool HasCompletedTutorial;
}
