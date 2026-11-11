using NUnit.Framework;

public class ServicesEnvironmentTests
{
    [Test]
    public void DebugBuildsUseDevelopment()
    {
        Assert.That(ServicesEnvironment.Select(isDebugBuild: true), Is.EqualTo("development"));
    }

    [Test]
    public void ReleaseBuildsUseProduction()
    {
        Assert.That(ServicesEnvironment.Select(isDebugBuild: false), Is.EqualTo("production"));
    }
}
