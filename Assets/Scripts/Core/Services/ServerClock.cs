using System;

// The server's clock as seen from this device. Research is timed by the server (docs/cloud-save.md), so its
// timers count against this, not the device clock, which is often off by seconds or more. Every server answer
// (ProgressResponse.ServerTime) updates the offset; until the first one it's the device clock.
public static class ServerClock
{
    private static long offsetSeconds;

    // Unix seconds on the server's clock
    public static long Now => DeviceNow() + offsetSeconds;

    public static long OffsetSeconds => offsetSeconds;

    public static void Sync(long serverTime) => Sync(serverTime, DeviceNow());

    public static void Sync(long serverTime, long deviceTime)
    {
        // 0: an answer without a time
        if (serverTime > 0)
        {
            offsetSeconds = serverTime - deviceTime;
        }
    }

    private static long DeviceNow() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
