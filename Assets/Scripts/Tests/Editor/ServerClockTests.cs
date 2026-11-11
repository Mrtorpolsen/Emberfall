using NUnit.Framework;

public class ServerClockTests
{
    [TearDown]
    public void TearDown()
    {
        ServerClock.Sync(1, 1);   // back to no offset
    }

    [Test]
    public void ADeviceBehindTheServerGetsAPositiveOffset()
    {
        ServerClock.Sync(serverTime: 1_700_000_011, deviceTime: 1_700_000_000);

        Assert.AreEqual(11, ServerClock.OffsetSeconds);
    }

    [Test]
    public void ADeviceAheadOfTheServerGetsANegativeOffset()
    {
        ServerClock.Sync(serverTime: 1_700_000_000, deviceTime: 1_700_000_300);

        Assert.AreEqual(-300, ServerClock.OffsetSeconds);
    }

    [Test]
    public void AnAnswerWithoutATimeKeepsTheOffset()
    {
        ServerClock.Sync(serverTime: 1_700_000_011, deviceTime: 1_700_000_000);

        ServerClock.Sync(serverTime: 0, deviceTime: 1_700_000_500);

        Assert.AreEqual(11, ServerClock.OffsetSeconds);
    }
}
