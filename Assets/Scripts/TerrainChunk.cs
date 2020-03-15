using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{

    GameObject gameObject;
    Bounds bounds;
    Transform viewer;

    NoiseSettings noiseSettings;
    float[,,] noiseMap;
    NoiseMapData noiseMapData;

    SphereMeshSettings meshSettings;

    MeshFilter meshFilter;
    MeshCollider meshCollider;
    LODMesh[] lodMeshes;
    LODSettings[] lodSettings;
    int lodMeshIndex = -1;

    bool hasRequestedNoiseMap;
    bool hasNoiseMap;

    public TerrainChunk(Vector3 offset, Transform parent, NoiseSettings noiseSettings, SphereMeshSettings meshSettings, Material material, Transform viewer)
    {
        Vector3 size = Vector3.one * meshSettings.chunkSize;
        bounds = new Bounds(offset + size / 2, size);

        gameObject = new GameObject("Chunk: " + bounds.min);
        gameObject.transform.SetParent(parent);

        this.viewer = viewer;
        this.noiseSettings = noiseSettings;
        this.meshSettings = meshSettings;
        this.lodSettings = meshSettings.lodSettings;

        gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        InitializeLOD();

        hasRequestedNoiseMap = true;
        UnityTaskManager.ScheduleTask(RequestNoiseMap, OnNoiseMapReceived);
    }

    public void Update()
    {
        var sqrDistanceToViewer = bounds.SqrDistance(viewer.position);
        var sqrMaxViewDist = lodSettings[lodSettings.Length - 1].maxViewDist * lodSettings[lodSettings.Length - 1].maxViewDist;

        bool isVisible = sqrDistanceToViewer <= sqrMaxViewDist;

        if (isVisible && hasRequestedNoiseMap)
        {
            lodMeshIndex = 0;

            for (int i = 0; i < lodSettings.Length - 1; i++)
            {
                var sqrLODMaxViewDist = lodSettings[i].maxViewDist * lodSettings[i].maxViewDist;

                if (sqrDistanceToViewer <= sqrLODMaxViewDist)
                {
                    break;
                }
                else
                {
                    lodMeshIndex++;
                }
            }

            var lodMesh = lodMeshes[lodMeshIndex];

            if (lodMesh.hasMesh)
            {
                meshFilter.sharedMesh = lodMesh.mesh;
            }
            else if (!lodMesh.hasRequestedMesh && hasNoiseMap)
            {
                //lodMesh.RequestMesh(noiseMapData);// to remove
                RequestMesh(lodMesh);
            }
        }
        else if (!hasRequestedNoiseMap)
        {
            hasRequestedNoiseMap = true;
            UnityTaskManager.ScheduleTask(RequestNoiseMap, OnNoiseMapReceived);
        }

        SetActive(isVisible);
    }

    public void RequestMesh(LODMesh lodMesh)
    {
        lodMesh.hasRequestedMesh = true;
        UnityTaskManager.ScheduleTask(RequestMeshData, OnMeshDataReceived);
    }

    object RequestMeshData()
    {
        return MeshGenerator.GenerateSphereChunkMesh(noiseMapData, meshSettings, lodMeshIndex);
    }

    void OnMeshDataReceived(object meshData)
    {
        MeshData receivedMeshData = (MeshData)meshData;
        LODMesh receivedLODMesh = lodMeshes[receivedMeshData.lod];

        receivedLODMesh.mesh = receivedMeshData.GetMesh();
        receivedLODMesh.hasMesh = true;

        meshFilter.sharedMesh = receivedLODMesh.mesh;
    }

    void InitializeLOD()
    {
        lodMeshes = new LODMesh[lodSettings.Length];

        for (int i = 0; i < lodSettings.Length; i++)
        {
            //lodMeshes[i] = new LODMesh(lodSettings[i].vertexIncrement, meshSettings);
            lodMeshes[i] = new LODMesh();
        }
    }

    object RequestNoiseMap()
    {
        return NoiseMapGenerator.GetNoiseMap3D(meshSettings.chunkSize + 1, meshSettings.chunkSize + 1, meshSettings.chunkSize + 1, bounds.min, noiseSettings);
    }

    void OnNoiseMapReceived(object noiseMap)
    {
        hasNoiseMap = true;
        //Debug.Log("noiseMap received");
        noiseMapData = new NoiseMapData((float[,,])noiseMap, bounds.min);

        Update();
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public bool isActive
    {
        get
        {
            return gameObject.activeSelf;
        }
    }
}
