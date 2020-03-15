using UnityEngine;

[System.Serializable]
public struct NoiseSettings
{
    public float coordScale;

    public int numOctaves;
    public float lacunarity;
    [Range(0, 1)]
    public float persistence;

    public long seed;

    public bool isNormalized;

    public float normShift;

    private OpenSimplexNoise[] _simplexNoiseOctaves;
    public OpenSimplexNoise[] noiseOctaves
    {
        get
        {
            if (_simplexNoiseOctaves != null)
            {
                return _simplexNoiseOctaves;
            }
            else
            {
                _simplexNoiseOctaves = new OpenSimplexNoise[numOctaves];
                for (int i = 0; i < numOctaves; i++)
                {
                    _simplexNoiseOctaves[i] = new OpenSimplexNoise(seed + i);
                }
                return _simplexNoiseOctaves;
            }
        }
    }

    public void OnValidate()
    {
        coordScale = Mathf.Max(coordScale, 0.01f);
        numOctaves = Mathf.Max(numOctaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }

}