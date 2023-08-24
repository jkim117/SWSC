using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGameAssets : MonoBehaviour
{
    // 0 rebel airbase
    // 1 imperial airbase
    // 2 AI X-Wing
    // 3 AI Tie LN
    // 4 AI MC80A
    // 5 AI ISD2
    public GameObject[] assetPrefabs;
    // 0 X-Wing
    // 1 A-Wing
    // 2 Y-Wing
    // 3 B-Wing
    // 4 Tie LN
    // 5 Tie IN
    // 6 Tie SA
    // 7 Tie D
    public GameObject[] playerShipObjects;
    public GameObject shieldHUD;
    public GameObject noShieldHUD;
    public static float despawnThreshold = 40000;
    public static bool[] airbaseSpawnStatus = new bool[NewWorldSetup.sectorDimension * NewWorldSetup.sectorDimension];
    private HUDManager hudManager;
    private GameObject player;
    private PlayerController pc;

    public static Vector3 calculateAirbaseUnityCoord(int sector)
    {
        int sectorRow = sector / NewWorldSetup.sectorDimension;
        int sectorCol = sector - sectorRow * NewWorldSetup.sectorDimension;
        Vector3 airbaseWorldCoord = NewWorldSetup.airbaseCoordinates[sectorCol, sectorRow];

        Vector2 airbaseUnityTemp = EndlessTerrain.worldToUnityCoord(airbaseWorldCoord.x, airbaseWorldCoord.z);
        Vector3 airbaseUnityCoord = new Vector3(airbaseUnityTemp.x, airbaseWorldCoord.y, airbaseUnityTemp.y);
        return airbaseUnityCoord;
    }

    public GameObject swapPlayerShip()
    {
        pc.destroyShipController();
        Object.Destroy(hudManager.gameObject.transform.parent.gameObject);

        if (CrossSceneValues.shipToLoad == "X")
        {
            CrossSceneValues.shipToLoad = "A";
        }
        else if (CrossSceneValues.shipToLoad == "A")
        {
            CrossSceneValues.shipToLoad = "Y";
        }
        else if (CrossSceneValues.shipToLoad == "Y")
        {
            CrossSceneValues.shipToLoad = "B";
        }
        else if (CrossSceneValues.shipToLoad == "B")
        {
            CrossSceneValues.shipToLoad = "X";
        }
        else if (CrossSceneValues.shipToLoad == "LN")
        {
            CrossSceneValues.shipToLoad = "IN";
        }
        else if (CrossSceneValues.shipToLoad == "IN")
        {
            CrossSceneValues.shipToLoad = "SA";
        }
        else if (CrossSceneValues.shipToLoad == "SA")
        {
            CrossSceneValues.shipToLoad = "D";
        }
        else if (CrossSceneValues.shipToLoad == "D")
        {
            CrossSceneValues.shipToLoad = "LN";
        }
        return spawnPlayer();
    }

    public GameObject spawnPlayer()
    {
        int shipIndex = 0;

        if (CrossSceneValues.shipToLoad == "X")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 0;
        }
        else if (CrossSceneValues.shipToLoad == "A")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 1;
        }
        else if (CrossSceneValues.shipToLoad == "Y")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 2;
        }
        else if (CrossSceneValues.shipToLoad == "B")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 3;
        }
        else if (CrossSceneValues.shipToLoad == "LN")
        {
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 4;
        }
        else if (CrossSceneValues.shipToLoad == "IN")
        {
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 5;
        }
        else if (CrossSceneValues.shipToLoad == "SA")
        {
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 6;
        }
        else if (CrossSceneValues.shipToLoad == "D")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 7;
        }

        Vector3 airbaseUnityCoord = calculateAirbaseUnityCoord(CrossSceneValues.currentAirbase);
        if (shipIndex == 3) // B-Wing
        {
            Vector3 playerSpawnOffset = new Vector3(153, -8, -160);
            player = GameObject.Instantiate(playerShipObjects[shipIndex], airbaseUnityCoord + playerSpawnOffset, Quaternion.Euler(-90, 0, 0));
        }
        else if (shipIndex < 4)
        {
            Vector3 playerSpawnOffset = new Vector3(153, 0, -160);
            player = GameObject.Instantiate(playerShipObjects[shipIndex], airbaseUnityCoord + playerSpawnOffset, Quaternion.Euler(-90, 0, 0));
        }
        else
        {
            Vector3 playerSpawnOffset = new Vector3(16.6f, 28, -83);
            player = GameObject.Instantiate(playerShipObjects[shipIndex], airbaseUnityCoord + playerSpawnOffset, Quaternion.Euler(-90, 0, 180));
        }
        pc = player.GetComponent<PlayerController>();
        hudManager.shipController = pc;
        CrossSceneValues.player = pc;

        return player;
    }
    public void spawnAirbase(bool rebelBase, bool rebelPlayer, int sector)
    {
        AirbaseController airbaseController;
        AIController fighter1;
        AIController fighter2;
        AIController fighter3;
        AIController fighter4;
        ShipController capShipController;

        AIController enFighter1;
        AIController enFighter2;
        AIController enFighter3;
        AIController enFighter4;
        ShipController enCapShipController;

        Vector3 airbaseUnityCoord = calculateAirbaseUnityCoord(sector);

        // Airbase we are trying to spawn is too far away
        if (Vector2.Distance(Vector2.zero, new Vector2(airbaseUnityCoord.x, airbaseUnityCoord.z)) > despawnThreshold)
        {
            return;
        }

        if (rebelBase)
        {
            airbaseController = GameObject.Instantiate(assetPrefabs[0], airbaseUnityCoord, Quaternion.Euler(-90, 0, 0)).GetComponent<AirbaseController>();
            fighter1 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x + 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            fighter2 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x - 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            fighter3 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            fighter4 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x - 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            capShipController = GameObject.Instantiate(assetPrefabs[4], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 800, airbaseUnityCoord.z + 1000), Quaternion.Euler(-90, 0, 0)).GetComponent<ShipController>();

            enFighter1 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x + 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z + 10000), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            enFighter2 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x - 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z + 10000), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            enFighter3 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z + 10000), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            enFighter4 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x - 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z + 10000), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            enCapShipController = GameObject.Instantiate(assetPrefabs[5], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 800, airbaseUnityCoord.z + 10000), Quaternion.Euler(-90, 0, -180)).GetComponent<ShipController>();
        }
        else
        {
            airbaseController = GameObject.Instantiate(assetPrefabs[1], airbaseUnityCoord, Quaternion.Euler(-90, 0, 0)).GetComponent<AirbaseController>();
            fighter1 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x + 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            fighter2 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x - 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            fighter3 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            fighter4 = GameObject.Instantiate(assetPrefabs[3], new Vector3(airbaseUnityCoord.x - 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z), Quaternion.Euler(-90, 0, -180)).GetComponent<AIController>();
            capShipController = GameObject.Instantiate(assetPrefabs[5], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 800, airbaseUnityCoord.z + 1000), Quaternion.Euler(-90, 0, -180)).GetComponent<ShipController>();

            enFighter1 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x + 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z - 10000), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            enFighter2 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x - 500, airbaseUnityCoord.y + 300, airbaseUnityCoord.z - 10000), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            enFighter3 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z - 10000), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            enFighter4 = GameObject.Instantiate(assetPrefabs[2], new Vector3(airbaseUnityCoord.x - 1000, airbaseUnityCoord.y + 300, airbaseUnityCoord.z - 10000), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            enCapShipController = GameObject.Instantiate(assetPrefabs[4], new Vector3(airbaseUnityCoord.x + 1000, airbaseUnityCoord.y + 800, airbaseUnityCoord.z - 10000), Quaternion.Euler(-90, 0, 0)).GetComponent<ShipController>();
        }

        if (rebelBase == rebelPlayer)
        {
            airbaseController.friendly = true;
            fighter1.friendly = true;
            fighter2.friendly = true;
            fighter3.friendly = true;
            fighter4.friendly = true;
            capShipController.friendly = true;
            fighter1.parentCapShip = capShipController.GetComponent<CapShipController>();
            fighter2.parentCapShip = capShipController.GetComponent<CapShipController>();
            fighter3.parentCapShip = capShipController.GetComponent<CapShipController>();
            fighter4.parentCapShip = capShipController.GetComponent<CapShipController>();

            enFighter1.friendly = false;
            enFighter2.friendly = false;
            enFighter3.friendly = false;
            enFighter4.friendly = false;
            enCapShipController.friendly = false;
            enFighter1.parentCapShip = enCapShipController.GetComponent<CapShipController>();
            enFighter2.parentCapShip = enCapShipController.GetComponent<CapShipController>();
            enFighter3.parentCapShip = enCapShipController.GetComponent<CapShipController>();
            enFighter4.parentCapShip = enCapShipController.GetComponent<CapShipController>();
        }
        else
        {
            airbaseController.friendly = false;
            fighter1.friendly = false;
            fighter2.friendly = false;
            fighter3.friendly = false;
            fighter4.friendly = false;
            capShipController.friendly = false;
            fighter1.parentCapShip = capShipController.GetComponent<CapShipController>();
            fighter2.parentCapShip = capShipController.GetComponent<CapShipController>();
            fighter3.parentCapShip = capShipController.GetComponent<CapShipController>();
            fighter4.parentCapShip = capShipController.GetComponent<CapShipController>();

            enFighter1.friendly = true;
            enFighter2.friendly = true;
            enFighter3.friendly = true;
            enFighter4.friendly = true;
            enCapShipController.friendly = true;
            enFighter1.parentCapShip = enCapShipController.GetComponent<CapShipController>();
            enFighter2.parentCapShip = enCapShipController.GetComponent<CapShipController>();
            enFighter3.parentCapShip = enCapShipController.GetComponent<CapShipController>();
            enFighter4.parentCapShip = enCapShipController.GetComponent<CapShipController>();
        }
        airbaseController.sector = sector;
        airbaseSpawnStatus[sector] = true;
    }
}
