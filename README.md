# 三维点云场景建模编程实践（C# 实现）

## 项目简介

本项目为测绘/GIS 专业"三维点云场景建模编程实践"课程的完整 C# 实现，涵盖从 LAS 点云数据读取到最终专题图制作与地形分析的全流程。

## 技术栈

- **语言**: C# (.NET 8.0)
- **核心算法**: 自研实现（LAS 解析、体素抽稀、统计滤波、DEM 栅格化、坡度坡向计算）
- **绘图**: System.Drawing (GDI+) 用于专题图
- **数学库**: Math.NET Numerics（矩阵运算）

## 数据说明

- **来源**: NEON 机载激光雷达点云数据
- **格式**: LAS 1.2
- **内容**: 包含地形和植被等复杂对象
- **下载**: https://figshare.com/ndownloader/files/7024955

## 项目结构

```
PointCloud-Modeling-CSharp/
├── src/
│   ├── Core/                 # LAS 数据读取与核心数据结构
│   │   ├── LasHeader.cs      # LAS 文件头解析
│   │   ├── LasPoint.cs       # 点云数据点定义
│   │   └── LasReader.cs      # LAS 文件读取器
│   ├── Preprocessing/        # 点云预处理
│   │   ├── Denoising.cs      # 统计滤波去噪
│   │   └── Downsampling.cs   # 体素网格抽稀
│   ├── Modeling/             # 三维模型构建
│   │   └── DemBuilder.cs     # DEM 栅格地形模型构建
│   ├── Analysis/             # 地形分析
│   │   ├── SlopeAspect.cs    # 坡度坡向计算
│   │   └── TerrainClassification.cs  # 地形分级分析
│   ├── Visualization/        # 专题图制作
│   │   ├── ThematicMap.cs    # 专题图绘制引擎
│   │   ├── ColorMap.cs       # 颜色映射
│   │   └── MapElements.cs    # 图名、图例、比例尺、指北针
│   └── Program.cs            # 主程序入口
├── docs/
│   └── 技术方案.md            # 详细技术方案文档
└── README.md
```

## 任务完成清单

| 序号 | 任务 | 状态 | 说明 |
|------|------|------|------|
| 1 | 点云预处理 | ✅ | LAS 读取、范围检查、统计滤波去噪、体素抽稀 |
| 2 | 三维模型构建 | ✅ | DEM 栅格地形模型（最近邻插值） |
| 3 | 坡度坡向计算 | ✅ | Horn 三阶反距离平方权算法 |
| 4 | 专题图制作 | ✅ | 高程图、坡度图、坡向图（含图名/图例/比例尺/指北针） |
| 5 | 自主应用分析 | ✅ | 地形起伏度分析 + 地形自动分级（平地/丘陵/山地/高山） |
| 6 | 成果提交 | ✅ | 所有中间成果与专题图自动输出 |

## 快速开始

### 1. 克隆仓库

```bash
git clone https://github.com/P313P/PointCloud-Modeling-CSharp.git
cd PointCloud-Modeling-CSharp
```

### 2. 下载数据

将下载的 LAS 文件放入项目根目录，或修改 `Program.cs` 中的路径。

### 3. 运行

```bash
dotnet run --project src/PointCloudModeling.csproj
```

## 算法说明

### 点云预处理
- **去噪**: 统计滤波——对每个点搜索半径 R 内的邻居，邻居数少于阈值的点视为噪点剔除
- **抽稀**: 体素网格下采样——将三维空间划分为固定边长的体素网格，每个体素保留一个代表点

### DEM 构建
- 采用**最近邻插值**将不规则点云栅格化为规则 DEM 网格
- 网格大小（分辨率）可配置，默认 1.0m

### 坡度坡向计算
- 采用 **Horn 三阶反距离平方权算法**（ArcGIS 同款算法）
- 3×3 移动窗口，中心点坡度的加权平均

### 地形分级分析
- **地形起伏度**: 在 5×5 窗口内计算高程最大值与最小值之差
- **地形分级标准**:
  - 平地: 起伏度 < 3m
  - 丘陵: 3m ≤ 起伏度 < 20m
  - 山地: 20m ≤ 起伏度 < 50m
  - 高山: 起伏度 ≥ 50m

## 输出成果

程序运行后在 `output/` 目录生成以下成果：

1. `dem.asc` — DEM 栅格数据（ASCII Grid 格式，可用 ArcGIS/QGIS 打开）
2. `elevation_map.png` — 高程专题图
3. `slope_map.png` — 坡度专题图
4. `aspect_map.png` — 坡向专题图
5. `relief_map.png` — 地形起伏度专题图
6. `terrain_class_map.png` — 地形分级专题图
7. `statistics.txt` — 统计分析报告

## 作者

- 邹亚东 / 测绘专业
- 课程: 三维点云场景建模编程实践
