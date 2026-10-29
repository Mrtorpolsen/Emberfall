using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ProfanityFilterTests
{
    private ProfanityFilter filter;

    [OneTimeSetUp]
    public void LoadWordList()
    {
        var words = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/ProfanityWords.txt");
        Assert.IsNotNull(words, "Assets/Data/ProfanityWords.txt is missing");
        filter = new ProfanityFilter(words.text);
    }

    [TestCase("fuck")]
    [TestCase("FuCk")]
    [TestCase("BigAss")]
    [TestCase("xXbitchXx")]
    public void IsAllowed_ListedWordAnywhere_Blocked(string name)
    {
        Assert.IsFalse(filter.IsAllowed(name));
    }

    [TestCase("sh1t")]
    [TestCase("5hit")]
    [TestCase("Pu$$y")]
    [TestCase("c0ck")]
    public void IsAllowed_LookAlikeCharacters_Blocked(string name)
    {
        Assert.IsFalse(filter.IsAllowed(name));
    }

    [Test]
    public void IsAllowed_RepeatedLetters_Blocked()
    {
        Assert.IsFalse(filter.IsAllowed("fuuuuck"));
    }

    [TestCase("Assassin")]
    [TestCase("Titan")]
    [TestCase("Classic")]
    [TestCase("Scunthorpe")]
    [TestCase("Raccoon")]
    [TestCase("Ti7an")]
    public void IsAllowed_AllowlistedWord_Allowed(string name)
    {
        Assert.IsTrue(filter.IsAllowed(name));
    }

    [TestCase("Torp")]
    [TestCase("xXSlayerXx")]
    [TestCase("Smash")]
    [TestCase("EmberKnight")]
    public void IsAllowed_OrdinaryName_Allowed(string name)
    {
        Assert.IsTrue(filter.IsAllowed(name));
    }

    [Test]
    public void IsAllowed_BlockedWordNextToAllowlistedWord_Blocked()
    {
        Assert.IsFalse(filter.IsAllowed("fuckspoon"));
    }
}
