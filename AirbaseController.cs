using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirbaseController : StaticObjectiveController
{
    public int sector;
    public static int targetedAirbase;

    // Start is called before the first frame update
    void Start()
    {
        StaticControllerStart();
        despawnThreshold = 75000;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetedAirbase != sector && targetMarker.gameObject.activeSelf)
        {
            toggleTargetMarker(false);
        }
        StaticControllerUpdate();
        ShipControllerUpdate();
    }

    public override void destroyShipController()
    {
        SpawnGameAssets.airbaseSpawnStatus[sector] = false;
        CrossSceneValues.objectiveList.Remove(this);
        CrossSceneValues.objectiveList_enemy.Remove(this);
        Destroy(gameObject);
    }

    public override string getShipName()
    {
        if ((CrossSceneValues.airbaseValues[targetedAirbase] && CrossSceneValues.isRebelPlayer()) || (!CrossSceneValues.airbaseValues[targetedAirbase] && !CrossSceneValues.isRebelPlayer()))
        {
            return "AIRBASE (NR) SECTOR " + targetedAirbase.ToString();
        }
        else
        {
            return "AIRBASE (EMPIRE) SECTOR " + targetedAirbase.ToString();
        }
        
    }

    public override Vector3 getJamPosition(bool friendly, out bool jammed)
    {
        jammed = false;
        if (sector == targetedAirbase)
        {
            return transform.position;
        }

        return SpawnGameAssets.calculateAirbaseUnityCoord(targetedAirbase);
    }
}
