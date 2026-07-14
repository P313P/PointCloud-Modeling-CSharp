namespace PointCloudModeling.Visualization;

public static class MapElements
{
    public static void DrawTitle(Graphics g, string title, int cx, int y)
    {
        using var font = new Font("SimHei", 16, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Black);
        var sz = g.MeasureString(title, font);
        g.DrawString(title, font, brush, cx - sz.Width / 2, y);
    }

    public static void DrawNorthArrow(Graphics g, int x, int y, int size = 40)
    {
        var pts = new[] { new PointF(x, y - size), new PointF(x - size * 0.3f, y + size * 0.2f), new PointF(x + size * 0.3f, y + size * 0.2f) };
        g.FillPolygon(Brushes.Black, pts);
        using var font = new Font("Arial", 10, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Black);
        var ns = g.MeasureString("N", font);
        g.DrawString("N", font, brush, x - ns.Width / 2, y - size - 18);
        g.DrawEllipse(Pens.Black, x - size * 0.35f, y - size * 0.8f, size * 0.7f, size * 0.7f);
    }

    public static void DrawScaleBar(Graphics g, int x, int y, double pxSizeM)
    {
        int[] nice = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
        int barM = 50;
        foreach (var len in nice) if (len / pxSizeM > 30 && len / pxSizeM < 150) { barM = len; break; }
        int bp = (int)(barM / pxSizeM), bh = 8, nSeg = 4, sw = bp / nSeg;
        for (int i = 0; i < nSeg; i++) g.FillRectangle(i % 2 == 0 ? Brushes.Black : Brushes.White, x + i * sw, y, sw, bh);
        g.DrawRectangle(Pens.Black, x, y, bp, bh);
        using var font = new Font("Arial", 9);
        using var brush = new SolidBrush(Color.Black);
        g.DrawString("0", font, brush, x - 5, y + bh + 2);
        var ls = g.MeasureString($"{barM}m", font);
        g.DrawString($"{barM}m", font, brush, x + bp - ls.Width / 2, y + bh + 2);
    }

    public static void DrawLegend(Graphics g, int x, int y, string title, (string label, Color color)[] entries)
    {
        int w = 160, ih = 22, th = 24, h = th + entries.Length * ih + 10;
        g.FillRectangle(new SolidBrush(Color.FromArgb(240, 255, 255, 255)), x, y, w, h);
        g.DrawRectangle(Pens.Black, x, y, w, h);
        using var tFont = new Font("SimHei", 10, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Black);
        g.DrawString(title, tFont, brush, x + 5, y + 3);
        g.DrawLine(Pens.Gray, x + 5, y + th - 2, x + w - 5, y + th - 2);
        using var eFont = new Font("Arial", 9);
        for (int i = 0; i < entries.Length; i++)
        {
            int ey = y + th + i * ih;
            g.FillRectangle(new SolidBrush(entries[i].color), x + 8, ey + 3, 18, 14);
            g.DrawRectangle(Pens.Gray, x + 8, ey + 3, 18, 14);
            g.DrawString(entries[i].label, eFont, brush, x + 30, ey + 3);
        }
    }
}
