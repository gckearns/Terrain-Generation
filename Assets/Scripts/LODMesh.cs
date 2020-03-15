using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODMesh
{
    //public int lod;
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;

    NoiseMapData mapData;
    SphereMeshSettings meshSettings;

    //public LODMesh(int lod, SphereMeshSettings meshSettings)
    //{
    //    this.lod = lod;
    //    this.meshSettings = meshSettings;
    //}

    //public void RequestMesh(NoiseMapData mapData)
    //{
    //    this.mapData = mapData;
    //    hasRequestedMesh = true;
    //    UnityTaskManager.ScheduleTask(RequestMeshData, OnMeshDataReceived);
    //}

    //object RequestMeshData()
    //{
    //    return MeshGenerator.GenerateSphereChunkMesh(mapData, meshSettings, lod);
    //}

    //void OnMeshDataReceived(object meshData)
    //{
    //    mesh = ((MeshData)meshData).GetMesh();
    //    hasMesh = true;
    //}
}
