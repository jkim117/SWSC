using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC80AController : CapShipController
{
    private float totalShield;
    private float currentShield;

    private bool bridgeDestroyed = false;
    private bool engine1Destroyed = false;
    private bool engine2Destroyed = false;
    private bool engine3Destroyed = false;
    private bool engine4Destroyed = false;
    private bool engine5Destroyed = false;
    private bool engine6Destroyed = false;
    private bool engine7Destroyed = false;
    private bool engine8Destroyed = false;
    private bool engine9Destroyed = false;
    private bool engine10Destroyed = false;
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

        fighterSpawnLocation = new Vector3(483, 0, -276);
        fighterSpawnOrientation = Quaternion.Euler(-90, 0, 0);
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
        // if all destroyed, current shield is zero and return
        if (subcomponentList[2].shipHealth <= 0 && subcomponentList[3].shipHealth <= 0 &&
            subcomponentList[4].shipHealth <= 0 && subcomponentList[5].shipHealth <= 0 &&
            subcomponentList[6].shipHealth <= 0 && subcomponentList[7].shipHealth <= 0)
        {
            currentShield = 0f;
            return;
        }
        if (shipHealth <= 0f)
        {
            currentShield = 0f;
            return;
        }
        // for every shield gen destroyed, count up and calculate the shieldregentimer and max regen values
        int shieldGensDestroyed = 0;
        if (subcomponentList[2].shipHealth <= 0)
        {
            shieldGensDestroyed++;
        }
        if (subcomponentList[3].shipHealth <= 0)
        {
            shieldGensDestroyed++;
        }
        if (subcomponentList[4].shipHealth <= 0)
        {
            shieldGensDestroyed++;
        }
        if (subcomponentList[5].shipHealth <= 0)
        {
            shieldGensDestroyed++;
        }
        if (subcomponentList[6].shipHealth <= 0)
        {
            shieldGensDestroyed++;
        }
        if (subcomponentList[7].shipHealth <= 0)
        {
            shieldGensDestroyed++;
        }

        shieldRegenTimer += Time.fixedDeltaTime;
        if (shieldRegenTimer >= shieldRegenInterval + shieldGensDestroyed)
        {
            shieldRegenTimer = 0f;
            currentShield += 50f;
            if (currentShield >= totalShield - (shieldGensDestroyed * totalShield / 6))
            {
                currentShield = totalShield - (shieldGensDestroyed * totalShield / 6);
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
            if (thisCollider.Contains("FrontShieldPort"))
            {
                subcomponentList[3].shipHealth += currentShield;
                if (subcomponentList[3].shipHealth <= 0)
                {
                    subcomponentList[3].shipHealth = 0;
                    subcomponentList[3].startFire();
                }
            }
            else if (thisCollider.Contains("FrontShieldStarboard"))
            {
                subcomponentList[2].shipHealth += currentShield;
                if (subcomponentList[2].shipHealth <= 0)
                {
                    subcomponentList[2].shipHealth = 0;
                    subcomponentList[2].startFire();
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
            else if (thisCollider.Contains("MidShieldPort"))
            {
                subcomponentList[5].shipHealth += currentShield;
                if (subcomponentList[5].shipHealth <= 0)
                {
                    subcomponentList[5].shipHealth = 0;
                    subcomponentList[5].startFire();
                }
            }
            else if (thisCollider.Contains("MidShieldStarboard"))
            {
                subcomponentList[4].shipHealth += currentShield;
                if (subcomponentList[4].shipHealth <= 0)
                {
                    subcomponentList[4].shipHealth = 0;
                    subcomponentList[4].startFire();
                }
            }
            else if (thisCollider.Contains("PowerGenerator"))
            {
                subcomponentList[1].shipHealth += currentShield;
                if (subcomponentList[1].shipHealth <= 0)
                {
                    subcomponentList[1].shipHealth = 0;
                    subcomponentList[1].startFire();
                }
            }
            else if (thisCollider.Contains("Engine1"))
            {
                subcomponentList[8].shipHealth += currentShield;
                if (subcomponentList[8].shipHealth <= 0)
                {
                    subcomponentList[8].shipHealth = 0;
                    subcomponentList[8].startFire();
                    if (!engine1Destroyed)
                    {
                        engine1Destroyed = true;
                        maxSpeed -= 15f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine2"))
            {
                subcomponentList[9].shipHealth += currentShield;
                if (subcomponentList[9].shipHealth <= 0)
                {
                    subcomponentList[9].shipHealth = 0;
                    subcomponentList[9].startFire();
                    if (!engine2Destroyed)
                    {
                        engine2Destroyed = true;
                        maxSpeed -= 15;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine3"))
            {
                subcomponentList[10].shipHealth += currentShield;
                if (subcomponentList[10].shipHealth <= 0)
                {
                    subcomponentList[10].shipHealth = 0;
                    subcomponentList[10].startFire();
                    if (!engine3Destroyed)
                    {
                        engine3Destroyed = true;
                        maxSpeed -= 15f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine4"))
            {
                subcomponentList[11].shipHealth += currentShield;
                if (subcomponentList[11].shipHealth <= 0)
                {
                    subcomponentList[11].shipHealth = 0;
                    subcomponentList[11].startFire();
                    if (!engine4Destroyed)
                    {
                        engine4Destroyed = true;
                        maxSpeed -= 15f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine5"))
            {
                subcomponentList[12].shipHealth += currentShield;
                if (subcomponentList[12].shipHealth <= 0)
                {
                    subcomponentList[12].shipHealth = 0;
                    subcomponentList[12].startFire();
                    if (!engine5Destroyed)
                    {
                        engine5Destroyed = true;
                        maxSpeed -= 6f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine6"))
            {
                subcomponentList[13].shipHealth += currentShield;
                if (subcomponentList[13].shipHealth <= 0)
                {
                    subcomponentList[13].shipHealth = 0;
                    subcomponentList[13].startFire();
                    if (!engine6Destroyed)
                    {
                        engine6Destroyed = true;
                        maxSpeed -= 6f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine7"))
            {
                subcomponentList[14].shipHealth += currentShield;
                if (subcomponentList[14].shipHealth <= 0)
                {
                    subcomponentList[14].shipHealth = 0;
                    subcomponentList[14].startFire();
                    if (!engine7Destroyed)
                    {
                        engine7Destroyed = true;
                        maxSpeed -= 6f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine8"))
            {
                subcomponentList[15].shipHealth += currentShield;
                if (subcomponentList[15].shipHealth <= 0)
                {
                    subcomponentList[15].shipHealth = 0;
                    subcomponentList[15].startFire();
                    if (!engine8Destroyed)
                    {
                        engine8Destroyed = true;
                        maxSpeed -= 6f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine9"))
            {
                subcomponentList[16].shipHealth += currentShield;
                if (subcomponentList[16].shipHealth <= 0)
                {
                    subcomponentList[16].shipHealth = 0;
                    subcomponentList[16].startFire();
                    if (!engine9Destroyed)
                    {
                        engine9Destroyed = true;
                        maxSpeed -= 6f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("Engine10"))
            {
                subcomponentList[17].shipHealth += currentShield;
                if (subcomponentList[17].shipHealth <= 0)
                {
                    subcomponentList[17].shipHealth = 0;
                    subcomponentList[17].startFire();
                    if (!engine10Destroyed)
                    {
                        engine10Destroyed = true;
                        maxSpeed -= 6f;
                        lookRateSpeedYaw -= 1f;
                    }
                }
            }
            else if (thisCollider.Contains("RearShieldPort"))
            {
                subcomponentList[7].shipHealth += currentShield;
                if (subcomponentList[7].shipHealth <= 0)
                {
                    subcomponentList[7].shipHealth = 0;
                    subcomponentList[7].startFire();
                }
            }
            else if (thisCollider.Contains("RearShieldStarboard"))
            {
                subcomponentList[6].shipHealth += currentShield;
                if (subcomponentList[6].shipHealth <= 0)
                {
                    subcomponentList[6].shipHealth = 0;
                    subcomponentList[6].startFire();
                }
            }

            // general damage
            if (subcomponentList[1].shipHealth <= 0f)
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
        subcomponentList[8].shipShield = currentShield;
        subcomponentList[9].shipShield = currentShield;
        subcomponentList[10].shipShield = currentShield;
        subcomponentList[11].shipShield = currentShield;
        subcomponentList[12].shipShield = currentShield;
        subcomponentList[13].shipShield = currentShield;
        subcomponentList[14].shipShield = currentShield;
        subcomponentList[15].shipShield = currentShield;
        subcomponentList[16].shipShield = currentShield;
        subcomponentList[17].shipShield = currentShield;

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
        subcomponentList[8].shipShield = currentShield;
        subcomponentList[9].shipShield = currentShield;
        subcomponentList[10].shipShield = currentShield;
        subcomponentList[11].shipShield = currentShield;
        subcomponentList[12].shipShield = currentShield;
        subcomponentList[13].shipShield = currentShield;
        subcomponentList[14].shipShield = currentShield;
        subcomponentList[15].shipShield = currentShield;
        subcomponentList[16].shipShield = currentShield;
        subcomponentList[17].shipShield = currentShield;
    }
}
