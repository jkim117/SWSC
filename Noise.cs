using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, Vector2 chunkCoord)
    {
        float[] noiseMap = new float[(mapWidth + 1) * (mapHeight + 1)];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int i = 0, y = 0; y <= mapHeight; y++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int j = 0; j < octaves; j++)
                {
                    //float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[j].x * frequency;
                    //float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[j].y * frequency;
                    float sampleX = (x - halfWidth + octaveOffsets[j].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[j].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    //float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                    
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[i] = noiseHeight;
                i++;

            }
        }

        for (int i = 0, y = 0; y <= mapHeight; y++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                float normalizedHeight = (noiseMap[i] + 1) / (2f * maxPossibleHeight / 1.5f);
                //float normalizedHeight = noiseMap[i];
                float falloff = FallofFunction.GenerateFalloff(chunkCoord, new Vector2(x, y));
                //falloff = 0;
                /*if (x == 0 && y == 0)
                {
                    Debug.Log(falloff);
                }*/
                if (normalizedHeight - falloff < 0)
                {
                    noiseMap[i] = 0;
                }
                else
                {
                    noiseMap[i] = normalizedHeight - falloff;
                }
                //noiseMap[i] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[i]);
                i++;
            }
        }
        
        return noiseMap;
    }
}
