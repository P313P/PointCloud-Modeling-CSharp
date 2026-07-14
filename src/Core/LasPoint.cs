namespace PointCloudModeling.Core;

public struct LasPoint
{
    public int XRaw { get; set; }
    public int YRaw { get; set; }
    public int ZRaw { get; set; }

    public double X => XRaw * ScaleX + OffsetX;
    public double Y => YRaw * ScaleY + OffsetY;
    public double Z => ZRaw * ScaleZ + OffsetZ;

    public double ScaleX { get; set; }
    public double ScaleY { get; set; }
    public double ScaleZ { get; set; }
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public double OffsetZ { get; set; }

    public ushort Intensity { get; set; }
    public byte ReturnInfo { get; set; }
    public byte Classification { get; set; }
    public sbyte ScanAngleRank { get; set; }
    public byte UserData { get; set; }
    public ushort PointSourceId { get; set; }
    public double GpsTime { get; set; }
    public ushort Red { get; set; }
    public ushort Green { get; set; }
    public ushort Blue { get; set; }

    public byte ReturnNumber => (byte)(ReturnInfo & 0x07);
    public byte NumberOfReturns => (byte)((ReturnInfo >> 3) & 0x07);
    public byte ClassValue => (byte)(Classification & 0x0F);

    public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3}) [Class: {ClassValue}]";
}
