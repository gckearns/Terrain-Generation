using UnityEngine;

public class SpherePreviewGenerator : MonoBehaviour
{
    public enum PreviewMode
    {
        WholeSphere,
        SingleChunk,
        AllChunks,
        Threaded
    }

    public PreviewMode previewMode;

    [IntArraySlider(new int[] { 1, 2, 4, 8, 16 })]
    public int lod;

    public int viewDistance;
    public Vector3 offset;

    public NoiseSettings noiseSettings;
    public SphereMeshSettings sphereMeshSettings;
    public bool showReferenceSphere;
    public bool showReferencePlanes;

    public GameObject previewReferenceSphere;
    public GameObject previewReferencePlanes;
    public GameObject allSphereChunks;

    public MeshFilter meshFilter;
    public Material material;

    int diameter { get { return 2 * sphereMeshSettings.radius; } }
    float[,,] noiseMap;

    public void GeneratePreview()
    {
        if (previewMode == PreviewMode.WholeSphere)
        {
            allSphereChunks.SetActive(false);
            meshFilter.gameObject.SetActive(true);

            noiseMap = NoiseMapGenerator.GetNoiseMap3D(diameter + 1, diameter + 1, diameter + 1, offset, noiseSettings);

            NoiseMapData mapData = new NoiseMapData(noiseMap, offset);

            Mesh mesh = MeshGenerator.GenerateSphereMesh(mapData, sphereMeshSettings, lod).GetMesh();

            meshFilter.sharedMesh = mesh;

            UpdateReferenceSphere();
            UpdateReferencePlanes();
        }
        else if (previewMode == PreviewMode.SingleChunk)
        {
            allSphereChunks.SetActive(false);
            meshFilter.gameObject.SetActive(true);

            noiseMap = NoiseMapGenerator.GetNoiseMap3D(sphereMeshSettings.chunkSize + 1, sphereMeshSettings.chunkSize + 1, sphereMeshSettings.chunkSize + 1, offset, noiseSettings);

            NoiseMapData mapData = new NoiseMapData(noiseMap, offset);

            Mesh mesh = MeshGenerator.GenerateSphereChunkMesh(mapData, sphereMeshSettings, lod).GetMesh();

            meshFilter.sharedMesh = mesh;
        }
        else if (previewMode == PreviewMode.AllChunks)
        {
            meshFilter.gameObject.SetActive(false);
            allSphereChunks.SetActive(true);

            DeleteChunks();

            int chunkRadius = Mathf.CeilToInt(sphereMeshSettings.radius / sphereMeshSettings.chunkSize);
            if (noiseSettings.isNormalized)
            {
                chunkRadius += Mathf.Max(Mathf.RoundToInt(sphereMeshSettings.noiseHeightScale * (1 - noiseSettings.normShift)) / sphereMeshSettings.chunkSize, 2);
            }
            else
            {
                chunkRadius += Mathf.Max(Mathf.RoundToInt(sphereMeshSettings.noiseHeightScale * 1.5f) / sphereMeshSettings.chunkSize, 2);
            }
            if (chunkRadius % 2 == 1)
            {
                chunkRadius--;
            }
            // Save the old offset so we can change it back later
            for (int z = -chunkRadius; z < chunkRadius; z++)
            {
                for (int y = -chunkRadius; y < chunkRadius; y++)
                {
                    for (int x = -chunkRadius; x < chunkRadius; x++)
                    {
                        Vector3 chunkOffset = new Vector3(x * sphereMeshSettings.chunkSize, y * sphereMeshSettings.chunkSize, z * sphereMeshSettings.chunkSize);

                        GameObject newChunk = new GameObject("Chunk: " + chunkOffset.x + ", " + chunkOffset.y + ", " + chunkOffset.z);
                        newChunk.transform.SetParent(allSphereChunks.transform);

                        noiseMap = NoiseMapGenerator.GetNoiseMap3D(sphereMeshSettings.chunkSize + 1, sphereMeshSettings.chunkSize + 1, sphereMeshSettings.chunkSize + 1, chunkOffset, noiseSettings);

                        NoiseMapData mapData = new NoiseMapData(noiseMap, chunkOffset);//

                        Mesh mesh = MeshGenerator.GenerateSphereChunkMesh(mapData, sphereMeshSettings, lod).GetMesh();//

                        newChunk.AddComponent<MeshFilter>().sharedMesh = mesh;
                        newChunk.AddComponent<MeshRenderer>().sharedMaterial = material;
                    }
                }
            }
        }
        else if (previewMode == PreviewMode.Threaded)
        {

            MyStopwatch.stopwatch.Start();

            meshFilter.gameObject.SetActive(false);
            allSphereChunks.SetActive(true);

            DeleteChunks();

            //int chunkRadius = Mathf.CeilToInt(sphereMeshSettings.radius / sphereMeshSettings.chunkSize) + 2;
            //if (noiseSettings.isNormalized)
            //{
            //    chunkRadius += Mathf.Max(Mathf.RoundToInt(sphereMeshSettings.noiseHeightScale * (1 - noiseSettings.normShift)) / sphereMeshSettings.chunkSize, 2);
            //}
            //else
            //{
            //    chunkRadius += Mathf.Max(Mathf.RoundToInt(sphereMeshSettings.noiseHeightScale * 1.5f) / sphereMeshSettings.chunkSize, 2);
            //}

            int chunkRadius = viewDistance / sphereMeshSettings.chunkSize;

            int chunksCreated = 0;
            for (int z = -chunkRadius; z < chunkRadius; z++)
            {
                for (int y = Mathf.FloorToInt(sphereMeshSettings.radius / sphereMeshSettings.chunkSize) - chunkRadius; y < Mathf.CeilToInt(sphereMeshSettings.radius / sphereMeshSettings.chunkSize) + 2; y++)
                {
                    for (int x = -chunkRadius; x < chunkRadius; x++)
                    {
                        Vector3 chunkOffset = new Vector3(x * sphereMeshSettings.chunkSize, y * sphereMeshSettings.chunkSize, z * sphereMeshSettings.chunkSize);

                        new TerrainChunk(chunkOffset, allSphereChunks.transform, noiseSettings, sphereMeshSettings, material, allSphereChunks.transform);

                        chunksCreated++;
                    }
                }
            }


            //if (chunkRadius % 2 == 1)
            //{
            //    chunkRadius--;
            //}

            //int chunksCreated = 0;
            //for (int z = -chunkRadius; z < chunkRadius; z++)
            //{
            //    for (int y = -chunkRadius; y < chunkRadius; y++)
            //    {
            //        for (int x = -chunkRadius; x < chunkRadius; x++)
            //        {
            //            Vector3 chunkOffset = new Vector3(x * sphereMeshSettings.chunkSize, y * sphereMeshSettings.chunkSize, z * sphereMeshSettings.chunkSize);

            //            new TerrainChunk(chunkOffset, sphereMeshSettings.chunkSize, allSphereChunks.transform, noiseSettings, sphereMeshSettings, material, vertexIncrement);

            //            chunksCreated++;
            //        }
            //    }
            //}
            MyStopwatch.s = string.Format("{0}", chunksCreated);
        }
        UpdateReferenceSphere();
        UpdateReferencePlanes();
    }

    void DeleteChunks()
    {
        while (allSphereChunks.transform.childCount > 0)
        {
            DestroyImmediate(allSphereChunks.transform.GetChild(0).gameObject);
        }
    }

    public void UpdateReferenceSphere()
    {
        previewReferenceSphere.transform.localScale = Vector3.one * (diameter);
        previewReferenceSphere.SetActive(showReferenceSphere);
    }

    public void UpdateReferencePlanes()
    {
        previewReferencePlanes.transform.localScale = Vector3.one * (diameter);
        previewReferencePlanes.SetActive(showReferencePlanes);
    }

    void OnValidate()
    {
        noiseSettings.OnValidate();
        sphereMeshSettings.OnValidate();
    }

    void OnDisable()
    {
        DeleteChunks();
    }

    public class MyStopwatch : System.Diagnostics.Stopwatch
    {
        public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public static string s;
    }
}
