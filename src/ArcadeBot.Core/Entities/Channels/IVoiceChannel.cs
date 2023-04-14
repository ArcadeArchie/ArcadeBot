using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeBot.Core.Entities.Channels;

public interface IVoiceChannel : ITextChannel, IAudioChannel
{
    int Bitrate { get; }
    int? UserLimit { get; }
    VideoQualityMode VideoQualityMode { get; }
}
