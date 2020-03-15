using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class MeshGenerator
{

    public static MeshData GenerateSphereMesh(NoiseMapData mapData, SphereMeshSettings sphereSettings, int vertexIncrement)
    {
        int mcExpand = Mathf.Min(Mathf.CeilToInt(sphereSettings.noiseHeightScale * sphereSettings.radius * 3) * 2 + 2, 59);
        int halfMCExpand = mcExpand / 2;
        Vector3 addOffset = new Vector3(-halfMCExpand, -halfMCExpand, -halfMCExpand);
        MarchingCubes mc = new MarchingCubes(sphereSettings.chunkSize / vertexIncrement + mcExpand, sphereSettings.chunkSize / vertexIncrement + mcExpand, sphereSettings.chunkSize / vertexIncrement + mcExpand);
        mc.init_all();
        mc = computeSphere(mc, mapData.noiseMap, sphereSettings.noiseHeightScale, sphereSettings.radius, mapData.offset, vertexIncrement, sphereSettings.chunkSize, halfMCExpand);
        mc.run();
        mc.clean_temps();

        MeshData meshData = new MeshData();
        meshData.vertices = VertexToVector3Vertices(mc.vertices, mc.nverts(), mapData.offset + addOffset, vertexIncrement);
        meshData.normals = VertexToVector3Normals(mc.vertices, mc.nverts());
        //meshData.uv = NormalsToUVs(mesh.normals);
        meshData.uv = VerticesToUVs(meshData.vertices);
        meshData.triangles = TrianglesToInt(mc.triangles, mc.ntrigs());

        return meshData;
    }

    public static MeshData GenerateSphereChunkMesh(NoiseMapData noiseMap, SphereMeshSettings sphereSettings, int lod)
    {
        int vertexIncrement = sphereSettings.GetVertexIncrement(lod);

        MarchingCubes mc = new MarchingCubes(sphereSettings.chunkSize / vertexIncrement + 1, sphereSettings.chunkSize / vertexIncrement + 1, sphereSettings.chunkSize / vertexIncrement + 1);
        mc.init_all();
        mc = computeSphereChunk(mc, noiseMap.noiseMap, sphereSettings.noiseHeightScale, sphereSettings.radius, noiseMap.offset, vertexIncrement, sphereSettings.chunkSize);
        mc.run();
        mc.clean_temps();

        //TerrainChunk.stopwatch = new System.Diagnostics.Stopwatch();
        //TerrainChunk.stopwatch.Start();

        MeshData meshData = new MeshData();
        meshData.vertices = VertexToVector3Vertices(mc.vertices, mc.nverts(), noiseMap.offset, vertexIncrement);
        meshData.normals = VertexToVector3Normals(mc.vertices, mc.nverts());
        //meshData.uv = NormalsToUVs(mesh.normals);
        meshData.uv = VerticesToUVs(meshData.vertices);
        meshData.triangles = TrianglesToInt(mc.triangles, mc.ntrigs());

        meshData.lod = lod;

        //TerrainChunk.stopwatch.Stop();
        //Debug.Log("Created MeshData in " + TerrainChunk.stopwatch.ElapsedMilliseconds / 1000f + " seconds");

        return meshData;
    }

    static Vector3[] VertexToVector3Vertices(Vertex[] vertices, int numVerts, int radius)
    {
        Vector3[] vectors = new Vector3[numVerts];
        for (int i = 0; i < numVerts; i++)
        {
            vectors[i] = new Vector3(vertices[i].x - radius, vertices[i].y - radius, vertices[i].z - radius);
        }
        return vectors;
    }

    static Vector3[] VertexToVector3Vertices(Vertex[] vertices, int numVerts, Vector3 offset, int vertexIncrement)
    {
        Vector3[] vectors = new Vector3[numVerts];
        for (int i = 0; i < numVerts; i++)
        {
            vectors[i] = new Vector3(vertices[i].x * vertexIncrement + offset.x, vertices[i].y * vertexIncrement + offset.y, vertices[i].z * vertexIncrement + offset.z);
        }
        return vectors;
    }

    static Vector3[] VertexToVector3Normals(Vertex[] vertices, int numVerts)
    {
        Vector3[] normals = new Vector3[numVerts];
        for (int i = 0; i < numVerts; i++)
        {
            normals[i] = new Vector3(vertices[i].nx, vertices[i].ny, vertices[i].nz);
        }
        return normals;
    }

    static Vector2[] NormalsToUVs(Vector3[] normals)
    {
        Vector2[] uvs = new Vector2[normals.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            uvs[i] = new Vector2(Mathf.Asin(normals[i].x) / Mathf.PI + 0.5f, Mathf.Asin(normals[i].y) / Mathf.PI + 0.5f);
        }
        return uvs;
    }

    static Vector2[] VerticesToUVs(Vector3[] vertices)
    {
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = (vertices[i] - Vector3.zero).normalized;
        }
        return uvs;
    }

    static int[] TrianglesToInt(Triangle[] triangles, int numTrigs)
    {
        int[] ints = new int[numTrigs * 3];
        for (int t = 0; t < numTrigs; t++)
        {
            ints[t * 3] = triangles[t].v1;
            ints[t * 3 + 1] = triangles[t].v2;
            ints[t * 3 + 2] = triangles[t].v3;
        }
        return ints;
    }

    static MarchingCubes computeSphere(MarchingCubes mc, float[,,] noiseMap, float heightScale, int bodyRadius, Vector3 offset, int vertexIncrement, int chunkSize, int halfMCExpand)
    {
        for (int z = 0; z < (chunkSize / vertexIncrement) + 1; z++)
        {
            for (int y = 0; y < (chunkSize / vertexIncrement) + 1; y++)
            {
                for (int x = 0; x < (chunkSize / vertexIncrement) + 1; x++)
                {
                    float xComponent = (x * vertexIncrement + offset.x);
                    float yComponent = (y * vertexIncrement + offset.y);
                    float zComponent = (z * vertexIncrement + offset.z);

                    float surface = (xComponent * xComponent) + (yComponent * yComponent) + (zComponent * zComponent) - (bodyRadius * bodyRadius) + bodyRadius;
                    mc.set_data(surface - heightScale * bodyRadius * noiseMap[x * vertexIncrement, y * vertexIncrement, z * vertexIncrement], x + halfMCExpand, y + halfMCExpand, z + halfMCExpand);
                }
            }
        }
        return mc;
    }

    static MarchingCubes computeSphereChunk(MarchingCubes mc, float[,,] noiseMap, float heightScale, int bodyRadius, Vector3 offset, int vertexIncrement, int chunkSize)
    {
        for (int z = 0; z < (chunkSize / vertexIncrement) + 1; z++)
        {
            for (int y = 0; y < (chunkSize / vertexIncrement) + 1; y++)
            {
                for (int x = 0; x < (chunkSize / vertexIncrement) + 1; x++)
                {
                    float xComponent = (x * vertexIncrement + offset.x);
                    float yComponent = (y * vertexIncrement + offset.y);
                    float zComponent = (z * vertexIncrement + offset.z);

                    float surface = (xComponent * xComponent) + (yComponent * yComponent) + (zComponent * zComponent) - (bodyRadius * bodyRadius) + bodyRadius;
                    mc.set_data(surface - heightScale * bodyRadius * noiseMap[x * vertexIncrement, y * vertexIncrement, z * vertexIncrement], x, y, z);
                }
            }
        }
        return mc;
    }
}
