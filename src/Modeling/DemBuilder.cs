namespace PointCloudModeling.Modeling;

using Core;

/// <summary>
/// DEM (Digital Elevation Model) builder from point cloud
/// Creates a regular grid raster using nearest-neighbor interpolation
/// </summary>
public class DemBuilder
{
    public double Resolution { get; set; } = 1.0;
    public double Margin { get; set; } = 5.0;
    public const double NoData = -9999.0;

    public double[,]? Grid { get; private set; }
    public (double x, double y) Origin { get; private set; }
    public (int cols, int rows) Dimensions { get; private set; }

    public double[,] BuildDem(List<LasPoint> points)
    {
        if (points.Count == 0) throw new ArgumentException("Empty point cloud");

        Console.WriteLine($"[DEM] Building raster: resolution={Resolution}m");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        double minX = points.Min(p => p.X) - Margin;
        double maxX = points.Max(p => p.X) + Margin;
        double minY = points.Min(p => p.Y) - Margin;
        double maxY = points.Max(p => p.Y) + Margin;

        int cols = (int)Math.Ceiling((maxX - minX) / Resolution);
        int rows = (int)Math.Ceiling((maxY - minY) / Resolution);

        Origin = (minX, minY);
        Dimensions = (cols, rows);
        Grid = new double[rows, cols];

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                Grid[r, c] = NoData;

        // Build spatial index
        var gridBuckets = new Dictionary<(int, int), List<LasPoint>>();
        foreach (var pt in points)
        {
            int gc = (int)((pt.X - minX) / Resolution);
            int gr = (int)((pt.Y - minY) / Resolution);
            gc = Math.Clamp(gc, 0, cols - 1);
            gr = Math.Clamp(gr, 0, rows - 1);
            var key = (gc, gr);
            if (!gridBuckets.TryGetValue(key, out var list))
            {
                list = new List<LasPoint>();
                gridBuckets[key] = list;
            }
            list.Add(pt);
        }

        // Assign elevation (average of lowest 20% per cell for ground estimation)
        foreach (var kvp in gridBuckets)
        {
            var (gc, gr) = kvp.Key;
            var sortedZ = kvp.Value.Select(p => p.Z).OrderBy(z => z).ToList();
            int groundCount = Math.Max(1, sortedZ.Count / 5);
            Grid[gr, gc] = sortedZ.Take(groundCount).Average();
        }

        FillNoDataValues();

        sw.Stop();
        Console.WriteLine($"[DEM] Grid: {cols}x{rows} cells, origin=({minX:F1},{minY:F1}), {sw.Elapsed.TotalSeconds:F1}s");
        return Grid;
    }

    private void FillNoDataValues()
    {
        if (Grid == null) return;
        int rows = Grid.GetLength(0), cols = Grid.GetLength(1);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (Grid[r, c] == NoData)
                {
                    double sumW = 0, sumV = 0;
                    for (int d = 1; d <= 10; d++)
                    {
                        bool found = false;
                        for (int dr = -d; dr <= d; dr++)
                            for (int dc = -d; dc <= d; dc++)
                            {
                                if (Math.Abs(dr) != d && Math.Abs(dc) != d) continue;
                                int nr = r + dr, nc = c + dc;
                                if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) continue;
                                if (Grid[nr, nc] == NoData) continue;
                                double w = 1.0 / (Math.Sqrt(dr * dr + dc * dc) + 0.1);
                                sumW += w; sumV += w * Grid[nr, nc]; found = true;
                            }
                        if (found) break;
                    }
                    if (sumW > 0) Grid[r, c] = sumV / sumW;
                }
    }

    public void ExportAsc(string filePath)
    {
        if (Grid == null) throw new InvalidOperationException("DEM not built yet");
        int rows = Grid.GetLength(0), cols = Grid.GetLength(1);
        using var writer = new StreamWriter(filePath);
        writer.WriteLine($"ncols {cols}");
        writer.WriteLine($"nrows {rows}");
        writer.WriteLine($"xllcorner {Origin.x:F2}");
        writer.WriteLine($"yllcorner {Origin.y:F2}");
        writer.WriteLine($"cellsize {Resolution:F2}");
        writer.WriteLine($"NODATA_value {NoData}");
        for (int r = rows - 1; r >= 0; r--)
        {
            for (int c = 0; c < cols; c++)
                writer.Write($"{Grid[r, c]:F3} ");
            writer.WriteLine();
        }
        Console.WriteLine($"[DEM] Exported ASCII Grid: {filePath}");
    }
}
