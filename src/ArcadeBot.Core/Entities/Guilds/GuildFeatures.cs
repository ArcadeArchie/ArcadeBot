using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ArcadeBot.Core.Entities.Guilds
{
    public class GuildFeatures
    {
        public GuildFeature Value { get; }
        public IReadOnlyCollection<string> Experimental { get; }


        internal GuildFeatures(GuildFeature value, string[] experimental)
        {
            Value = value;
            Experimental = experimental.ToImmutableArray();
        }

        public bool HasFeature(GuildFeature feature) => Value.HasFlag(feature);
        public bool HasFeature(string feature) => Experimental.Contains(feature);
        internal void EnsureFeature(GuildFeature feature)
        {
            if (HasFeature(feature))
                return;
            var missingFeatures = Enum.GetValues<GuildFeature>();//.Cast<GuildFeature>();
            throw new InvalidOperationException($"Missing Features: {(missingFeatures.Length > 1 ? "s" : "")} {string.Join(", ", missingFeatures)}, are required for this operation");
        }
    }
}