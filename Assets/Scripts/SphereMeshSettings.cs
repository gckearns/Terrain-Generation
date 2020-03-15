using UnityEngine;

[System.Serializable]
public struct SphereMeshSettings
{

    [IntArraySlider(new int[] { 16, 32, 48, 64 })]
    public int chunkSizeIndex;

    public int radius;
    public float noiseHeightScale;

    [Range(0,4)]
    public int colliderLODIndex;
    public LODSettings[] lodSettings;

    static readonly int[] validChunkSizes = { 16, 32, 48, 64 };
    public static readonly int[] validVertexIncrements = { 1, 2, 4, 8, 16 };

    public int chunkSize
    {
        get
        {
            return validChunkSizes[chunkSizeIndex];
        }
    }

    public int GetVertexIncrement(int lod)
    {
        return validVertexIncrements[lod];
    }

    public void OnValidate()
    {
        radius = Mathf.Max(radius, 1);
        noiseHeightScale = Mathf.Max(noiseHeightScale, 0);

        if (lodSettings != null)
        {
            if (lodSettings.Length == validVertexIncrements.Length)
            {
                for (int i = 0; i < lodSettings.Length; i++)
                {
                    if (lodSettings[i].vertexIncrement != GetVertexIncrement(i))
                    {
                        lodSettings[i].vertexIncrement = GetVertexIncrement(i);
                    }
                }
            }
            else
            {
                lodSettings = new LODSettings[validVertexIncrements.Length];
                for (int i = 0; i < lodSettings.Length; i++)
                {
                    lodSettings[i] = new LODSettings(GetVertexIncrement(i), i * (chunkSize / 2) + chunkSize);
                }
            }
        }
    }
}
