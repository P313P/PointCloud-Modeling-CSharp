namespace PointCloudModeling.Analysis;

/// <summary>
/// Slope and Aspect calculation using Horn's 3rd-order finite difference method
/// Same algorithm used by ArcGIS and GDAL
/// </summary>
public class SlopeAspectAnalyzer
{
    public double CellSize { get; set; } = 1.0;
    public double[,]? Slope { get; private set; }
    public double[,]? Aspect { get; private set; }

    public (double[,] slope, double[,] aspect) Calculate(double[,] dem)
    {
        int rows = dem.GetLength(0);
        int cols = dem.GetLength(1);
        Slope = new double[rows, cols];
        Aspect = new double[rows, cols];

        Console.WriteLine("[Analysis] Calculating slope & aspect (Horn method)...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        for (int r = 1; r < rows - 1; r++)
            for (int c = 1; c < cols - 1; c++)
            {
                double a = dem[r - 1, c - 1], b = dem[r - 1, c], c_val = dem[r - 1, c + 1];
                double d = dem[r, c - 1], f = dem[r, c + 1];
                double g = dem[r + 1, c - 1], h = dem[r + 1, c], i = dem[r + 1, c + 1];

                double dzdx = ((c_val + 2 * f + i) - (a + 2 * d + g)) / (8 * CellSize);
                double dzdy = ((g + 2 * h + i) - (a + 2 * b + c_val)) / (8 * CellSize);

                Slope[r, c] = Math.Atan(Math.Sqrt(dzdx * dzdx + dzdy * dzdy)) * 180.0 / Math.PI;

                double aspectRad = Math.Atan2(dzdy, -dzdx);
                double aspectDeg = aspectRad * 180.0 / Math.PI;
                if (aspectDeg < 0) Aspect[r, c] = 90.0 - aspectDeg;
                else if (aspectDeg > 90.0) Aspect[r, c] = 360.0 - aspectDeg + 90.0;
                else Aspect[r, c] = 90.0 - aspectDeg;

                if (Slope[r, c] < 0.5) Aspect[r, c] = -1;
            }

        CopyBorders(Slope);
        CopyBorders(Aspect);

        sw.Stop();
        Console.WriteLine($"[Analysis] Slope range: [{Slope.Cast<double>().Min():F1}, {Slope.Cast<double>().Max():F1}] deg, {sw.Elapsed.TotalSeconds:F1}s");
        return (Slope, Aspect);
    }

    private void CopyBorders(double[,] grid)
    {
        int rows = grid.GetLength(0), cols = grid.GetLength(1);
        for (int c = 0; c < cols; c++) { grid[0, c] = grid[1, c]; grid[rows - 1, c] = grid[rows - 2, c]; }
        for (int r = 0; r < rows; r++) { grid[r, 0] = grid[r, 1]; grid[r, cols - 1] = grid[r, cols - 2]; }
    }

    public static Dictionary<string, int> ClassifySlope(double[,] slope)
    {
        var cats = new Dictionary<string, int>
        {
            ["Flat (0-3 deg)"] = 0, ["Gentle (3-8 deg)"] = 0, ["Moderate (8-15 deg)"] = 0,
            ["Steep (15-25 deg)"] = 0, ["Very Steep (25-35 deg)"] = 0, ["Extreme (>35 deg)"] = 0
        };
        foreach (var v in slope)
        {
            if (v < 3) cats["Flat (0-3 deg)"]++;
            else if (v < 8) cats["Gentle (3-8 deg)"]++;
            else if (v < 15) cats["Moderate (8-15 deg)"]++;
            else if (v < 25) cats["Steep (15-25 deg)"]++;
            else if (v < 35) cats["Very Steep (25-35 deg)"]++;
            else cats["Extreme (>35 deg)"]++;
        }
        return cats;
    }
}
