namespace ArcadeBot.Core.Entities.Channels
{
    public interface IAudioChannel : IChannel
    {
        string RTCRegion { get; }
    }
}