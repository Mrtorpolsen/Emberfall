using NUnit.Framework;

public class TutorialFlowTests
{
    [Test]
    public void NewPlayerIsNotPastTheResearchStep()
    {
        Assert.IsFalse(TutorialFlow.IsPastResearchStep(new PlayerResearchState()));
    }

    [Test]
    public void RunningResearchMeansPastTheResearchStep()
    {
        var research = new PlayerResearchState();
        research.ActiveResearch.Add(new ActiveResearch(ResearchCategory.Unit, "unit_statsmodifier_health", 1, "Health", 1_700_000_000));

        Assert.IsTrue(TutorialFlow.IsPastResearchStep(research));
    }

    [Test]
    public void CompletedResearchMeansPastTheResearchStep()
    {
        var research = new PlayerResearchState();
        research.CompletedResearch["unit_statsmodifier_health"] = 1;

        Assert.IsTrue(TutorialFlow.IsPastResearchStep(research));
    }

    [Test]
    public void ResearchAtLevelZeroDoesNotCount()
    {
        var research = new PlayerResearchState();
        research.CompletedResearch["unit_statsmodifier_health"] = 0;

        Assert.IsFalse(TutorialFlow.IsPastResearchStep(research));
    }
}
