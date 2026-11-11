using EmberfallModule;
using Newtonsoft.Json;

namespace TestProject;

// Uses the real embedded definitions, so it also checks the import against the shipped data
public class ProgressImportTests
{
    const long Now = 1_800_000_000;

    static TalentTree Talents => DefinitionsStore.Talents;
    static ResearchTree Research => DefinitionsStore.Research;

    static (string Unit, Talent Talent) PurchasableTalent()
    {
        foreach (var (unit, talents) in Talents.TalentsByUnit)
        {
            var talent = talents.FirstOrDefault(t => t.Type == TalentType.StatModifier && t.Purchase.MaxPurchases > 1);
            if (talent != null)
                return (unit, talent);
        }

        throw new InvalidOperationException("No stat talent with several levels in Talents.json");
    }

    static ResearchDefinition AnyResearch() => Research.ResearchByCategoryEnum.Values.First(list => list.Count > 0).First();

    static ServerProgress Import(ServerProgress? local) => ProgressImport.Sanitize(local, Talents, Research, Now);

    static ServerProgress WithTalent(string unit, string talentId, int levels)
    {
        var local = new ServerProgress();
        local.Talents.Purchases[unit] = new UnitSaveData { PurchasedTalents = new Dictionary<string, int> { [talentId] = levels } };
        return local;
    }

    [Test]
    public void NoLocalSaveStartsFresh()
    {
        var progress = Import(null);

        Assert.That(progress.Currency.Cinders, Is.Zero);
        Assert.That(progress.Talents.Purchases, Is.Empty);
        Assert.That(progress.HasReceivedLoginGift, Is.False);
        Assert.That(progress.Version, Is.EqualTo(ServerProgress.CurrentVersion));
    }

    [TestCase(1234, 1234)]
    [TestCase(-50, 0)]
    [TestCase(9_999_999, ProgressImport.MaxCinders)]
    public void CindersAreLimited(int local, int expected)
    {
        var save = new ServerProgress();
        save.Currency.Cinders = local;

        Assert.That(Import(save).Currency.Cinders, Is.EqualTo(expected));
    }

    [Test]
    public void EmbersAreLimited()
    {
        var save = new ServerProgress();
        save.Currency.Embers = 1_000_000;

        Assert.That(Import(save).Currency.Embers, Is.EqualTo(ProgressImport.MaxEmbers));
    }

    [Test]
    public void LoginGiftFlagIsKept()
    {
        Assert.That(Import(new ServerProgress { HasReceivedLoginGift = true }).HasReceivedLoginGift, Is.True);
    }

    [Test]
    public void TalentLevelsAreKeptUpToMaxAndSpentIsRecomputed()
    {
        var (unit, talent) = PurchasableTalent();
        int max = talent.Purchase.MaxPurchases;
        var local = WithTalent(unit, talent.Id, max + 10);
        local.Talents.CurrencySpent[CurrencyTypes.Cinders] = 999_999;

        var progress = Import(local);

        Assert.That(TalentRules.GetOwned(progress.Talents, unit, talent.Id), Is.EqualTo(max));
        Assert.That(progress.Talents.CurrencySpent[CurrencyTypes.Cinders], Is.EqualTo(max * talent.GetCurrentCost()));
    }

    [Test]
    public void UnknownTalentsAndUnitsAreDropped()
    {
        var (unit, _) = PurchasableTalent();
        var local = WithTalent(unit, "no_such_talent_T1", 3);
        local.Talents.Purchases["no_such_unit"] = new UnitSaveData { PurchasedTalents = new Dictionary<string, int> { ["x"] = 1 } };

        var progress = Import(local);

        Assert.That(progress.Talents.Purchases, Is.Empty);
        Assert.That(progress.Talents.CurrencySpent, Is.Empty);
    }

    [Test]
    public void TalentsThatArentSoldAreDropped()
    {
        var (unit, talent) = Talents.TalentsByUnit
            .SelectMany(u => u.Value.Select(t => (u.Key, t)))
            .FirstOrDefault(x => !TalentRules.IsPurchasableType(x.t.Type));

        if (talent == null)
            Assert.Ignore("Talents.json has no talent that isn't for sale");

        Assert.That(Import(WithTalent(unit, talent.Id, 1)).Talents.Purchases, Is.Empty);
    }

    [Test]
    public void ResearchLevelsAreLimitedAndUnknownResearchDropped()
    {
        var research = AnyResearch();
        var local = new ServerProgress();
        local.Research.CompletedResearch[research.Id] = research.MaxLevel + 5;
        local.Research.CompletedResearch["no_such_research"] = 3;

        var progress = Import(local);

        Assert.That(progress.Research.CompletedResearch, Is.EqualTo(new Dictionary<string, int> { [research.Id] = research.MaxLevel }));
    }

    [Test]
    public void ActiveResearchForTheNextLevelIsKept()
    {
        var research = AnyResearch();
        var local = new ServerProgress();
        local.Research.CompletedResearch[research.Id] = 1;
        local.Research.ActiveResearch.Add(new ActiveResearch(research.Category, research.Id, 2, "renamed", Now - 60));

        var active = Import(local).Research.ActiveResearch.Single();

        Assert.That(active.TargetLevel, Is.EqualTo(2));
        Assert.That(active.StartTime, Is.EqualTo(Now - 60));
        Assert.That(active.ResearchName, Is.EqualTo(research.Name));
    }

    [TestCase(3, Now - 60)]     // skips a level
    [TestCase(2, Now + 3600)]   // starts in the future
    public void InvalidActiveResearchIsDropped(int targetLevel, long startTime)
    {
        var research = AnyResearch();
        var local = new ServerProgress();
        local.Research.CompletedResearch[research.Id] = 1;
        local.Research.ActiveResearch.Add(new ActiveResearch(research.Category, research.Id, targetLevel, research.Name, startTime));

        Assert.That(Import(local).Research.ActiveResearch, Is.Empty);
    }

    [Test]
    public void OnlyOneActiveResearchPerCategory()
    {
        var research = AnyResearch();
        var local = new ServerProgress();
        local.Research.ActiveResearch.Add(new ActiveResearch(research.Category, research.Id, 1, research.Name, Now));
        local.Research.ActiveResearch.Add(new ActiveResearch(research.Category, research.Id, 1, research.Name, Now));

        Assert.That(Import(local).Research.ActiveResearch, Has.Count.EqualTo(1));
    }

    [Test]
    public void ProgressSurvivesTheServersJson()
    {
        var (unit, talent) = PurchasableTalent();
        var progress = Import(WithTalent(unit, talent.Id, 1));

        var back = JsonConvert.DeserializeObject<ServerProgress>(ProgressStore.ToJson(progress))!;

        Assert.That(TalentRules.GetOwned(back.Talents, unit, talent.Id), Is.EqualTo(1));
        Assert.That(back.Talents.CurrencySpent[CurrencyTypes.Cinders], Is.EqualTo(talent.GetCurrentCost()));
    }
}
