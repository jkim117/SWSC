using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string shipToLoad;
    public bool rebelPlayer;

    public int difficulty;
    public int worldSeed;
    public bool[] airbaseValues;
    public int currentAirbase;

    public GameData()
    {
        shipToLoad = CrossSceneValues.shipToLoad;
        rebelPlayer = CrossSceneValues.isRebelPlayer();

        difficulty = CrossSceneValues.difficulty;
        worldSeed = CrossSceneValues.worldSeed;
        airbaseValues = new bool[NewWorldSetup.sectorDimension * NewWorldSetup.sectorDimension];
        CrossSceneValues.airbaseValues.CopyTo(airbaseValues, 0);
        currentAirbase = CrossSceneValues.currentAirbase;

    }
}
