namespace PointCloudModeling.Core;

public class LasReader : IDisposable
{
    private FileStream? _fileStream;
    private BinaryReader? _reader;
    private bool _disposed;

    public LasHeader Header { get; private set; } = new();
    public long PointDataStartPosition => Header.OffsetToPointData;

    public void Open(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"LAS file not found: {filePath}");

        _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        _reader = new BinaryReader(_fileStream);
        ReadHeader();
        Console.WriteLine($"[LAS] {Header}");
        if (!Header.ValidateBounds())
            Console.WriteLine("[Warning] Invalid bounds in header");
    }

    private void ReadHeader()
    {
        if (_reader == null) throw new InvalidOperationException("File not open");

        var sigBytes = _reader.ReadBytes(4);
        Header.FileSignature = System.Text.Encoding.ASCII.GetString(sigBytes);
        if (Header.FileSignature != "LASF")
            throw new InvalidDataException($"Invalid LAS signature: {Header.FileSignature}");

        Header.FileSourceId = _reader.ReadUInt16();
        Header.GlobalEncoding = _reader.ReadUInt16();
        Header.ProjectIdGuidData1 = _reader.ReadBytes(16);
        Header.VersionMajor = _reader.ReadByte();
        Header.VersionMinor = _reader.ReadByte();

        var sysIdBytes = _reader.ReadBytes(32);
        Header.SystemIdentifier = System.Text.Encoding.ASCII.GetString(sysIdBytes).TrimEnd('\0');

        var genSwBytes = _reader.ReadBytes(32);
        Header.GeneratingSoftware = System.Text.Encoding.ASCII.GetString(genSwBytes).TrimEnd('\0');

        Header.CreationDay = _reader.ReadUInt16();
        Header.CreationYear = _reader.ReadUInt16();
        Header.HeaderSize = _reader.ReadUInt16();
        Header.OffsetToPointData = _reader.ReadUInt32();
        Header.NumberOfVariableLengthRecords = _reader.ReadUInt32();
        Header.PointDataRecordFormat = _reader.ReadByte();
        Header.PointDataRecordLength = _reader.ReadUInt16();
        Header.LegacyNumberOfPointRecords = _reader.ReadUInt32();

        for (int i = 0; i < 5; i++)
            Header.LegacyNumberOfPointsByReturn[i] = _reader.ReadUInt32();

        Header.XScaleFactor = _reader.ReadDouble();
        Header.YScaleFactor = _reader.ReadDouble();
        Header.ZScaleFactor = _reader.ReadDouble();
        Header.XOffset = _reader.ReadDouble();
        Header.YOffset = _reader.ReadDouble();
        Header.ZOffset = _reader.ReadDouble();
        Header.MaxX = _reader.ReadDouble();
        Header.MinX = _reader.ReadDouble();
        Header.MaxY = _reader.ReadDouble();
        Header.MinY = _reader.ReadDouble();
        Header.MaxZ = _reader.ReadDouble();
        Header.MinZ = _reader.ReadDouble();

        if ((Header.VersionMajor > 1 || (Header.VersionMajor == 1 && Header.VersionMinor >= 4)) && Header.HeaderSize >= 375)
        {
            _reader.ReadBytes(148);
            Header.NumberOfPointRecords = _reader.ReadUInt64();
            for (int i = 0; i < 15; i++)
                Header.NumberOfPointsByReturn[i] = _reader.ReadUInt64();
        }

        _fileStream!.Seek(Header.OffsetToPointData, SeekOrigin.Begin);
    }

    public List<LasPoint> ReadAllPoints(IProgress<double>? progress = null)
    {
        if (_reader == null) throw new InvalidOperationException("File not open");

        var totalPoints = Header.GetTotalPoints();
        var points = new List<LasPoint>((int)Math.Min(totalPoints, 10000000));

        Console.WriteLine($"[Read] Loading {totalPoints:N0} points...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (ulong i = 0; i < totalPoints; i++)
        {
            points.Add(ReadSinglePoint());
            if (i % 50000 == 0) progress?.Report((double)i / totalPoints * 100);
        }

        sw.Stop();
        Console.WriteLine($"[Read] Loaded {points.Count:N0} points in {sw.Elapsed.TotalSeconds:F1}s");
        return points;
    }

    private LasPoint ReadSinglePoint()
    {
        if (_reader == null) throw new InvalidOperationException();

        var point = new LasPoint
        {
            ScaleX = Header.XScaleFactor, ScaleY = Header.YScaleFactor, ScaleZ = Header.ZScaleFactor,
            OffsetX = Header.XOffset, OffsetY = Header.YOffset, OffsetZ = Header.ZOffset
        };

        switch (Header.PointDataRecordFormat)
        {
            case 0: ReadFormat0(point); break;
            case 1: ReadFormat1(point); break;
            case 2: ReadFormat2(point); break;
            case 3: ReadFormat3(point); break;
            default: throw new NotSupportedException($"Format {Header.PointDataRecordFormat} not supported");
        }
        return point;
    }

    private void ReadFormat0(LasPoint point)
    {
        point.XRaw = _reader!.ReadInt32();
        point.YRaw = _reader.ReadInt32();
        point.ZRaw = _reader.ReadInt32();
        point.Intensity = _reader.ReadUInt16();
        point.ReturnInfo = _reader.ReadByte();
        point.Classification = _reader.ReadByte();
        point.ScanAngleRank = _reader.ReadSByte();
        point.UserData = _reader.ReadByte();
        point.PointSourceId = _reader.ReadUInt16();
    }

    private void ReadFormat1(LasPoint point)
    {
        ReadFormat0(point);
        point.GpsTime = _reader!.ReadDouble();
    }

    private void ReadFormat2(LasPoint point)
    {
        ReadFormat0(point);
        point.Red = _reader!.ReadUInt16();
        point.Green = _reader.ReadUInt16();
        point.Blue = _reader.ReadUInt16();
    }

    private void ReadFormat3(LasPoint point)
    {
        ReadFormat0(point);
        point.GpsTime = _reader!.ReadDouble();
        point.Red = _reader.ReadUInt16();
        point.Green = _reader.ReadUInt16();
        point.Blue = _reader.ReadUInt16();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _reader?.Dispose();
            _fileStream?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
