using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class SceneInitializer : MonoBehaviour
{
    private GameObject player;
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineVirtualCamera rearVirtualCamera;
    private bool isRebelPlayer;
    [SerializeField] private SpawnGameAssets spawner;

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
        PlayerController pc;

        player = spawner.swapPlayerShip();
        pc = player.GetComponent<PlayerController>();
        pc.onReset += worldShift;

        playerCameraSetUp();
        EndlessTerrain et = GameObject.Find("TerrainGenerator").GetComponent<EndlessTerrain>();
        et.viewer = player.transform;
        et.subscribeToViewerEvent();

        GameObject.Find("PauseMenu").GetComponent<PauseMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        GameObject.Find("InGameMenu").GetComponent<InGameMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        if (CrossSceneValues.tutorialMode)
            GameObject.Find("TutorialPrompts").GetComponent<TutorialScript>().playerInput = player.GetComponent<PlayerInput>();
    }

    void setUpScene()
    {
        PlayerController pc;

        NewWorldSetup.generateWorldData();
        isRebelPlayer = CrossSceneValues.isRebelPlayer();

        // based on CrossSceneValues.currentAirbase, spawn the airbase, the player at the airbase with the chosen fighter
        int sectorRow = CrossSceneValues.currentAirbase / NewWorldSetup.sectorDimension;
        int sectorCol = CrossSceneValues.currentAirbase - sectorRow * NewWorldSetup.sectorDimension;
        Vector3 airbaseWorldCoord = NewWorldSetup.airbaseCoordinates[sectorCol, sectorRow];
        PlayerController.totalxShift = -airbaseWorldCoord.x;
        PlayerController.totalzShift = -airbaseWorldCoord.z;

        spawner.spawnAirbase(isRebelPlayer, isRebelPlayer, CrossSceneValues.currentAirbase);
        AirbaseController.targetedAirbase = CrossSceneValues.currentAirbase;
        player = spawner.spawnPlayer();
        pc = player.GetComponent<PlayerController>();
        pc.onReset += worldShift;

        playerCameraSetUp();
        GameObject.Find("TerrainGenerator").GetComponent<EndlessTerrain>().viewer = player.transform;
        GameObject.Find("PauseMenu").GetComponent<PauseMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        GameObject.Find("InGameMenu").GetComponent<InGameMenuScript>().playerInput = player.GetComponent<PlayerInput>();
        if (CrossSceneValues.tutorialMode)
            GameObject.Find("TutorialPrompts").GetComponent<TutorialScript>().playerInput = player.GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {

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

        setUpScene();
    }

    // When within a certain distance of an airbase, spawn it
    private void Update()
    {
        float spawnThreshold = 30000;
        for (int i = 0; i < NewWorldSetup.sectorDimension * NewWorldSetup.sectorDimension; i++)
        {
            Vector3 airbaseUnityCoord = SpawnGameAssets.calculateAirbaseUnityCoord(i);
            if (new Vector2(airbaseUnityCoord.x, airbaseUnityCoord.z).magnitude < spawnThreshold && !SpawnGameAssets.airbaseSpawnStatus[i]) //spawn this airbase
            {
                // if rebelplayer, airbase is true, then airbase is rebel. airbase false, airbase imp
                // if not rebelplayer, airbase is true, airbase is imp. airbase false, airbase rebel
                bool rebelBase = false;
                if (isRebelPlayer && CrossSceneValues.airbaseValues[i])
                    rebelBase = true;
                if (!isRebelPlayer && !CrossSceneValues.airbaseValues[i])
                    rebelBase = true;
                spawner.spawnAirbase(rebelBase, isRebelPlayer, i);
            }
        }
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
