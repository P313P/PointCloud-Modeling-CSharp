namespace PointCloudModeling.Preprocessing;

using Core;

/// <summary>
/// Voxel Grid Downsampling - reduces point density while preserving terrain features
/// </summary>
public class VoxelGridDownsampler
{
    public double VoxelSize { get; set; } = 0.5;

    public List<LasPoint> Downsample(List<LasPoint> points)
    {
        if (points.Count == 0) return points;

        Console.WriteLine($"[Downsampling] Voxel grid: cell={VoxelSize}m");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var voxelDict = new Dictionary<(int, int, int), List<LasPoint>>();
        foreach (var point in points)
        {
            var key = ((int)(point.X / VoxelSize), (int)(point.Y / VoxelSize), (int)(point.Z / VoxelSize));
            if (!voxelDict.TryGetValue(key, out var list))
            {
                list = new List<LasPoint>();
                voxelDict[key] = list;
            }
            list.Add(point);
        }

        var result = new List<LasPoint>(voxelDict.Count);
        foreach (var voxelPoints in voxelDict.Values)
        {
            if (voxelPoints.Count == 0) continue;

            double avgX = 0, avgY = 0, avgZ = 0;
            foreach (var p in voxelPoints)
            {
                avgX += p.X; avgY += p.Y; avgZ += p.Z;
            }
            avgX /= voxelPoints.Count;
            avgY /= voxelPoints.Count;
            avgZ /= voxelPoints.Count;

            var rep = voxelPoints[0];
            rep.XRaw = (int)Math.Round((avgX - rep.OffsetX) / rep.ScaleX);
            rep.YRaw = (int)Math.Round((avgY - rep.OffsetY) / rep.ScaleY);
            rep.ZRaw = (int)Math.Round((avgZ - rep.OffsetZ) / rep.ScaleZ);
            result.Add(rep);
        }

        sw.Stop();
        Console.WriteLine($"[Downsampling] Done: {points.Count:N0} -> {result.Count:N0} ({result.Count*100.0/points.Count:F1}%), {sw.Elapsed.TotalSeconds:F1}s");
        return result;
    }
}
