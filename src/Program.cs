using PointCloudModeling.Core;
using PointCloudModeling.Preprocessing;
using PointCloudModeling.Modeling;
using PointCloudModeling.Analysis;
using PointCloudModeling.Visualization;

namespace PointCloudModeling;

class Program
{
    static string LasFilePath = @"data.las";
    static string OutputDir = @"output";

    static void Main(string[] args)
    {
        if (args.Length > 0) LasFilePath = args[0];
        if (args.Length > 1) OutputDir = args[1];

        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("  3D Point Cloud Scene Modeling - C# Implementation");
        Console.WriteLine("=".PadRight(60, '='));
        Directory.CreateDirectory(OutputDir);

        try
        {
            // Step 1: Read LAS
            Console.WriteLine("\n[Step 1] Reading LAS...");
            List<LasPoint> points;
            using (var r = new LasReader()) { r.Open(LasFilePath); points = r.ReadAllPoints(); }
            if (points.Count == 0) return;

            // Step 2: Preprocessing
            Console.WriteLine("\n[Step 2] Preprocessing...");
            points = new StatisticalFilter { SearchRadius = 2.0, MinNeighbors = 10 }.Filter(points);
            points = new VoxelGridDownsampler { VoxelSize = 0.5 }.Downsample(points);

            // Step 3: DEM
            Console.WriteLine("\n[Step 3] Building DEM...");
            var demBuilder = new DemBuilder { Resolution = 1.0 };
            double[,] dem = demBuilder.BuildDem(points);
            demBuilder.ExportAsc(Path.Combine(OutputDir, "dem.asc"));

            // Step 4: Slope & Aspect
            Console.WriteLine("\n[Step 4] Slope & Aspect...");
            var (slope, aspect) = new SlopeAspectAnalyzer { CellSize = 1.0 }.Calculate(dem);

            // Step 5: Terrain Classification
            Console.WriteLine("\n[Step 5] Terrain Classification...");
            var (relief, terrainClass) = new TerrainClassifier { WindowSize = 5 }.Classify(dem);
            string report = new TerrainClassifier { WindowSize = 5 }.GenerateReport(dem);
            File.WriteAllText(Path.Combine(OutputDir, "statistics.txt"), report);

            // Step 6: Maps
            Console.WriteLine("\n[Step 6] Thematic Maps...");
            var rnd = new ThematicMapRenderer();
            using (var m = rnd.RenderElevationMap(dem)) rnd.SaveMap(m, Path.Combine(OutputDir, "elevation_map.png"));
            using (var m = rnd.RenderSlopeMap(slope)) rnd.SaveMap(m, Path.Combine(OutputDir, "slope_map.png"));
            using (var m = rnd.RenderAspectMap(aspect)) rnd.SaveMap(m, Path.Combine(OutputDir, "aspect_map.png"));
            using (var m = rnd.RenderReliefMap(relief)) rnd.SaveMap(m, Path.Combine(OutputDir, "relief_map.png"));
            using (var m = rnd.RenderTerrainClassMap(terrainClass)) rnd.SaveMap(m, Path.Combine(OutputDir, "terrain_class_map.png"));

            Console.WriteLine("\n" + "=".PadRight(60, '='));
            Console.WriteLine("  All tasks completed!");
            Console.WriteLine($"  Output: {Path.GetFullPath(OutputDir)}");
            Console.WriteLine("=".PadRight(60, '='));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }
    }
}
