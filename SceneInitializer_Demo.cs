using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class SceneInitializer_Demo : MonoBehaviour
{
    public GameObject[] playerShipObjects;
    public GameObject tiePrefab;
    public GameObject xPrefab;
    public GameObject isdPrefab;
    public GameObject mca80aPrefab;
    public GameObject platformPrefab;
    public GameObject base1Prefab;
    public GameObject shieldHUD;
    public GameObject noShieldHUD;

    private GameObject player;
    private PlayerController pc;
    private AIController friendlyAI0;
    private AIController friendlyAI1;
    private AIController friendlyAI2;
    private AIController enAI0;
    private AIController enAI1;
    private AIController enAI2;
    private AIController enAI3;
    private ShipController capShip;
    private ShipController enCapShip;
    private StaticObjectiveController obj;
    private ShipController platform;
    private HUDManager hudManager;

    private bool rebelPlayerBool;

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineVirtualCamera rearVirtualCamera;

    private Vector3 playerStartPositionWorldCoord;

    void setUpRebelScene()
    {
        capShip = GameObject.Find("CRSIndependence").GetComponent<ShipController>();
        enCapShip = GameObject.Find("ISDAvenger").GetComponent<ShipController>();
        /*if (CrossSceneValues.loadFighters)
        {
            spawnFighters(rebelPlayerBool);
        }*/
        
    }

    void setUpImpScene()
    {
        capShip = GameObject.Find("ISDAvenger").GetComponent<ShipController>();
        enCapShip = GameObject.Find("CRSIndependence").GetComponent<ShipController>();
        /*if (CrossSceneValues.loadFighters)
        {
            spawnFighters(rebelPlayerBool);
        }*/
    }

    void playerCameraSetUp()
    {
        CinemachineVirtualCamera vc = GameObject.Find("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        vc.Follow = player.transform.Find("FollowTarget");
        vc.LookAt = player.transform;

        CinemachineVirtualCamera rearvc = GameObject.Find("Rear Camera").GetComponent<CinemachineVirtualCamera>();
        rearvc.Follow = player.transform.Find("FollowTargetRear");
        rearvc.LookAt = player.transform;

        virtualCamera = vc;
        rearVirtualCamera = rearvc;
    }

    public void SwapShip()
    {
        CrossSceneValues.friendlyFighterList.Remove(pc);
        Destroy(player);
        Destroy(hudManager.gameObject.transform.parent.gameObject);

        int shipIndex = 0;
        bool rebelPlayer = true;
        if (CrossSceneValues.shipToLoad == "X")
        {
            CrossSceneValues.shipToLoad = "A";
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 1;
        }
        else if (CrossSceneValues.shipToLoad == "A")
        {
            CrossSceneValues.shipToLoad = "Y";
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 2;
        }
        else if (CrossSceneValues.shipToLoad == "Y")
        {
            CrossSceneValues.shipToLoad = "B";
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 3;
        }
        else if (CrossSceneValues.shipToLoad == "B")
        {
            CrossSceneValues.shipToLoad = "X";
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 0;
        }
        else if (CrossSceneValues.shipToLoad == "LN")
        {
            CrossSceneValues.shipToLoad = "IN";
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 5;
            rebelPlayer = false;
        }
        else if (CrossSceneValues.shipToLoad == "IN")
        {
            CrossSceneValues.shipToLoad = "SA";
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 6;
            rebelPlayer = false;
        }
        else if (CrossSceneValues.shipToLoad == "SA")
        {
            CrossSceneValues.shipToLoad = "D";
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 7;
            rebelPlayer = false;
        }
        else if (CrossSceneValues.shipToLoad == "D")
        {
            CrossSceneValues.shipToLoad = "LN";
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            shipIndex = 4;
            rebelPlayer = false;
        }

        Vector2 startPosTemp = EndlessTerrain.worldToUnityCoord(playerStartPositionWorldCoord.x, playerStartPositionWorldCoord.z);
        if (shipIndex == 3)
        {
            player = GameObject.Instantiate(playerShipObjects[shipIndex], new Vector3(startPosTemp.x, playerStartPositionWorldCoord.y - 8, startPosTemp.y), Quaternion.Euler(-90, 0, 0));
        }
        else if (rebelPlayer)
        {
            player = GameObject.Instantiate(playerShipObjects[shipIndex], new Vector3(startPosTemp.x, playerStartPositionWorldCoord.y, startPosTemp.y), Quaternion.Euler(-90, 0, 0));
        }
        else
        {
            player = GameObject.Instantiate(playerShipObjects[shipIndex], new Vector3(startPosTemp.x, playerStartPositionWorldCoord.y, startPosTemp.y), Quaternion.Euler(-90, 0, 180));
        }
        pc = player.GetComponent<PlayerController>();

        
        pc.onReset += worldShift;

        //obj.player = pc;
        playerCameraSetUp();
        EndlessTerrain et = GameObject.Find("TerrainGenerator").GetComponent<EndlessTerrain>();
        et.viewer = player.transform;
        et.subscribeToViewerEvent();

        hudManager.shipController = pc;
        GameObject.Find("PauseMenu").GetComponent<PauseMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        GameObject.Find("InGameMenu").GetComponent<InGameMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        GameObject.Find("TutorialPrompts").GetComponent<TutorialScript>().playerInput = player.GetComponent<PlayerInput>();
    }

    void setUpScene(int shipIndex, bool rebelPlayer)
    {
        if (shipIndex == 3)
        {
            Vector2 worldCoordTemp = EndlessTerrain.unityToWorldCoord(-556, 110);
            playerStartPositionWorldCoord = new Vector3(worldCoordTemp.x, 456, worldCoordTemp.y);
            player = GameObject.Instantiate(playerShipObjects[shipIndex], new Vector3(-556, 456, 110), Quaternion.Euler(-90, 0, 0));
        }
        else if (rebelPlayer)
        {
            Vector2 worldCoordTemp = EndlessTerrain.unityToWorldCoord(-556, 110);
            playerStartPositionWorldCoord = new Vector3(worldCoordTemp.x, 464, worldCoordTemp.y);
            player = GameObject.Instantiate(playerShipObjects[shipIndex], new Vector3(-556, 464, 110), Quaternion.Euler(-90, 0, 0));
        }
        else
        {
            Vector2 worldCoordTemp = EndlessTerrain.unityToWorldCoord(674.6f, 14148);
            playerStartPositionWorldCoord = new Vector3(worldCoordTemp.x, 1627, worldCoordTemp.y);
            player = GameObject.Instantiate(playerShipObjects[shipIndex], new Vector3(674.6f, 1627, 14148), Quaternion.Euler(-90, 0, 180));
        }
        rebelPlayerBool = rebelPlayer;
        obj = GameObject.Find("EnemyBase").GetComponent<StaticObjectiveController>();
        
        platform = GameObject.Find("XQ6 Platform").GetComponent<ShipController>();

        pc = player.GetComponent<PlayerController>();

        //obj.player = pc;
        obj.friendly = false;

        if (rebelPlayer)
        {
            setUpRebelScene();
        }
        else
        {
            setUpImpScene();
        }
        capShip.friendly = true;
        enCapShip.friendly = false;
        platform.friendly = true;
        obj.friendly = false;
        
        CrossSceneValues.friendlyCapitalShipTargetList.Add(platform);
        CrossSceneValues.friendlyCapitalShipTargetList_enemy.Add(platform);
        CrossSceneValues.objectiveList.Add(obj);
        CrossSceneValues.objectiveList_enemy.Add(obj);

        playerCameraSetUp();
        GameObject.Find("TerrainGenerator").GetComponent<EndlessTerrain>().viewer = player.transform;
        hudManager.shipController = pc;
        GameObject.Find("PauseMenu").GetComponent<PauseMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        GameObject.Find("InGameMenu").GetComponent<InGameMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        GameObject.Find("TutorialPrompts").GetComponent<TutorialScript>().playerInput = player.GetComponent<PlayerInput>();
    }

    void despawnFighter()
    {
        Destroy(friendlyAI0);
        Destroy(friendlyAI1);
        Destroy(friendlyAI2);
        Destroy(enAI0);
        Destroy(enAI1);
        Destroy(enAI2);
        Destroy(enAI3);
    }

    void spawnFighters(bool rebelPlayer)
    {
        if (rebelPlayer)
        {
            friendlyAI0 = GameObject.Instantiate(xPrefab, new Vector3(-723, 700, -1190), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            friendlyAI1 = GameObject.Instantiate(xPrefab, new Vector3(-1300, 700, -1190), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            friendlyAI2 = GameObject.Instantiate(xPrefab, new Vector3(-200, 700, -1190), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            friendlyAI0.friendly = true;
            friendlyAI1.friendly = true;
            friendlyAI2.friendly = true;

            enAI0 = GameObject.Instantiate(tiePrefab, new Vector3(501, 700, 11654), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();
            enAI1 = GameObject.Instantiate(tiePrefab, new Vector3(-442, 700, 11654), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();
            enAI2 = GameObject.Instantiate(tiePrefab, new Vector3(1000, 700, 11654), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();
            enAI3 = GameObject.Instantiate(tiePrefab, new Vector3(1500, 700, 11654), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();   
        }
        else
        {
            friendlyAI0 = GameObject.Instantiate(tiePrefab, new Vector3(1500, 700, 11654), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();
            friendlyAI1 = GameObject.Instantiate(tiePrefab, new Vector3(1000, 700, 11654), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();
            friendlyAI2 = GameObject.Instantiate(tiePrefab, new Vector3(501, 700, 11654), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();
            friendlyAI0.friendly = true;
            friendlyAI1.friendly = true;
            friendlyAI2.friendly = true;

            enAI0 = GameObject.Instantiate(xPrefab, new Vector3(-723, 700, -1190), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            enAI1 = GameObject.Instantiate(xPrefab, new Vector3(-1300, 700, -1190), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            enAI2 = GameObject.Instantiate(xPrefab, new Vector3(-200, 700, -1190), Quaternion.Euler(-90, 0, 0)).GetComponent<AIController>();
            enAI3 = GameObject.Instantiate(xPrefab, new Vector3(200, 700, -1190), Quaternion.Euler(-90, 0, 180)).GetComponent<AIController>();
        }

        

    }

    // Start is called before the first frame update
    void Start()
    {
        CrossSceneValues.worldSeed = 0; // specific to the demo scene
        PlayerController.totalxShift = -20000;
        PlayerController.totalzShift = -20000;

        // reset all target lists
        CrossSceneValues.targetList = new List<List<ShipController>>();
        CrossSceneValues.enemyFighterList = new List<ShipController>();
        CrossSceneValues.enemyCapitalShipTargetList = new List<ShipController>();
        CrossSceneValues.friendlyFighterList = new List<ShipController>();
        CrossSceneValues.friendlyCapitalShipTargetList = new List<ShipController>();
        CrossSceneValues.objectiveList = new List<ShipController>();
        CrossSceneValues.projectilesList = new List<ShipController>();

        CrossSceneValues.targetList.Add(CrossSceneValues.enemyFighterList);
        CrossSceneValues.targetList.Add(CrossSceneValues.enemyCapitalShipTargetList);
        CrossSceneValues.targetList.Add(CrossSceneValues.friendlyFighterList);
        CrossSceneValues.targetList.Add(CrossSceneValues.friendlyCapitalShipTargetList);
        CrossSceneValues.targetList.Add(CrossSceneValues.objectiveList);
        CrossSceneValues.targetList.Add(CrossSceneValues.projectilesList);

        CrossSceneValues.targetList_enemy = new List<List<ShipController>>();
        CrossSceneValues.enemyFighterList_enemy = new List<ShipController>();
        CrossSceneValues.enemyCapitalShipTargetList_enemy = new List<ShipController>();
        CrossSceneValues.friendlyFighterList_enemy = new List<ShipController>();
        CrossSceneValues.friendlyCapitalShipTargetList_enemy = new List<ShipController>();
        CrossSceneValues.objectiveList_enemy = new List<ShipController>();
        CrossSceneValues.projectilesList_enemy = new List<ShipController>();

        CrossSceneValues.targetList_enemy.Add(CrossSceneValues.enemyFighterList_enemy);
        CrossSceneValues.targetList_enemy.Add(CrossSceneValues.enemyCapitalShipTargetList_enemy);
        CrossSceneValues.targetList_enemy.Add(CrossSceneValues.friendlyFighterList_enemy);
        CrossSceneValues.targetList_enemy.Add(CrossSceneValues.friendlyCapitalShipTargetList_enemy);
        CrossSceneValues.targetList_enemy.Add(CrossSceneValues.objectiveList_enemy);
        CrossSceneValues.targetList_enemy.Add(CrossSceneValues.projectilesList_enemy);

        if (CrossSceneValues.shipToLoad == "X")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(0, true);
        }
        else if (CrossSceneValues.shipToLoad == "A")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(1, true);
        }
        else if (CrossSceneValues.shipToLoad == "Y")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(2, true);
        }
        else if (CrossSceneValues.shipToLoad == "B")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(3, true);
        }
        else if (CrossSceneValues.shipToLoad == "LN")
        {
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(4, false);
        }
        else if (CrossSceneValues.shipToLoad == "IN")
        {
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(5, false);
        }
        else if (CrossSceneValues.shipToLoad == "SA")
        {
            hudManager = GameObject.Instantiate(noShieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(6, false);
        }
        else if (CrossSceneValues.shipToLoad == "D")
        {
            hudManager = GameObject.Instantiate(shieldHUD).transform.Find("HUD Parent").gameObject.GetComponent<HUDManager>();
            setUpScene(7, false);
        }

        pc.onReset += worldShift;
    }

    void worldShift(float xShift, float zShift)
    {
        virtualCamera.OnTargetObjectWarped(player.transform.Find("FollowTarget"), new Vector3(xShift, 0, zShift));
        rearVirtualCamera.OnTargetObjectWarped(player.transform.Find("FollowTargetRear"), new Vector3(xShift, 0, zShift));

        foreach (List<ShipController> subList in CrossSceneValues.targetList)
        {
            foreach (ShipController sc in subList)
            {
                sc.transform.position += new Vector3(xShift, 0, zShift);
            }
        }
    }

}
