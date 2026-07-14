namespace PointCloudModeling.Analysis;

/// <summary>
/// Terrain classification based on relief amplitude (起伏度)
/// Standard: China 1:1,000,000 geomorphology mapping
/// </summary>
public class TerrainClassifier
{
    public int WindowSize { get; set; } = 5;
    public double[,]? Relief { get; private set; }
    public int[,]? TerrainClass { get; private set; }

    public static class Thresholds
    {
        public const double Flat = 3.0;
        public const double Hill = 20.0;
        public const double Mountain = 50.0;
    }

    public (double[,] relief, int[,] terrainClass) Classify(double[,] dem)
    {
        int rows = dem.GetLength(0), cols = dem.GetLength(1);
        int hw = WindowSize / 2;
        Relief = new double[rows, cols];
        TerrainClass = new int[rows, cols];

        Console.WriteLine($"[Terrain] Relief amplitude ({WindowSize}x{WindowSize} window)...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (int r = hw; r < rows - hw; r++)
            for (int c = hw; c < cols - hw; c++)
            {
                double minZ = double.MaxValue, maxZ = double.MinValue;
                for (int dr = -hw; dr <= hw; dr++)
                    for (int dc = -hw; dc <= hw; dc++)
                    {
                        double z = dem[r + dr, c + dc];
                        if (z < minZ) minZ = z;
                        if (z > maxZ) maxZ = z;
                    }
                double relief = maxZ - minZ;
                Relief[r, c] = relief;
                if (relief < Thresholds.Flat) TerrainClass[r, c] = 0;
                else if (relief < Thresholds.Hill) TerrainClass[r, c] = 1;
                else if (relief < Thresholds.Mountain) TerrainClass[r, c] = 2;
                else TerrainClass[r, c] = 3;
            }

        FillBorders(Relief, 0);
        FillBorders(TerrainClass, 0);
        sw.Stop();

        var stats = GenerateStatistics();
        Console.WriteLine("[Terrain] Results:");
        foreach (var kvp in stats)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value:N0} cells ({kvp.Value * 100.0 / (rows * cols):F1}%)");

        return (Relief, TerrainClass);
    }

    private void FillBorders(double[,] g, double v) { int r = g.GetLength(0), c = g.GetLength(1), h = WindowSize / 2; for (int i = 0; i < r; i++) for (int j = 0; j < c; j++) if (i < h || i >= r - h || j < h || j >= c - h) g[i, j] = v; }
    private void FillBorders(int[,] g, int v) { int r = g.GetLength(0), c = g.GetLength(1), h = WindowSize / 2; for (int i = 0; i < r; i++) for (int j = 0; j < c; j++) if (i < h || i >= r - h || j < h || j >= c - h) g[i, j] = v; }

    public Dictionary<string, int> GenerateStatistics()
    {
        var s = new Dictionary<string, int> { ["Flat (<3m)"] = 0, ["Hill (3-20m)"] = 0, ["Mountain (20-50m)"] = 0, ["High Mountain (>=50m)"] = 0 };
        foreach (var v in TerrainClass!) switch (v) { case 0: s["Flat (<3m)"]++; break; case 1: s["Hill (3-20m)"]++; break; case 2: s["Mountain (20-50m)"]++; break; case 3: s["High Mountain (>=50m)"]++; break; }
        return s;
    }

    public string GenerateReport(double[,] dem)
    {
        if (TerrainClass == null || Relief == null) return "No classification data";
        var stats = GenerateStatistics();
        int total = TerrainClass.Length;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=".PadRight(50, '='));
        sb.AppendLine("  3D Point Cloud Terrain Analysis Report");
        sb.AppendLine("=".PadRight(50, '='));
        sb.AppendLine();
        sb.AppendLine($"  DEM Range:");
        sb.AppendLine($"    Elevation: {dem.Cast<double>().Min():F2}m ~ {dem.Cast<double>().Max():F2}m");
        sb.AppendLine($"    Mean: {dem.Cast<double>().Average():F2}m");
        sb.AppendLine($"    Grid: {dem.GetLength(1)} x {dem.GetLength(0)}");
        sb.AppendLine();
        sb.AppendLine($"  Relief Amplitude (window={WindowSize}x{WindowSize}):");
        sb.AppendLine($"    Mean relief: {Relief.Cast<double>().Average():F2}m");
        sb.AppendLine();
        sb.AppendLine("  Terrain Classification:");
        foreach (var kvp in stats)
            sb.AppendLine($"    {kvp.Key}: {kvp.Value,8:N0} cells ({kvp.Value * 100.0 / total,5:F1}%)");
        sb.AppendLine();
        sb.AppendLine("  Suitability Assessment:");
        double fp = stats["Flat (<3m)"] * 100.0 / total;
        sb.AppendLine(fp > 60 ? "    Terrain is generally flat, suitable for construction." :
                        fp > 30 ? "    Terrain is mostly hilly; slope effects must be considered." :
                        "    Terrain is mountainous; construction difficulty is high.");
        sb.AppendLine();
        sb.AppendLine("=".PadRight(50, '='));
        return sb.ToString();
    }
}
