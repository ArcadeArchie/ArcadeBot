using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities;

namespace ArcadeBot.Core.Utils
{
    public static class CDNUtils
    {
        public static string GetEmoteUri(ulong id, bool isAnimated) => $"{Constants.CDNUrl}emojis/{id}.{(isAnimated ? "gif" : "png")}";

        internal static string? GetGuildBannerUrl(ulong id, string? bannerId, ImageFormat format) =>
            string.IsNullOrEmpty(bannerId) ? null : $"{Constants.CDNUrl}banners/{id}/{bannerId}.{GetExtention(format, bannerId)}";

        internal static string? GetGuildIconUrl(ulong? id, string iconId) => id.HasValue ? $"{Constants.CDNUrl}icons/{id}/{iconId}.jpg" : null;

        internal static string? GetGuildSplashUrl(ulong? id, string splashId) => id.HasValue ? $"{Constants.CDNUrl}splashes/{id}/{splashId}.jpg" : null;
  
  
        private static string GetExtention(ImageFormat format, string imageId) => format switch
        {
            ImageFormat.Auto => imageId.StartsWith("a_") ? "gif" : "png",
            ImageFormat.Gif => "gif",
            ImageFormat.Jpeg => "jpeg",
            ImageFormat.Png => "png",
            ImageFormat.WebP => "webp",
            _ => throw new ArgumentException(nameof(format))
        };
    }
}