using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CrossSceneValues
{
    public static string shipToLoad = "X";
    //public static bool loadFighters = false;
    public static bool tutorialMode = false;
    public static bool tutorialActive = false;
    // 0 kessel, 1 korriban, 2 exegol, 3 kashyyk, 4 geonosis, 5 jedha
    public static string currentPlanet = "";
    public static bool inGameMenu = false;
    public static PlayerController player;

    public static List<List<ShipController>> targetList = new List<List<ShipController>>();
    public static List<ShipController> enemyFighterList = new List<ShipController>();
    public static List<ShipController> enemyCapitalShipTargetList = new List<ShipController>();
    public static List<ShipController> friendlyFighterList = new List<ShipController>();
    public static List<ShipController> friendlyCapitalShipTargetList = new List<ShipController>();
    public static List<ShipController> objectiveList = new List<ShipController>();
    public static List<ShipController> projectilesList = new List<ShipController>();

    public static List<List<ShipController>> targetList_enemy = new List<List<ShipController>>();
    public static List<ShipController> enemyFighterList_enemy = new List<ShipController>();
    public static List<ShipController> enemyCapitalShipTargetList_enemy = new List<ShipController>();
    public static List<ShipController> friendlyFighterList_enemy = new List<ShipController>();
    public static List<ShipController> friendlyCapitalShipTargetList_enemy = new List<ShipController>();
    public static List<ShipController> objectiveList_enemy = new List<ShipController>();
    public static List<ShipController> projectilesList_enemy = new List<ShipController>();

    public static int difficulty = 0; // 0 easy, 1 medium, 2 hard
    public static int worldSeed;
    public static float camStatus;
    public static int currentAirbase;
    public static bool[] airbaseValues = new bool[NewWorldSetup.sectorDimension * NewWorldSetup.sectorDimension];

    public static float sensitivity = 1f; // from 0.5 to 1.5
    public static bool invertX = false;
    public static bool invertY = false;

    public static bool isRebelPlayer()
    {
        switch (shipToLoad)
        {
            case "X":
                return true;
            case "Y":
                return true;
            case "A":
                return true;
            case "B":
                return true;
            case "LN":
                return false;
            case "IN":
                return false;
            case "SA":
                return false;
            case "D":
                return false;
            default:
                return true;
        }
    }
}
