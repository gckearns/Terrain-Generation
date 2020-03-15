using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    public float viewerMoveDistToUpdate;

    public Vector3 center;

    public NoiseSettings noiseSettings;
    public SphereMeshSettings sphereMeshSettings;
    public Material material;
    public Transform viewer;

    Vector3 viewerPosLastUpdate;
    List<Vector3Int> chunksInUpdateRadius = new List<Vector3Int>();
    HashSet<Vector3Int> chunksVisibleLastUpdate = new HashSet<Vector3Int>();
    HashSet<Vector3Int> chunksInDictionary = new HashSet<Vector3Int>();
    Dictionary<Vector3Int, TerrainChunk> terrainChunksDictionary = new Dictionary<Vector3Int, TerrainChunk>();

    Octree octree;

    // Use this for initialization
    void Start()
    {
        octree = new Octree(sphereMeshSettings);
    }

    // Update is called once per frame
    void Update()
    {
        var sqrViewerMoveDistToUpdate = viewerMoveDistToUpdate * viewerMoveDistToUpdate;
        var sqrViewerMoveDist = (viewer.position - viewerPosLastUpdate).sqrMagnitude;

        if (sqrViewerMoveDist >= sqrViewerMoveDistToUpdate)
        {
            GetChunksInUpdateRadius();
            viewerPosLastUpdate = viewer.position;
        }
    }

    void GetChunksInUpdateRadius()
    {
        int chunkUpdateRadius = Mathf.CeilToInt(sphereMeshSettings.lodSettings[sphereMeshSettings.lodSettings.Length - 1].maxViewDist / sphereMeshSettings.chunkSize);

        var viewerChunkX = Mathf.FloorToInt(viewer.position.x / sphereMeshSettings.chunkSize);
        var viewerChunkY = Mathf.FloorToInt(viewer.position.y / sphereMeshSettings.chunkSize);
        var viewerChunkZ = Mathf.FloorToInt(viewer.position.z / sphereMeshSettings.chunkSize);

        for (int z = viewerChunkZ - chunkUpdateRadius; z < viewerChunkZ + chunkUpdateRadius; z++)
        {
            for (int y = viewerChunkY - chunkUpdateRadius; y < viewerChunkY + chunkUpdateRadius; y++)
            {
                for (int x = viewerChunkX - chunkUpdateRadius; x < viewerChunkX + chunkUpdateRadius; x++)
                {
                    var chunkIndex = new Vector3Int(x, y, z);
                    if (octree.IsOctreeChunk(chunkIndex))
                    {
                        chunksInUpdateRadius.Add(chunkIndex);
                    }
                }
            }
        }

        UpdateChunks();
    }

    bool IsSurfaceChunk(Vector3Int chunkIndex)
    {
        float xComponent = (chunkIndex.x * sphereMeshSettings.chunkSize + sphereMeshSettings.chunkSize / 2);
        float yComponent = (chunkIndex.y * sphereMeshSettings.chunkSize + sphereMeshSettings.chunkSize / 2);
        float zComponent = (chunkIndex.z * sphereMeshSettings.chunkSize + sphereMeshSettings.chunkSize / 2);

        float surface = (xComponent * xComponent) + (yComponent * yComponent) + (zComponent * zComponent) - (sphereMeshSettings.radius * sphereMeshSettings.radius) + sphereMeshSettings.radius;
        float surfaceHeightOuter = surface - 1.5f * sphereMeshSettings.noiseHeightScale * sphereMeshSettings.radius - sphereMeshSettings.chunkSize * sphereMeshSettings.radius;
        float surfaceHeightInner = surface + 1.5f * sphereMeshSettings.noiseHeightScale * sphereMeshSettings.radius + sphereMeshSettings.chunkSize * sphereMeshSettings.radius;

        return surfaceHeightOuter <= 0 && surfaceHeightInner >= 0;
    }

    void UpdateChunks()
    {

        for (int i = 0; i < chunksInUpdateRadius.Count; i++)
        {
            Vector3Int chunkIndex = chunksInUpdateRadius[i];

            if (octree.GetNodeFromIndex(chunkIndex) == null)
            {
                Vector3 chunkOffset = new Vector3(chunkIndex.x * sphereMeshSettings.chunkSize, chunkIndex.y * sphereMeshSettings.chunkSize, chunkIndex.z * sphereMeshSettings.chunkSize);
                var chunk = new TerrainChunk(chunkOffset, transform, noiseSettings, sphereMeshSettings, material, viewer);

                octree.SetNodeFromIndex(chunkIndex, chunk);
                // Add the new chunk whether or not it will be visible, so we don't update currently visible chunks multiple times
                chunksVisibleLastUpdate.Add(chunkIndex);
            }
            else
            {
                if (!chunksVisibleLastUpdate.Contains(chunkIndex))
                {
                    chunksVisibleLastUpdate.Add(chunkIndex);
                }
            }

            //if (!chunksInDictionary.Contains(chunkIndex))
            //{
            //    Vector3 chunkOffset = new Vector3(chunkIndex.x * sphereMeshSettings.chunkSize, chunkIndex.y * sphereMeshSettings.chunkSize, chunkIndex.z * sphereMeshSettings.chunkSize);
            //    var chunk = new TerrainChunk(chunkOffset, transform, noiseSettings, sphereMeshSettings, material, viewer);
            //    chunksInDictionary.Add(chunkIndex);
            //    terrainChunksDictionary.Add(chunkIndex, chunk);
            //    // Add the new chunk whether or not it will be visible, so we don't update currently visible chunks multiple times
            //    chunksVisibleLastUpdate.Add(chunkIndex);
            //}
            //else
            //{
            //    if (!chunksVisibleLastUpdate.Contains(chunkIndex))
            //    {
            //        chunksVisibleLastUpdate.Add(chunkIndex);
            //    }
            //}

            //terrainChunksDictionary[chunkIndex].Update();

            //if (terrainChunksDictionary[chunkIndex].isActive)
            //{
            //    chunksVisibleLastUpdate.Add(chunkIndex);
            //}
        }

        List<Vector3Int> chunksToRemove = new List<Vector3Int>();
        foreach (var chunkIndex in chunksVisibleLastUpdate)
        {
            var chunk = octree.GetNodeFromIndex(chunkIndex);
            chunk.Update();

            if (!chunk.isActive)
            {
                chunksToRemove.Add(chunkIndex);
            }
        }

        foreach (var chunkIndex in chunksToRemove)
        {
            chunksVisibleLastUpdate.Remove(chunkIndex);
        }
    }

    void OnValidate()
    {
        noiseSettings.OnValidate();
        sphereMeshSettings.OnValidate();
    }
}
