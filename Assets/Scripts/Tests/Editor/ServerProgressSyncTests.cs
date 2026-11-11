using Newtonsoft.Json;
using NUnit.Framework;

public class ServerProgressSyncTests
{
    [Test]
    public void ParseFillsMissingSections()
    {
        ProgressResponse response = ServerProgressSync.ParseResponse(
            @"{ ""Progress"": { ""Version"": 1, ""Currency"": { ""Cinders"": 12 }, ""Talents"": null }, ""ServerTime"": 1700000000 }");

        ServerProgress progress = response.Progress;
        Assert.AreEqual(12, progress.Currency.Cinders);
        Assert.IsNotNull(progress.Talents.Purchases);
        Assert.IsNotNull(progress.Talents.CurrencySpent);
        Assert.IsNotNull(progress.Research.CompletedResearch);
        Assert.IsNotNull(progress.Research.ActiveResearch);
        Assert.AreEqual(1_700_000_000, response.ServerTime);
    }

    [Test]
    public void ParseReadsWhatTheServerSends()
    {
        var sent = new ServerProgress { HasReceivedLoginGift = true };
        sent.Currency.Cinders = 345;
        sent.Talents.CurrencySpent[CurrencyTypes.Cinders] = 25;
        sent.Research.ActiveResearch.Add(new ActiveResearch(ResearchCategory.Tower, "tower_range_1", 2, "Tower Range", 1_700_000_000));
        string json = JsonConvert.SerializeObject(new ProgressResponse { Progress = sent, ServerTime = 1_700_000_100 });

        ServerProgress back = ServerProgressSync.ParseResponse(json).Progress;

        Assert.AreEqual(345, back.Currency.Cinders);
        Assert.AreEqual(25, back.Talents.CurrencySpent[CurrencyTypes.Cinders]);
        Assert.AreEqual(ResearchCategory.Tower, back.Research.ActiveResearch[0].ResearchCategory);
        Assert.AreEqual(1_700_000_000, back.Research.ActiveResearch[0].StartTime);
        Assert.IsTrue(back.HasReceivedLoginGift);
    }

    [Test]
    public void APlayerWithoutServerProgressHasNullProgress()
    {
        ProgressResponse response = ServerProgressSync.ParseResponse(@"{ ""Error"": null, ""Progress"": null, ""ServerTime"": 1700000000 }");

        Assert.IsNull(response.Progress);
        Assert.IsNull(response.Error);
    }

    [Test]
    public void AnEmptyAnswerIsAnError()
    {
        Assert.Throws<JsonException>(() => ServerProgressSync.ParseResponse("null"));
    }
}
