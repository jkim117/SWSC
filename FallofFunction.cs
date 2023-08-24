using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FallofFunction
{
    public static float GenerateFalloff(Vector2 chunkCoord, Vector2 localCoord)
    {
        float size = EndlessTerrain.chunkWorldLimit * (MeshGenerator.chunkSize); //10000

        //5000 - 10000
        float x = Mathf.Abs((size / 2f) -  (chunkCoord.x * (MeshGenerator.chunkSize) + localCoord.x));
        float y = Mathf.Abs((size / 2f) - (chunkCoord.y * (MeshGenerator.chunkSize) + localCoord.y));

        //float distance = Mathf.Sqrt(x * x + y * y);

        // Find distance form center point of world to current point. Map values from 0 to 1. 
        // Center is 1, farthest is 0. That value is to be entered in evaluate function
        /*if (localCoord == Vector2.one)
        {
            Debug.Log(Mathf.InverseLerp(0, size / 2f, distance));
        }*/

        //return Evaluate(Mathf.InverseLerp(0, size / 2f, distance));
        return Evaluate(Mathf.InverseLerp(0, size / 2f, Mathf.Max(x, y)));
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 10f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
