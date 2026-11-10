using EmberfallModule;
using Newtonsoft.Json.Linq;

namespace TestProject;

// Runs on the server's .NET and Newtonsoft, unlike the Unity EditMode tests: the embedded JSON must parse here too
public class DefinitionsStoreTests
{
    [Test]
    public void TalentsParseAndBuild()
    {
        Assert.That(DefinitionsStore.Talents.TalentsByUnit, Is.Not.Empty);
    }

    [Test]
    public void ResearchParsesAndScales()
    {
        Assert.That(DefinitionsStore.Research.ResearchByCategoryEnum, Is.Not.Empty);
    }

    [Test]
    public void CombinedJsonHoldsBothFilesWithTheirVersions()
    {
        var combined = JObject.Parse(DefinitionsStore.CombinedJson);

        Assert.That(combined["talents"]?["definitionsVersion"]?.Value<int>(), Is.GreaterThan(0));
        Assert.That(combined["research"]?["definitionsVersion"]?.Value<int>(), Is.GreaterThan(0));
    }

    [Test]
    public void SharedTalentRulesWorkOnTheServer()
    {
        var tree = DefinitionsStore.Talents;
        string unit = tree.TalentsByUnit.Keys.First();
        Talent talent = tree.TalentsByUnit[unit].First(t => TalentRules.IsPurchasableType(t.Type));

        var check = TalentRules.CanPurchase(tree, new PlayerTalentState(), unit, talent.Id, cinders: 0);

        Assert.That(check, Is.AnyOf(TalentPurchaseCheck.NotEnoughCurrency, TalentPurchaseCheck.PrerequisitesNotMet));
    }
}
