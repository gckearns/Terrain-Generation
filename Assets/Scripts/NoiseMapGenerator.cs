using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public static class NoiseMapGenerator
{

    public static float[,,] GetNoiseMap3D(int sizeX, int sizeY, int sizeZ, Vector3 offset, NoiseSettings settings)
    {
        float[,,] noiseMap = new float[sizeX, sizeY, sizeZ];

        //OpenSimplexNoise[] noise = new OpenSimplexNoise[settings.numOctaves];
        //for (int i = 0; i < settings.numOctaves; i++)
        //{
        //    noise[i] = new OpenSimplexNoise(settings.seed + i);
        //}

        float minLocalNoiseHeight = float.MaxValue;
        float maxLocalNoiseHeight = float.MinValue;

        for (int z = 0; z < sizeZ; z++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    float frequency = 1f;
                    float amplitude = 1f;

                    float noiseValue = 0;

                    for (int i = 0; i < settings.numOctaves; i++)
                    {
                        frequency *= settings.lacunarity;
                        float sampleX = (x + offset.x) * frequency / settings.coordScale;
                        float sampleY = (y + offset.y) * frequency / settings.coordScale;
                        float sampleZ = (z + offset.z) * frequency / settings.coordScale;

                        //double noiseEval = noise[i].eval(sampleX, sampleY, sampleZ);
                        double noiseEval = settings.noiseOctaves[i].eval(sampleX, sampleY, sampleZ);

                        noiseValue += (float)noiseEval * amplitude;

                        amplitude *= settings.persistence;
                    }

                    if (noiseValue > maxLocalNoiseHeight)
                        maxLocalNoiseHeight = noiseValue;
                    else if (noiseValue < minLocalNoiseHeight)
                        minLocalNoiseHeight = noiseValue;
                    noiseMap[x, y, z] = noiseValue;
                }
            }
        }

        if (settings.isNormalized)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        // Normalize noise by clamping the range of height values between 0 and 1
                        noiseMap[x, y, z] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y, z]) + settings.normShift;
                    }
                }
            }
        }

        //Debug.Log("Noise: Min= " + minLocalNoiseHeight + " Max= " + maxLocalNoiseHeight);
        return noiseMap;
    }
}
