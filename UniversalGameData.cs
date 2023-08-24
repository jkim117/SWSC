using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UniversalGameData
{
    public int numGaveSaves;
    public bool xInvert;
    public bool yInvert;
    public float sensitivity;

    public UniversalGameData(int gameSaves)
    {
        numGaveSaves = gameSaves;
        xInvert = CrossSceneValues.invertX;
        yInvert = CrossSceneValues.invertY;
        sensitivity = CrossSceneValues.sensitivity;
    }
}
