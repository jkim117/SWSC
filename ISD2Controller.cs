using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISD2Controller : CapShipController
{
    /*private float totalFrontShield;
    private float frontShield;
    private float totalMidShield;
    private float midShield;
    private float totalRearShield;
    private float rearShield;
    private float totalBridgeShield;
    private float bridgeShield;*/
    private float totalShield;
    private float currentShield;

    private bool bridgeDestroyed = false;
    private bool engine1Destroyed = false;
    private bool engine2Destroyed = false;
    private bool engine3Destroyed = false;
    private bool TLTurnedOn = true;

    private float shieldRegenTimer = 0f;
    private float shieldRegenInterval = 5f;

    // Start is called before the first frame update
    void Start()
    {
        CapShipControllerStart();

        rollValue = 10f;
        rollAcceleration = 0.1f;
        lookRateSpeedPitch = 10f;
        lookRateSpeedYaw = 10f;
        forwardAcceleration = 1f;
        maxSpeed = 100f;
        pitchAcceleration = 0.5f;
        yawAcceleration = 0.5f;

        sensorBurnLimit = 1000;
        sensorRangeLimit = 10000f;

        if (CrossSceneValues.difficulty == 0)
        {
            totalShipHealth = 10000f;
            shipHealth = 10000f;

            totalShield = 10000f;
            currentShield = 10000f;
        }
        else if (CrossSceneValues.difficulty == 1)
        {
            totalShipHealth = 15000f;
            shipHealth = 15000f;

            totalShield = 15000f;
            currentShield = 15000f;
        }
        else
        {
            totalShipHealth = 20000f;
            shipHealth = 20000f;

            totalShield = 20000f;
            currentShield = 20000f;
        }
        
        /*totalFrontShield = 10000f;
        frontShield = 10000f;
        totalMidShield = 10000f;
        midShield = 10000f;
        totalRearShield = 10000f;
        rearShield = 10000f;
        totalBridgeShield = 10000f;
        bridgeShield = 10000f;*/

        fighterSpawnLocation = new Vector3(0, -214, -148);
        fighterSpawnOrientation = Quaternion.Euler(-90, 0, -180);
    }

    // Update is called once per frame
    void Update()
    {
        StaticControllerUpdate();
        ShipControllerUpdate();
    }

    private void FixedUpdate()
    {
        handleAIMovement();
        handleMovement();
        
        if (TLTurnedOn)
            handleTL();
        shieldRegenUpdate();
    }

    private void shieldRegenUpdate()
    {
        if (subcomponentList[2].shipHealth <= 0 && subcomponentList[3].shipHealth <= 0)
        {
            currentShield = 0f;
            return;
        }
        if (shipHealth <= 0f)
        {
            currentShield = 0f;
            return;
        }

        shieldRegenTimer += Time.fixedDeltaTime;
        if (shieldRegenTimer >= shieldRegenInterval)
        {
            shieldRegenTimer = 0f;
            currentShield += 50f;

            if (subcomponentList[2].shipHealth <= 0 || subcomponentList[3].shipHealth <= 0)
            {
                shieldRegenInterval = 10f;
                if (currentShield >= totalShield / 2f)
                {
                    currentShield = totalShield / 2f;
                }
            }
            else if (currentShield >= totalShield)
            {
                currentShield = totalShield;
            }
        }
    }
    
    private void handleTL()
    {
        if (!bridgeDestroyed && subcomponentList[0].shipHealth <= 0f)
        {
            foreach (TLController tl in TLList)
            {
                tl.alterRateOfFire(1.5f, 3f, 25f);
            }
            bridgeDestroyed = true;

        }
        foreach (TLController tl in TLList)
        {
            if (tl.currentTarget == null)
            {
                tl.currentTarget = randomCycleTargets();
            }
        }
        if (shipHealth <= 0)
        {
            TLTurnedOn = false;
            foreach (TLController tl in TLList)
            {
                Destroy(tl.gameObject);
            }
        }
    }

    public override float getShieldPercentage()
    {
        return (float)Mathf.Round(currentShield / totalShield * 100f);
    }

    protected override void takeDamage(float damage, Vector3 hitPoint, string thisCollider)
    {
        if (shipHealth <= 0)
        {
            return;
        }
        currentShield -= damage;
        if (currentShield < 0)
        {
            if (thisCollider.Contains("MainHangar"))
            {
                subcomponentList[1].shipHealth += currentShield;
                if (subcomponentList[1].shipHealth <= 0)
                {
                    subcomponentList[1].shipHealth = 0;
                    subcomponentList[1].startFire();
                    maxNumberFighters = 0;
                }
            }
            else if (thisCollider.Contains("PowerGenerator"))
            {
                subcomponentList[4].shipHealth += currentShield;
                if (subcomponentList[4].shipHealth <= 0)
                {
                    subcomponentList[4].shipHealth = 0;
                    subcomponentList[4].startFire();
                }
            }
            else if (thisCollider.Contains("Bridge"))
            {
                subcomponentList[0].shipHealth += currentShield;
                if (subcomponentList[0].shipHealth <= 0)
                {
                    subcomponentList[0].shipHealth = 0;
                    subcomponentList[0].startFire();
                }
            }
            else if (thisCollider.Contains("StarboardShield"))
            {
                subcomponentList[2].shipHealth += currentShield;
                if (subcomponentList[2].shipHealth <= 0)
                {
                    subcomponentList[2].shipHealth = 0;
                    subcomponentList[2].startFire();
                }
            }
            else if (thisCollider.Contains("PortShield"))
            {
                subcomponentList[3].shipHealth += currentShield;
                if (subcomponentList[3].shipHealth <= 0)
                {
                    subcomponentList[3].shipHealth = 0;
                    subcomponentList[3].startFire();
                }
            }
            else if (thisCollider.Contains("Engine1"))
            {
                subcomponentList[5].shipHealth += currentShield;
                if (subcomponentList[5].shipHealth <= 0)
                {
                    subcomponentList[5].shipHealth = 0;
                    subcomponentList[5].startFire();
                    if (!engine1Destroyed)
                    {
                        engine1Destroyed = true;
                        maxSpeed -= 33f;
                        lookRateSpeedYaw -= 3f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine2"))
            {
                subcomponentList[6].shipHealth += currentShield;
                if (subcomponentList[6].shipHealth <= 0)
                {
                    subcomponentList[6].shipHealth = 0;
                    subcomponentList[6].startFire();
                    if (!engine2Destroyed)
                    {
                        engine2Destroyed = true;
                        maxSpeed -= 33f;
                        lookRateSpeedYaw -= 3f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine3"))
            {
                subcomponentList[7].shipHealth += currentShield;
                if (subcomponentList[7].shipHealth <= 0)
                {
                    subcomponentList[7].shipHealth = 0;
                    subcomponentList[7].startFire();
                    if (!engine3Destroyed)
                    {
                        engine3Destroyed = true;
                        maxSpeed -= 33f;
                        lookRateSpeedYaw -= 3f;
                    }
                }
            }
            
            // general damage
            if (subcomponentList[4].shipHealth <= 0f)
            {
                shipHealth += currentShield * 1.5f;
            }
            else
            {
                shipHealth += currentShield;
            }
            if (shipHealth < 0f)
            {
                shipHealth = 0;
            }
            currentShield = 0;
        }
        subcomponentList[0].shipShield = currentShield;
        subcomponentList[1].shipShield = currentShield;
        subcomponentList[2].shipShield = currentShield;
        subcomponentList[3].shipShield = currentShield;
        subcomponentList[4].shipShield = currentShield;
        subcomponentList[5].shipShield = currentShield;
        subcomponentList[6].shipShield = currentShield;
        subcomponentList[7].shipShield = currentShield;

        if (getHealthPercentage() < 67 && !shipFires1.activeSelf)
        {
            shipFires1.SetActive(true);
        }
        if (getHealthPercentage() < 33 && !shipFires2.activeSelf)
        {
            shipFires2.SetActive(true);
        }

        if (shipHealth <= 0)
        {
            shipFires3.SetActive(true);
            if (friendly)
            {
                CrossSceneValues.friendlyCapitalShipTargetList.Remove(this);
                CrossSceneValues.friendlyCapitalShipTargetList_enemy.Remove(this);
            }
            else
            {
                CrossSceneValues.enemyCapitalShipTargetList.Remove(this);
                CrossSceneValues.enemyCapitalShipTargetList_enemy.Remove(this);
            }
            //AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);
            //Destroy(gameObject);
            maxNumberFighters = 0;
        }
    }
    protected override void takeIonDamage(float damage, Vector3 hitPoint, string thisCollider)
    {
        currentShield -= damage;
        if (currentShield < 0)
        {
            currentShield = 0;
        }
        subcomponentList[0].shipShield = currentShield;
        subcomponentList[1].shipShield = currentShield;
        subcomponentList[2].shipShield = currentShield;
        subcomponentList[3].shipShield = currentShield;
        subcomponentList[4].shipShield = currentShield;
        subcomponentList[5].shipShield = currentShield;
        subcomponentList[6].shipShield = currentShield;
        subcomponentList[7].shipShield = currentShield;
    }
}
