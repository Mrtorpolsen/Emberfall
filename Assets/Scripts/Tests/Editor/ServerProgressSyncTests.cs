using Newtonsoft.Json;
using NUnit.Framework;

public class ServerProgressSyncTests
{
    [Test]
    public void ParseFillsMissingSections()
    {
        ServerProgress progress = ServerProgressSync.Parse(@"{ ""Version"": 1, ""Currency"": { ""Cinders"": 12 }, ""Talents"": null }");

        Assert.AreEqual(12, progress.Currency.Cinders);
        Assert.IsNotNull(progress.Talents.Purchases);
        Assert.IsNotNull(progress.Talents.CurrencySpent);
        Assert.IsNotNull(progress.Research.CompletedResearch);
        Assert.IsNotNull(progress.Research.ActiveResearch);
    }

    [Test]
    public void ParseReadsWhatTheGameSends()
    {
        var sent = new ServerProgress { HasReceivedLoginGift = true };
        sent.Currency.Cinders = 345;
        sent.Talents.CurrencySpent[CurrencyTypes.Cinders] = 25;
        sent.Research.ActiveResearch.Add(new ActiveResearch(ResearchCategory.Tower, "tower_range_1", 2, "Tower Range", 1_700_000_000));

        ServerProgress back = ServerProgressSync.Parse(JsonConvert.SerializeObject(sent));

        Assert.AreEqual(345, back.Currency.Cinders);
        Assert.AreEqual(25, back.Talents.CurrencySpent[CurrencyTypes.Cinders]);
        Assert.AreEqual(ResearchCategory.Tower, back.Research.ActiveResearch[0].ResearchCategory);
        Assert.AreEqual(1_700_000_000, back.Research.ActiveResearch[0].StartTime);
        Assert.IsTrue(back.HasReceivedLoginGift);
    }

    [Test]
    public void EmptyServerProgressIsAnError()
    {
        Assert.Throws<JsonException>(() => ServerProgressSync.Parse("null"));
    }
}
