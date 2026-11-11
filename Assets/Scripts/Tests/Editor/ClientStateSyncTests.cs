using NUnit.Framework;

public class ClientStateSyncTests
{
    private const string Server = @"{""HasCompletedTutorial"":true}";
    private const string Local = @"{""HasCompletedTutorial"":false}";

    [Test]
    public void FreshInstallTakesTheServersCopy()
    {
        Assert.IsTrue(ClientStateSync.ShouldUseServer(Server, Local, lastUploadedJson: null));
    }

    [Test]
    public void UnchangedLocalCopyTakesTheServersCopy()
    {
        Assert.IsTrue(ClientStateSync.ShouldUseServer(Server, Local, lastUploadedJson: Local));
    }

    [Test]
    public void LocalChangesSinceTheLastUploadWin()
    {
        Assert.IsFalse(ClientStateSync.ShouldUseServer(Server, Local, lastUploadedJson: @"{""HasCompletedTutorial"":true,""x"":1}"));
    }

    [TestCase(null)]
    [TestCase("")]
    public void NoServerCopyKeepsTheLocalOne(string server)
    {
        Assert.IsFalse(ClientStateSync.ShouldUseServer(server, Local, lastUploadedJson: null));
    }

    [Test]
    public void ParseFillsMissingLoadouts()
    {
        ClientState state = ClientStateSync.Parse(@"{""HasCompletedTutorial"":true,""Loadouts"":null}");

        Assert.IsTrue(state.HasCompletedTutorial);
        Assert.IsNotNull(state.Loadouts);
    }
}
