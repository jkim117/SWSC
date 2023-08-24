using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenFunctions
{
    public static float mod(float x, float m)
    {
        return (x % m + m) % m;
    }
}
