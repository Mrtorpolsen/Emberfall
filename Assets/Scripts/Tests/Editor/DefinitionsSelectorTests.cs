using NUnit.Framework;

public class DefinitionsSelectorTests
{
    private static string Json(int schema, int definitions) =>
        $@"{{ ""schemaVersion"": {schema}, ""definitionsVersion"": {definitions}, ""marker"": ""{schema}.{definitions}"" }}";

    private static readonly string Bundled = Json(2, 7);

    [Test]
    public void NewerServerDefinitionsWin()
    {
        string server = Json(2, 8);

        Assert.That(DefinitionsSelector.Choose(Bundled, server), Is.EqualTo(server));
    }

    [TestCase(7)]
    [TestCase(6)]
    public void SameOrOlderServerDefinitionsLose(int serverVersion)
    {
        Assert.That(DefinitionsSelector.Choose(Bundled, Json(2, serverVersion)), Is.EqualTo(Bundled));
    }

    [TestCase(1)]
    [TestCase(3)]
    public void ADifferentSchemaIsNeverUsed(int serverSchema)
    {
        Assert.That(DefinitionsSelector.Choose(Bundled, Json(serverSchema, 99)), Is.EqualTo(Bundled));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("{ not json")]
    public void MissingOrBrokenServerDefinitionsFallBack(string server)
    {
        Assert.That(DefinitionsSelector.Choose(Bundled, server), Is.EqualTo(Bundled));
    }
}
