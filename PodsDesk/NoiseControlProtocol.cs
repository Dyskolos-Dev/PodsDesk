namespace PodsDesk;

/// <summary>
/// Encodes the community-documented AACP listening-mode command. This class
/// does not provide a Bluetooth connection; no packets are sent by PodsDesk yet.
/// </summary>
internal static class NoiseControlProtocol
{
    internal enum Mode : byte
    {
        Off = 1,
        NoiseCancellation = 2,
        Transparency = 3,
    }

    internal static byte[] CreateCommand(Mode mode)
    {
        if (mode is not (Mode.Off or Mode.NoiseCancellation or Mode.Transparency))
            throw new ArgumentOutOfRangeException(nameof(mode));

        return [0x04, 0x00, 0x04, 0x00, 0x09, 0x00, 0x0D, (byte)mode, 0x00, 0x00, 0x00];
    }
}
