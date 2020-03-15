using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree
{

    SphereMeshSettings meshSettings;

    Vector3Int origin;
    int rootNodeRadius;

    Branch trunk;

    public bool IsOctreeChunk(Vector3Int chunkIndex)
    {
        
        if (Mathf.Abs(chunkIndex.x) <= trunk.nodesPerChild)
        {
            if (Mathf.Abs(chunkIndex.y) <= trunk.nodesPerChild)
            {
                if (Mathf.Abs(chunkIndex.z) <= trunk.nodesPerChild)
                {

                    return true;
                }
            }
        }

        return false;
    }

    public Octree(SphereMeshSettings meshSettings)
    {
        rootNodeRadius = Mathf.CeilToInt((meshSettings.radius + meshSettings.noiseHeightScale * 1.5f) / meshSettings.chunkSize);

        if (rootNodeRadius % 2 == 1)
        {
            rootNodeRadius++;
        }

        //origin = new Vector3Int(-rootNodeRadius * meshSettings.chunkSize, -rootNodeRadius * meshSettings.chunkSize, -rootNodeRadius * meshSettings.chunkSize);
        origin = new Vector3Int(-rootNodeRadius, -rootNodeRadius, -rootNodeRadius);

        trunk = new Branch(rootNodeRadius, origin, meshSettings);
    }

    public TerrainChunk GetNodeFromIndex(Vector3Int chunkIndex)
    {
        Branch branch = trunk;

        while (!branch.isLeafNode)
        {
            branch = branch.GetNodeFromChunkIndex(chunkIndex);
        }

        return branch.GetChunkFromIndex(chunkIndex);
    }

    public void SetNodeFromIndex(Vector3Int chunkIndex, TerrainChunk chunk)
    {
        Branch branch = trunk;

        while (!branch.isLeafNode)
        {
            branch = branch.GetNodeFromChunkIndex(chunkIndex);
        }

        branch.SetChunkFromIndex(chunkIndex, chunk);
    }

}

public class Branch
{
    SphereMeshSettings meshSettings;

    public Vector3Int origin;
    public int nodesPerChild;

    Branch[,,] nodes;
    TerrainChunk[,,] chunks;

    public bool isLeafNode;

    bool[,,] isInitialized;

    public Bounds bounds;

    public Branch(int nodesPerChild, Vector3Int trunkOrigin, SphereMeshSettings meshSettings)
    {
        this.nodesPerChild = nodesPerChild;
        this.meshSettings = meshSettings;
        origin = trunkOrigin;
        //Debug.Log("Created trunk with origin: " + origin + "nodesPerChild: " + nodesPerChild);

        int nodeDiameter = nodesPerChild * 2;
        bounds = new Bounds(trunkOrigin * meshSettings.chunkSize + (Vector3.one * meshSettings.chunkSize * nodesPerChild), (Vector3.one * meshSettings.chunkSize * nodeDiameter));

        GenerateChildren();
    }

    public Branch(Vector3Int nodeIndex, int nodesPerChild, Vector3Int parentOrigin, SphereMeshSettings meshSettings)
    {
        this.nodesPerChild = nodesPerChild;
        this.meshSettings = meshSettings;
        origin = parentOrigin + (nodeIndex * 2 * nodesPerChild);
        //origin = parentOrigin + (nodeIndex * 2 * nodesPerChild * meshSettings.chunkSize);
        //Debug.Log("Created branch " + nodeIndex + "with origin: " + origin + "nodesPerChild: " + nodesPerChild);

        int nodeDiameter = nodesPerChild * 2;
        bounds = new Bounds(origin * meshSettings.chunkSize + (Vector3.one * meshSettings.chunkSize * nodesPerChild), (Vector3.one * meshSettings.chunkSize * nodeDiameter));

        GenerateChildren();
    }

    void GenerateChildren()
    {
        if (nodesPerChild != 1)
        {
            nodes = new Branch[2, 2, 2];
            //Debug.Log("Created child branches for origin: " + origin + "nodesPerChild: " + nodesPerChild);
        }
        else
        {
            isLeafNode = true;

            chunks = new TerrainChunk[2, 2, 2];
            isInitialized = new bool[2, 2, 2];
            //Debug.Log("Created child leaves for origin: " + origin + "nodesPerChild: " + nodesPerChild);
        }

        for (int z = 0; z < 2; z++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    if (nodesPerChild != 1)
                    {
                        nodes[x, y, z] = new Branch(new Vector3Int(x, y, z), nodesPerChild / 2, origin, meshSettings);
                    }
                }
            }
        }
    }

    //public TerrainChunk GetChunk(Vector3Int nodeIndex)
    //{
    //    //Vector3 inputMod = nodeIndex - origin;
    //    //Vector3Int nodeIndex = Vector3Int.FloorToInt(inputMod / nodesPerChild);

    //    if (isInitialized[nodeIndex.x, nodeIndex.y, nodeIndex.z])
    //    {
    //        return chunks[nodeIndex.x, nodeIndex.y, nodeIndex.z];
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

    public TerrainChunk GetChunkFromIndex(Vector3Int chunkIndex)
    {
        Vector3 inputMod = chunkIndex - origin;
        Vector3Int nodeIndex = Vector3Int.FloorToInt(inputMod / nodesPerChild);

        if (isInitialized[nodeIndex.x, nodeIndex.y, nodeIndex.z])
        {
            return chunks[nodeIndex.x, nodeIndex.y, nodeIndex.z];
        }
        else
        {
            return null;
        }
    }

    public void SetChunkFromIndex(Vector3Int chunkIndex, TerrainChunk chunk)
    {
        Vector3 inputMod = chunkIndex - origin;
        Vector3Int nodeIndex = Vector3Int.FloorToInt(inputMod / nodesPerChild);

        chunks[nodeIndex.x, nodeIndex.y, nodeIndex.z] = chunk;

        isInitialized[nodeIndex.x, nodeIndex.y, nodeIndex.z] = true;
    }

    //public Branch GetNode(Vector3Int nodeIndex)
    //{
    //    return nodes[nodeIndex.x, nodeIndex.y, nodeIndex.z];
    //}

    public Branch GetNodeFromChunkIndex(Vector3Int chunkIndex)
    {
        Vector3 inputMod = chunkIndex - origin;
        Vector3Int nodeIndex = Vector3Int.FloorToInt(inputMod / nodesPerChild);

        return nodes[nodeIndex.x, nodeIndex.y, nodeIndex.z];
    }
}