namespace PointCloudModeling.Visualization;

public static class ColorMap
{
    public static Color ElevationToColor(double elev, double minElev, double maxElev)
    {
        double t = Math.Clamp((elev - minElev) / (maxElev - minElev), 0, 1);
        (double p, byte r, byte g, byte b)[] stops = new[]
        {
            (0.00, 34, 139, 34), (0.15, 50, 205, 50), (0.30, 154, 205, 50),
            (0.45, 255, 255, 0), (0.60, 210, 180, 140), (0.75, 139, 119, 101),
            (0.90, 139, 69, 19), (1.00, 255, 250, 250),
        };
        return Interpolate(t, stops);
    }

    public static Color SlopeToColor(double slope)
    {
        if (slope < 3) return Color.FromArgb(144, 238, 144);
        if (slope < 8) return Color.FromArgb(173, 255, 47);
        if (slope < 15) return Color.FromArgb(255, 255, 0);
        if (slope < 25) return Color.FromArgb(255, 165, 0);
        if (slope < 35) return Color.FromArgb(255, 69, 0);
        return Color.FromArgb(178, 34, 34);
    }

    public static Color AspectToColor(double aspect)
    {
        if (aspect < 0) return Color.FromArgb(200, 200, 200);
        return HsvToRgb(aspect / 360.0 * 240.0, 0.7, 0.9);
    }

    public static Color ReliefToColor(double relief, double maxRelief)
    {
        byte v = (byte)(255 * (1 - Math.Clamp(relief / maxRelief, 0, 1)));
        return Color.FromArgb(255, v, v);
    }

    public static Color TerrainClassToColor(int tc) => tc switch
    {
        0 => Color.FromArgb(144, 238, 144),
        1 => Color.FromArgb(255, 215, 0),
        2 => Color.FromArgb(255, 140, 0),
        3 => Color.FromArgb(139, 69, 19),
        _ => Color.Gray
    };

    public static (string, Color)[] GetTerrainLegend() => new[]
    {
        ("Flat (<3m)", TerrainClassToColor(0)), ("Hill (3-20m)", TerrainClassToColor(1)),
        ("Mountain (20-50m)", TerrainClassToColor(2)), ("High Mountain (>=50m)", TerrainClassToColor(3)),
    };

    public static (string, Color)[] GetSlopeLegend() => new[]
    {
        ("Flat (0-3)", SlopeToColor(0)), ("Gentle (3-8)", SlopeToColor(5)),
        ("Moderate (8-15)", SlopeToColor(11)), ("Steep (15-25)", SlopeToColor(20)),
        ("Very Steep (25-35)", SlopeToColor(30)), ("Extreme (>35)", SlopeToColor(40)),
    };

    private static Color Interpolate(double t, (double p, byte r, byte g, byte b)[] s)
    {
        if (t <= s[0].p) return Color.FromArgb(s[0].r, s[0].g, s[0].b);
        if (t >= s[^1].p) return Color.FromArgb(s[^1].r, s[^1].g, s[^1].b);
        for (int i = 0; i < s.Length - 1; i++)
            if (t >= s[i].p && t <= s[i + 1].p)
            {
                double lt = (t - s[i].p) / (s[i + 1].p - s[i].p);
                return Color.FromArgb((byte)(s[i].r + lt * (s[i + 1].r - s[i].r)),
                                      (byte)(s[i].g + lt * (s[i + 1].g - s[i].g)),
                                      (byte)(s[i].b + lt * (s[i + 1].b - s[i].b)));
            }
        return Color.Gray;
    }

    private static Color HsvToRgb(double h, double s, double v)
    {
        int hi = (int)(h / 60.0) % 6;
        double f = h / 60.0 - hi, p = v * (1 - s), q = v * (1 - f * s), t = v * (1 - (1 - f) * s);
        double r, g, b;
        switch (hi)
        {
            case 0: r = v; g = t; b = p; break; case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break; case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break; default: r = v; g = p; b = q; break;
        }
        return Color.FromArgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }
}
