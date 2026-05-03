/* 

Simple terrain painter for:
- Grass base layer
- Rock on steep slopes
- Snow on high peaks
- Dirt paths in low/flat areas
- Procedural tree placement

Layer order:
0 = Dirt
1 = Grass
2 = Rock
3 = Snow

*/

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Terrain))]
public class SimpleTerrainPainter : MonoBehaviour
{
    private Terrain terrain;
    private TerrainData terrainData;

    private float minHeight;
    private float maxHeight;

    [Header("Layer Order")]
    [SerializeField] private int dirtLayer = 0;
    [SerializeField] private int grassLayer = 1;
    [SerializeField] private int rockLayer = 2;
    [SerializeField] private int snowLayer = 3;

    [Header("Rock Settings")]
    public float rockSlopeStart = 32f;
    public float rockSlopeFull = 55f;

    [Header("Snow Settings")]
    public float snowHeightStart = 0.82f;
    public float snowHeightFull = 0.95f;
    public float snowMaxSlope = 38f;

    [Header("Grass Variation")]
    public float grassNoiseScale = 18f;
    public float grassNoiseStrength = 0.12f;

    [Header("Dirt Paths")]
    public bool enableDirtPaths = true;
    public float pathMaxSlope = 12f;
    public float pathMinHeight = 0.08f;
    public float pathMaxHeight = 0.42f;
    public float pathNoiseScale = 7f;
    public float pathNoiseThreshold = 0.62f;
    public float pathStrength = 0.85f;

    [Header("Tree Settings")]
    public bool spawnTrees = true;
    public int treeCount = 400;
    public int treeSeed = 12345;
    public float treeMinHeight = 0.12f;
    public float treeMaxHeight = 0.68f;
    public float treeMaxSlope = 22f;
    public float treeNoiseScale = 9f;
    public float treeNoiseThreshold = 0.5f;
    public float minTreeSpacing = 0.025f;
    public float treeMinScale = 0.8f;
    public float treeMaxScale = 1.25f;

    #region Initialization
    private void Start()
    {
        PaintTerrain();

        if (spawnTrees)
            SpawnTrees();
    }
    #endregion

    #region Terrain Painting
    public void PaintTerrain()
    {
        CacheTerrain();

        if (terrainData.alphamapLayers < 4)
        {
            Debug.LogWarning("Needs 4 terrain layers: Dirt, Grass, Rock, Snow.");
            return;
        }

        CalculateHeightRange();

        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        float[,,] splatmap = new float[height, width, terrainData.alphamapLayers];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = x / (float)(width - 1);
                float ny = y / (float)(height - 1);

                TerrainSample sample = GetSample(nx, ny);

                float dirt = GetPathWeight(sample);
                float grass = GetGrassWeight(sample);
                float rock = GetRockWeight(sample);
                float snow = GetSnowWeight(sample);

                //Dirt cuts into grass, but shouldn't wipe out cliffs
                grass *= 1f - dirt;
                rock *= 1f - dirt * 0.35f;
                snow *= 1f - dirt;

                //Rock dominates steep surfaces
                grass *= 1f - rock;
                snow *= 1f - rock;

                float total = dirt + grass + rock + snow;

                if (total <= 0f)
                {
                    grass = 1f;
                    total = 1f;
                }

                splatmap[y, x, dirtLayer] = dirt / total;
                splatmap[y, x, grassLayer] = grass / total;
                splatmap[y, x, rockLayer] = rock / total;
                splatmap[y, x, snowLayer] = snow / total;
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmap);
    }
    #endregion

    #region Layer Weights
    float GetGrassWeight(TerrainSample sample)
    {
        float noise = Mathf.PerlinNoise(
            sample.nx * grassNoiseScale,
            sample.ny * grassNoiseScale
        );

        //Small variation breaks visible tiling
        return Mathf.Lerp(
            1f - grassNoiseStrength,
            1f + grassNoiseStrength,
            noise
        );
    }

    float GetRockWeight(TerrainSample sample)
    {
        //Slope drives rock placement
        return Mathf.InverseLerp(rockSlopeStart, rockSlopeFull, sample.slope);
    }

    float GetSnowWeight(TerrainSample sample)
    {
        //Avoid snow on steep cliffs
        if (sample.slope > snowMaxSlope)
            return 0f;

        return Mathf.InverseLerp(snowHeightStart, snowHeightFull, sample.height01);
    }

    float GetPathWeight(TerrainSample sample)
    {
        if (!enableDirtPaths)
            return 0f;

        //Paths only exist on flatter, mid-height terrain
        if (sample.slope > pathMaxSlope)
            return 0f;

        if (sample.height01 < pathMinHeight || sample.height01 > pathMaxHeight)
            return 0f;

        float noise = Mathf.PerlinNoise(
            sample.nx * pathNoiseScale,
            sample.ny * pathNoiseScale
        );

        float path = Mathf.InverseLerp(pathNoiseThreshold, 1f, noise);

        return path * pathStrength;
    }
    #endregion

    #region Trees
    public void SpawnTrees()
    {
        CacheTerrain();

        if (terrainData.treePrototypes.Length == 0)
        {
            Debug.LogWarning("Add tree prefabs in Terrain first.");
            return;
        }

        CalculateHeightRange();
        Random.InitState(treeSeed);

        List<TreeInstance> trees = new List<TreeInstance>();
        List<Vector2> placed = new List<Vector2>();

        int attempts = treeCount * 15;

        for (int i = 0; i < attempts && trees.Count < treeCount; i++)
        {
            float nx = Random.value;
            float ny = Random.value;

            TerrainSample sample = GetSample(nx, ny);

            if (!CanPlaceTree(sample))
                continue;

            Vector2 pos = new Vector2(nx, ny);

            if (!HasEnoughSpacing(pos, placed))
                continue;

            float scale = Random.Range(treeMinScale, treeMaxScale);

            trees.Add(new TreeInstance
            {
                position = new Vector3(nx, sample.treeY, ny),
                prototypeIndex = Random.Range(0, terrainData.treePrototypes.Length),
                widthScale = scale,
                heightScale = scale,
                color = Color.white,
                lightmapColor = Color.white
            });

            placed.Add(pos);
        }

        terrainData.treeInstances = trees.ToArray();
        terrain.Flush();

        Debug.Log($"Spawned {trees.Count} trees.");
    }

    bool CanPlaceTree(TerrainSample sample)
    {
        if (sample.height01 < treeMinHeight || sample.height01 > treeMaxHeight)
            return false;

        if (sample.slope > treeMaxSlope)
            return false;

        //Avoid cliffs, snow, and paths
        if (GetRockWeight(sample) > 0.25f) return false;
        if (GetSnowWeight(sample) > 0.1f) return false;
        if (GetPathWeight(sample) > 0.2f) return false;

        float noise = Mathf.PerlinNoise(
            sample.nx * treeNoiseScale + 200f,
            sample.ny * treeNoiseScale + 200f
        );

        return noise >= treeNoiseThreshold;
    }

    bool HasEnoughSpacing(Vector2 pos, List<Vector2> placed)
    {
        float spacing = minTreeSpacing * minTreeSpacing;

        for (int i = 0; i < placed.Count; i++)
        {
            if ((pos - placed[i]).sqrMagnitude < spacing)
                return false;
        }

        return true;
    }

    public void ClearTrees()
    {
        CacheTerrain();

        terrainData.treeInstances = new TreeInstance[0];
        terrain.Flush();
    }
    #endregion

    #region Terrain Data
    void CacheTerrain()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
    }

    void CalculateHeightRange()
    {
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        minHeight = float.MaxValue;
        maxHeight = float.MinValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = x / (float)(width - 1);
                float ny = y / (float)(height - 1);

                float h = terrainData.GetInterpolatedHeight(nx, ny);

                minHeight = Mathf.Min(minHeight, h);
                maxHeight = Mathf.Max(maxHeight, h);
            }
        }
    }

    TerrainSample GetSample(float nx, float ny)
    {
        float worldHeight = terrainData.GetInterpolatedHeight(nx, ny);

        float height01 = Mathf.InverseLerp(minHeight, maxHeight, worldHeight);
        float treeY = worldHeight / terrainData.size.y;
        float slope = terrainData.GetSteepness(nx, ny);

        return new TerrainSample
        {
            nx = nx,
            ny = ny,
            height01 = height01,
            treeY = treeY,
            slope = slope
        };
    }

    struct TerrainSample
    {
        public float nx;
        public float ny;
        public float height01;
        public float treeY;
        public float slope;
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimpleTerrainPainter))]
public class SimpleTerrainPainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SimpleTerrainPainter painter = (SimpleTerrainPainter)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Paint Terrain"))
            painter.PaintTerrain();

        if (GUILayout.Button("Spawn Trees"))
            painter.SpawnTrees();

        if (GUILayout.Button("Paint + Trees"))
        {
            painter.PaintTerrain();
            painter.SpawnTrees();
        }

        if (GUILayout.Button("Clear Trees"))
            painter.ClearTrees();
    }
}
#endif