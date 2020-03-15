using UnityEngine;

public struct NoiseMapData
{

    public float[,,] noiseMap;
    public Vector3 offset;

    public NoiseMapData(float[,,] noiseMap, Vector3 offset)
    {
        this.noiseMap = noiseMap;
        this.offset = offset;
    }
}
