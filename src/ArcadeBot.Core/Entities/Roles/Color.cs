using SysColor = System.Drawing.Color;

namespace ArcadeBot.Core.Entities.Roles
{
    public struct Color
    {
        public uint RawValue { get; }
        public byte R => (byte)(RawValue >> 16);
        public byte G => (byte)(RawValue >> 8);
        public byte B => (byte)RawValue;

    }
}