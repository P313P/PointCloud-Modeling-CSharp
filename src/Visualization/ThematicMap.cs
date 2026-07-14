namespace PointCloudModeling.Visualization;

public class ThematicMapRenderer
{
    public int MapWidth { get; set; } = 1200;
    public int MapHeight { get; set; } = 900;
    public int Margin { get; set; } = 80;

    public Bitmap RenderElevationMap(double[,] dem, string title = "Elevation Map")
    {
        double minZ = dem.Cast<double>().Min(), maxZ = dem.Cast<double>().Max();
        return Render(dem, (v, r, c) => ColorMap.ElevationToColor(v, minZ, maxZ), title, $"Elevation (m): {minZ:F1} ~ {maxZ:F1}", ColorMap.GetTerrainLegend());
    }

    public Bitmap RenderSlopeMap(double[,] slope, string title = "Slope Map")
    {
        return Render(slope, (v, r, c) => ColorMap.SlopeToColor(v), title, "Slope (degrees)", ColorMap.GetSlopeLegend());
    }

    public Bitmap RenderAspectMap(double[,] aspect, string title = "Aspect Map")
    {
        return Render(aspect, (v, r, c) => ColorMap.AspectToColor(v), title, "Aspect (degrees from North)");
    }

    public Bitmap RenderReliefMap(double[,] relief, string title = "Relief Amplitude Map")
    {
        double maxR = relief.Cast<double>().Max();
        return Render(relief, (v, r, c) => ColorMap.ReliefToColor(v, maxR), title, $"Relief (m): 0 ~ {maxR:F1}");
    }

    public Bitmap RenderTerrainClassMap(int[,] tc, string title = "Terrain Classification Map")
    {
        return RenderInt(tc, (v, r, c) => ColorMap.TerrainClassToColor(v), title, "Terrain Type", ColorMap.GetTerrainLegend());
    }

    private Bitmap Render(double[,] data, Func<double, int, int, Color> cf, string title, string lt, (string, Color)[]? le = null)
    {
        int rows = data.GetLength(0), cols = data.GetLength(1);
        int mw = MapWidth - 2 * Margin, mh = MapHeight - 2 * Margin;
        double cs = Math.Min((double)mw / cols, (double)mh / rows);
        int aw = (int)(cols * cs), ah = (int)(rows * cs);
        int ox = Margin + (mw - aw) / 2, oy = Margin + (mh - ah) / 2;

        var bmp = new Bitmap(MapWidth, MapHeight);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                g.FillRectangle(new SolidBrush(cf(data[r, c], r, c)), ox + (int)(c * cs), oy + (int)((rows - 1 - r) * cs), (int)Math.Ceiling(cs), (int)Math.Ceiling(cs));

        g.DrawRectangle(Pens.Black, ox, oy, aw, ah);
        MapElements.DrawTitle(g, title, MapWidth / 2, 30);
        MapElements.DrawNorthArrow(g, MapWidth - 60, 80);
        MapElements.DrawScaleBar(g, Margin + 20, MapHeight - 60, cs);
        if (le != null) MapElements.DrawLegend(g, MapWidth - 200, MapHeight - 250, lt, le);
        return bmp;
    }

    private Bitmap RenderInt(int[,] data, Func<int, int, int, Color> cf, string title, string lt, (string, Color)[]? le = null)
    {
        int rows = data.GetLength(0), cols = data.GetLength(1);
        int mw = MapWidth - 2 * Margin, mh = MapHeight - 2 * Margin;
        double cs = Math.Min((double)mw / cols, (double)mh / rows);
        int aw = (int)(cols * cs), ah = (int)(rows * cs);
        int ox = Margin + (mw - aw) / 2, oy = Margin + (mh - ah) / 2;

        var bmp = new Bitmap(MapWidth, MapHeight);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                g.FillRectangle(new SolidBrush(cf(data[r, c], r, c)), ox + (int)(c * cs), oy + (int)((rows - 1 - r) * cs), (int)Math.Ceiling(cs), (int)Math.Ceiling(cs));

        g.DrawRectangle(Pens.Black, ox, oy, aw, ah);
        MapElements.DrawTitle(g, title, MapWidth / 2, 30);
        MapElements.DrawNorthArrow(g, MapWidth - 60, 80);
        MapElements.DrawScaleBar(g, Margin + 20, MapHeight - 60, cs);
        if (le != null) MapElements.DrawLegend(g, MapWidth - 200, MapHeight - 250, lt, le);
        return bmp;
    }

    public void SaveMap(Bitmap bmp, string path)
    {
        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine($"[Map] Saved: {path}");
    }
}
