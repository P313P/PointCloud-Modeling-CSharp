namespace PointCloudModeling.Preprocessing;

using Core;

/// <summary>
/// Statistical filter for point cloud denoising
/// Uses spatial hashing for fast radius neighbor search
/// </summary>
public class StatisticalFilter
{
    public double SearchRadius { get; set; } = 2.0;
    public int MinNeighbors { get; set; } = 10;

    public List<LasPoint> Filter(List<LasPoint> points)
    {
        if (points.Count == 0) return points;

        Console.WriteLine($"[Denoising] Statistical filter: radius={SearchRadius}m, minNeighbors={MinNeighbors}");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var spatialGrid = BuildSpatialGrid(points, SearchRadius);
        var result = new List<LasPoint>(points.Count);
        int noiseCount = 0;

        for (int i = 0; i < points.Count; i++)
        {
            if (CountNeighbors(spatialGrid, points[i], SearchRadius, points) >= MinNeighbors)
                result.Add(points[i]);
            else
                noiseCount++;

            if (i % 100000 == 0 && i > 0)
                Console.WriteLine($"[Denoising] Processed {i:N0}/{points.Count:N0}");
        }

        sw.Stop();
        Console.WriteLine($"[Denoising] Done: {points.Count:N0} -> {result.Count:N0} (removed {noiseCount:N0}, {noiseCount*100.0/points.Count:F1}%), {sw.Elapsed.TotalSeconds:F1}s");
        return result;
    }

    private Dictionary<(int, int, int), List<int>> BuildSpatialGrid(List<LasPoint> points, double cellSize)
    {
        var grid = new Dictionary<(int, int, int), List<int>>();
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            var key = ((int)(p.X / cellSize), (int)(p.Y / cellSize), (int)(p.Z / cellSize));
            if (!grid.TryGetValue(key, out var list))
            {
                list = new List<int>();
                grid[key] = list;
            }
            list.Add(i);
        }
        return grid;
    }

    private int CountNeighbors(Dictionary<(int, int, int), List<int>> grid, LasPoint point, double radius, List<LasPoint> allPoints)
    {
        double r2 = radius * radius;
        int cx = (int)(point.X / radius);
        int cy = (int)(point.Y / radius);
        int cz = (int)(point.Z / radius);
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                    if (grid.TryGetValue((cx + dx, cy + dy, cz + dz), out var indices))
                        foreach (int idx in indices)
                        {
                            var o = allPoints[idx];
                            double ddx = o.X - point.X, ddy = o.Y - point.Y, ddz = o.Z - point.Z;
                            if (ddx * ddx + ddy * ddy + ddz * ddz <= r2)
                            {
                                count++;
                                if (count >= MinNeighbors) return count;
                            }
                        }
        return count;
    }
}
