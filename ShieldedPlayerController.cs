using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ShieldedPlayerController : PlayerController
{
    // POWER VALUES CONFIGURABLE
    protected float totalForwardShieldHealth; // can max to 150% on both rear and forward shields or 200% on front/rear and 100% on the other
    protected float totalRearShieldHealth;
    protected float totalShieldLimit;
    // POWER VALUES VARIABLE
    private bool engineDown = false;
    private bool laserDown = false;
    private bool shieldDown = false;
    private bool engineActionPerformed = false;
    private bool laserActionPerformed = false;
    private bool shieldActionPerformed = false;
    private int shieldFocusStatus = 0; // 0 is stabilize, -1 is focus rear, 1 is focus forward
    protected float forwardShieldHealth;
    protected float rearShieldHealth;
    protected float forwardOverShieldHealth;
    protected float rearOverShieldHealth;
    private float shieldChargeTimestamp = 0f;
    private float shieldRegenDelay = 0f;

    protected float shieldTransfer0 = 0.05f;
    protected float shieldTransfer1 = 0.075f;
    protected float shieldTransfer2 = 0.1f;
    protected float shieldTransfer3 = 0.125f;
    protected float shieldTransfer4 = 0.15f;
    protected float shieldChange0 = -0.025f;
    protected float shieldChange1 = -0.0125f;
    protected float shieldChange3 = 0.05f;
    protected float shieldChange4 = 0.1f;
    protected float shieldRegenDelayFull = 5f;

    //************************************************INPUT FUNCTIONS************************************************//
    void OnEnginePower(InputValue value)
    {
        if (ionStatus)
        {
            return;
        }
        if (value.Get<float>() == 1f)
        {
            engineDown = true;
            engineActionPerformed = false;
        }
        else
        {
            engineDown = false;
        }

        if (!engineDown && laserDown && !engineActionPerformed)
        {
            if (laserPower > 0 && enginePower < enginePowerLimit)
            {
                enginePower++;
                laserPower--;
            }

            engineActionPerformed = true;
            laserActionPerformed = true;
        }

        if (!engineDown && shieldDown && !engineActionPerformed)
        {
            if (shieldPower > 0 && enginePower < enginePowerLimit)
            {
                enginePower++;
                shieldPower--;
            }

            engineActionPerformed = true;
            shieldActionPerformed = true;
        }
        else if (!engineDown && shieldDown && enginePowerLimit == enginePower)
        {
            shieldActionPerformed = true;
        }

        if (!engineDown && !laserDown && !shieldDown && !engineActionPerformed)
        {
            if (enginePower == enginePowerLimit || overallPower == overallPowerLimit)
            {
                overallPower -= enginePower;
                enginePower = 0;
            }
            else if (overallPower < overallPowerLimit)
            {
                overallPower++;
                enginePower++;
            }
            engineActionPerformed = true;
        }
    }

    void OnLaserPower(InputValue value)
    {
        if (ionStatus)
        {
            return;
        }
        if (value.Get<float>() == 1f)
        {
            laserDown = true;
            laserActionPerformed = false;
        }
        else
        {
            laserDown = false;
        }

        if (!laserDown && engineDown && !laserActionPerformed)
        {
            if (enginePower > 0 && laserPower < otherPowerLimit)
            {
                laserPower++;
                enginePower--;
            }
            laserActionPerformed = true;
            engineActionPerformed = true;
        }
        if (!laserDown && shieldDown && !laserActionPerformed)
        {
            if (shieldPower > 0 && laserPower < otherPowerLimit)
            {
                laserPower++;
                shieldPower--;
            }

            laserActionPerformed = true;
            shieldActionPerformed = true;
        }
        if (!laserDown && !engineDown && !shieldDown && !laserActionPerformed)
        {
            laserActionPerformed = true;
            if (enginePower == 0)
            {
                enginePower += laserPower;
                laserPower = 0;
            }
            else if (laserPower == otherPowerLimit)
            {
                laserPower = 0;
                enginePower += otherPowerLimit;
                if (enginePower > enginePowerLimit)
                {
                    laserPower = enginePower - enginePowerLimit;
                    enginePower = enginePowerLimit;
                }
            }
            else
            {
                laserPower++;
                enginePower--;
            }
        }


    }

    void OnShieldPower(InputValue value)
    {
        if (ionStatus)
        {
            return;
        }
        if (value.Get<float>() == 1f)
        {
            shieldDown = true;
            shieldActionPerformed = false;
        }
        else
        {
            shieldDown = false;
        }

        if (!shieldDown && engineDown && !shieldActionPerformed)
        {
            if (enginePower > 0 && shieldPower < otherPowerLimit)
            {
                shieldPower++;
                enginePower--;
            }

            shieldActionPerformed = true;
            engineActionPerformed = true;
        }
        if (!shieldDown && laserDown && !shieldActionPerformed)
        {
            if (laserPower > 0 && shieldPower < otherPowerLimit)
            {
                shieldPower++;
                laserPower--;
            }

            shieldActionPerformed = true;
            laserActionPerformed = true;
        }
        if (!shieldDown && !engineDown && !laserDown && !shieldActionPerformed)
        {
            shieldActionPerformed = true;
            if (enginePower == 0)
            {
                enginePower += shieldPower;
                shieldPower = 0;
            }
            else if (shieldPower == otherPowerLimit)
            {
                shieldPower = 0;
                enginePower += otherPowerLimit;
                if (enginePower > enginePowerLimit)
                {
                    shieldPower = enginePower - enginePowerLimit;
                    enginePower = enginePowerLimit;
                }
            }
            else
            {
                shieldPower++;
                enginePower--;
            }
        }

    }

    void OnOverallPower()
    {
        if (ionStatus)
        {
            return;
        }
        if (overallPower < overallPowerLimit)
        {
            overallPower++;

            if (enginePower < enginePowerLimit)
            {
                enginePower++;
            }
            else if (laserPower < otherPowerLimit)
            {
                laserPower++;
            }
            else if (shieldPower < otherPowerLimit)
            {
                shieldPower++;
            }

        }

    }

    void OnDecreaseOverallPower()
    {
        if (ionStatus)
        {
            return;
        }
        if (overallPower > 0)
        {
            overallPower--;

            if (enginePower > 0)
            {
                enginePower--;
            }
            else if (laserPower > 0)
            {
                laserPower--;
            }
            else if (shieldPower > 0)
            {
                shieldPower--;
            }
        }

    }
    void OnShieldControls()
    {
        if (shieldFocusStatus == 0)
        {
            shieldFocusStatus = 1;
        }
        else if (shieldFocusStatus == 1)
        {
            shieldFocusStatus = -1;
        }
        else if (shieldFocusStatus == -1)
        {
            shieldFocusStatus = 0;
        }
    }
    void OnShuntToLasers()
    {
        float totalCurrentShield = forwardShieldHealth + forwardOverShieldHealth + rearShieldHealth + rearOverShieldHealth;
        if (laserCharge >= laserChargeLimit || totalCurrentShield <= 0f)
        {
            return;
        }
        float shuntTax = 0.8f;


        if ((float)(laserChargeLimit - laserCharge) < shuntTax * 5f)
        {
            if (totalCurrentShield >= (1f / shuntTax) * (float)(laserChargeLimit - laserCharge))
            {
                laserCharge = laserChargeLimit;
            }
            else
            {
                laserCharge += (int)totalCurrentShield;
            }
            subtractToForwardShield((1f / shuntTax) * (float)(laserChargeLimit - laserCharge) / -2f);
            subtractToRearShield((1f / shuntTax) * (float)(laserChargeLimit - laserCharge) / -2f);
        }
        else
        {
            if (totalCurrentShield >= 5)
            {
                laserCharge += 4;
            }
            else
            {
                laserCharge += (int)(totalCurrentShield * shuntTax);
            }
            subtractToForwardShield(-2.5f);
            subtractToRearShield(-2.5f);
        }
    }
    void OnShuntToShields()
    {
        float totalCurrentShield = forwardShieldHealth + forwardOverShieldHealth + rearShieldHealth + rearOverShieldHealth;
        if (totalCurrentShield >= totalShieldLimit || laserCharge <= 0)
        {
            return;
        }
        float shuntTax = 0.8f;

        if (totalShieldLimit - totalCurrentShield < shuntTax * 5f)
        {
            // check if you can subtract the full amount from lasers
            if (laserCharge >= (int)((1 / shuntTax) * (totalShieldLimit - totalCurrentShield)))
            {
                laserCharge -= (int)((1 / shuntTax) * (totalShieldLimit - totalCurrentShield));
                addToForwardShield((totalShieldLimit - totalCurrentShield) / 2f);
                addToRearShield((totalShieldLimit - totalCurrentShield) / 2f);
            }
            else
            {
                addToForwardShield((float)laserCharge / 2f);
                addToRearShield((float)laserCharge / 2f);
                laserCharge = 0;
            }
        }
        else
        {
            if (laserCharge >= 5)
            {
                laserCharge -= 5;
                addToForwardShield(shuntTax * 2.5f);
                addToRearShield(shuntTax * 2.5f);
            }
            else
            {
                addToForwardShield(shuntTax * (float)laserCharge / 2f);
                addToRearShield(shuntTax * (float)laserCharge / 2f);
                laserCharge = 0;
            }
        }
    }
    void OnShieldsToJammer()
    {
        float totalCurrentShield = forwardShieldHealth + forwardOverShieldHealth + rearShieldHealth + rearOverShieldHealth;
        if (totalCurrentShield >= 10 && currentJammer <= maxJammer - 1)
        {
            if (currentJammer <= 0)
            {
                jammerLocation = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            }
            subtractToForwardShield(-5f);
            subtractToRearShield(-5f);
            currentJammer += 1;
        }
    }

    //************************************************OVERRIDE FUNCTIONS************************************************//
    public override float getShieldPercentage()
    {
        return (float)Mathf.Round((float)(forwardShieldHealth + rearShieldHealth + forwardOverShieldHealth + rearOverShieldHealth) / (float)(totalForwardShieldHealth + totalRearShieldHealth) * 100f);
    }
    public override float getFrontShield()
    {
        return forwardShieldHealth / totalForwardShieldHealth;
    }

    public override float getFrontOverShield()
    {
        return forwardOverShieldHealth / totalForwardShieldHealth;
    }

    public override float getRearShield()
    {
        return rearShieldHealth / totalRearShieldHealth;
    }
    public override float getRearOverShield()
    {
        return rearOverShieldHealth / totalRearShieldHealth;
    }

    public override int getShieldFocusStatus()
    {
        return shieldFocusStatus;
    }
    protected override void takeDamage(float damage, bool frontOrRear)
    {
        if (CrossSceneValues.difficulty == 0)
        {
            damage = damage * 0.75f;
        }
        else if (CrossSceneValues.difficulty == 2)
        {
            damage = damage * 1.25f;
        }

        float damageToHull = 0f;
        shieldRegenDelay = Time.time + shieldRegenDelayFull;

        if (frontOrRear) // front damage
        {
            if (forwardOverShieldHealth > 0)
            {
                forwardOverShieldHealth -= damage;
                if (forwardOverShieldHealth < 0)
                {
                    forwardShieldHealth += forwardOverShieldHealth;
                    forwardOverShieldHealth = 0;
                    if (forwardShieldHealth < 0)
                    {
                        damageToHull = -forwardShieldHealth;
                        forwardShieldHealth = 0;
                    }
                }
            }
            else if (forwardShieldHealth > 0)
            {
                forwardShieldHealth -= damage;
                if (forwardShieldHealth < 0)
                {
                    damageToHull = -forwardShieldHealth;
                    forwardShieldHealth = 0;
                }
            }
            else
            {
                damageToHull = damage;
            }
        }
        else
        {
            if (rearOverShieldHealth > 0)
            {
                rearOverShieldHealth -= damage;
                if (rearOverShieldHealth < 0)
                {
                    rearShieldHealth += rearOverShieldHealth;
                    rearOverShieldHealth = 0;
                    if (rearShieldHealth < 0)
                    {
                        damageToHull = -rearShieldHealth;
                        rearShieldHealth = 0;
                    }
                }
            }
            else if (rearShieldHealth > 0)
            {
                rearShieldHealth -= damage;
                if (rearShieldHealth < 0)
                {
                    damageToHull = -rearShieldHealth;
                    rearShieldHealth = 0;
                }
            }
            else
            {
                damageToHull = damage;
            }
        }
        shipHealth -= damageToHull;

        if (shipHealth <= 0)
        {
            GameObject explosion = GameObject.Instantiate(explosionPrefab, transform.position, transform.rotation);
            explosion.GetComponent<ParticleSystem>().Play();
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);
            //destroyShipController();
            if (CrossSceneValues.shipToLoad == "X")
            {
                CrossSceneValues.shipToLoad = "B";
            }
            else if (CrossSceneValues.shipToLoad == "A")
            {
                CrossSceneValues.shipToLoad = "X";
            }
            else if (CrossSceneValues.shipToLoad == "Y")
            {
                CrossSceneValues.shipToLoad = "A";
            }
            else if (CrossSceneValues.shipToLoad == "B")
            {
                CrossSceneValues.shipToLoad = "Y";
            }
            else if (CrossSceneValues.shipToLoad == "LN")
            {
                CrossSceneValues.shipToLoad = "D";
            }
            else if (CrossSceneValues.shipToLoad == "IN")
            {
                CrossSceneValues.shipToLoad = "LN";
            }
            else if (CrossSceneValues.shipToLoad == "SA")
            {
                CrossSceneValues.shipToLoad = "IN";
            }
            else if (CrossSceneValues.shipToLoad == "D")
            {
                CrossSceneValues.shipToLoad = "SA";
            }
            GameObject.Find("SceneInitializer").GetComponent<SceneInitializer>().SwapShip();
        }
    }
    protected override void takeIonDamage(float damage, bool frontOrRear)
    {
        if (CrossSceneValues.difficulty == 0)
        {
            damage = damage * 0.75f;
        }
        else if (CrossSceneValues.difficulty == 2)
        {
            damage = damage * 1.25f;
        }

        float damageToHull = 0f;
        shieldRegenDelay = Time.time + shieldRegenDelayFull;

        if (frontOrRear) // front damage
        {
            if (forwardOverShieldHealth > 0)
            {
                forwardOverShieldHealth -= damage;
                if (forwardOverShieldHealth < 0)
                {
                    forwardShieldHealth += forwardOverShieldHealth;
                    forwardOverShieldHealth = 0;
                    if (forwardShieldHealth < 0)
                    {
                        damageToHull = -forwardShieldHealth;
                        forwardShieldHealth = 0;
                    }
                }
            }
            else if (forwardShieldHealth > 0)
            {
                forwardShieldHealth -= damage;
                if (forwardShieldHealth < 0)
                {
                    damageToHull = -forwardShieldHealth;
                    forwardShieldHealth = 0;
                }
            }
            else
            {
                damageToHull = damage;
            }
        }
        else
        {
            if (rearOverShieldHealth > 0)
            {
                rearOverShieldHealth -= damage;
                if (rearOverShieldHealth < 0)
                {
                    rearShieldHealth += rearOverShieldHealth;
                    rearOverShieldHealth = 0;
                    if (rearShieldHealth < 0)
                    {
                        damageToHull = -rearShieldHealth;
                        rearShieldHealth = 0;
                    }
                }
            }
            else if (rearShieldHealth > 0)
            {
                rearShieldHealth -= damage;
                if (rearShieldHealth < 0)
                {
                    damageToHull = -rearShieldHealth;
                    rearShieldHealth = 0;
                }
            }
            else
            {
                damageToHull = damage;
            }
        }
        shipIonHealth -= damageToHull;

        if (shipIonHealth <= 0)
        {
            shipIonHealth = 0;
            for (int i = 0; i < 10; i++)
            {
                OnDecreaseOverallPower();
            }
            ionStatus = true;
            subtractToForwardShield(-totalForwardShieldHealth);
            subtractToRearShield(-totalRearShieldHealth);
            laserCharge = 0;
        }
    }

    //************************************************SHIELD UPDATE FUNCTIONS************************************************//
    void addToForwardShield(float amount)
    {
        Debug.Assert(amount >= 0); // amount to add to shield needs to be positive
        float currentShieldTotal = forwardShieldHealth + forwardOverShieldHealth + rearShieldHealth + rearOverShieldHealth;
        if (currentShieldTotal + amount > totalShieldLimit)
        {
            amount = totalShieldLimit - currentShieldTotal;
        }

        forwardShieldHealth += amount;
        if (forwardShieldHealth > totalForwardShieldHealth)
        {
            forwardOverShieldHealth += (forwardShieldHealth - totalForwardShieldHealth);
            forwardShieldHealth = totalForwardShieldHealth;
        }

        if (shieldFocusStatus == -1 && forwardOverShieldHealth > 0)
        {
            if (rearOverShieldHealth < totalRearShieldHealth)
            {
                addToRearShield(forwardOverShieldHealth);
            }
            else if (rearOverShieldHealth >= totalRearShieldHealth)
            {
                rearOverShieldHealth = totalRearShieldHealth;
            }
            forwardOverShieldHealth = 0f;
        }
        else if (shieldFocusStatus == 0 && forwardOverShieldHealth > totalForwardShieldHealth / 2f)
        {
            if (rearOverShieldHealth < totalRearShieldHealth / 2f)
            {
                addToRearShield(forwardOverShieldHealth - (totalForwardShieldHealth / 2f));
            }
            else if (rearOverShieldHealth >= totalRearShieldHealth / 2f)
            {
                rearOverShieldHealth = totalRearShieldHealth / 2f;
            }
            forwardOverShieldHealth = totalForwardShieldHealth / 2f;
        }
        else if (shieldFocusStatus == 1 && forwardOverShieldHealth > totalForwardShieldHealth)
        {
            if (rearShieldHealth < totalRearShieldHealth)
            {
                addToRearShield(forwardOverShieldHealth - totalForwardShieldHealth);
            }
            else if (rearShieldHealth >= totalRearShieldHealth)
            {
                rearShieldHealth = totalRearShieldHealth;
                rearOverShieldHealth = 0f;
            }
            forwardOverShieldHealth = totalForwardShieldHealth;
        }
    }

    void addToRearShield(float amount)
    {
        Debug.Assert(amount >= 0); // amount to add to shield needs to be positive

        float currentShieldTotal = forwardShieldHealth + forwardOverShieldHealth + rearShieldHealth + rearOverShieldHealth;
        if (currentShieldTotal + amount > totalShieldLimit)
        {
            amount = totalShieldLimit - currentShieldTotal;
        }

        rearShieldHealth += amount;
        if (rearShieldHealth > totalRearShieldHealth)
        {
            rearOverShieldHealth += (rearShieldHealth - totalRearShieldHealth);
            rearShieldHealth = totalRearShieldHealth;
        }

        if (shieldFocusStatus == 1 && rearOverShieldHealth > 0)
        {
            if (forwardOverShieldHealth < totalForwardShieldHealth)
            {
                addToForwardShield(rearOverShieldHealth);
            }
            else if (forwardOverShieldHealth >= totalForwardShieldHealth)
            {
                forwardOverShieldHealth = totalForwardShieldHealth;
            }
            rearOverShieldHealth = 0f;
        }
        else if (shieldFocusStatus == 0 && rearOverShieldHealth > totalRearShieldHealth / 2f)
        {
            if (forwardOverShieldHealth < totalForwardShieldHealth / 2f)
            {
                addToForwardShield(rearOverShieldHealth - (totalRearShieldHealth / 2f));
            }
            else if (forwardOverShieldHealth >= totalForwardShieldHealth / 2f)
            {
                forwardOverShieldHealth = totalForwardShieldHealth / 2f;
            }
            rearOverShieldHealth = totalRearShieldHealth / 2f;
        }
        else if (shieldFocusStatus == -1 && rearOverShieldHealth > totalRearShieldHealth)
        {
            if (forwardShieldHealth < totalForwardShieldHealth)
            {
                addToForwardShield(rearOverShieldHealth - totalRearShieldHealth);
            }
            else if (forwardShieldHealth >= totalForwardShieldHealth)
            {
                forwardShieldHealth = totalForwardShieldHealth;
                forwardOverShieldHealth = 0f;
            }
            rearOverShieldHealth = totalRearShieldHealth;
        }
    }

    void subtractToForwardShield(float amount)
    {
        Debug.Assert(amount <= 0); // amount to add to shield needs to be negative

        forwardOverShieldHealth += amount;
        if (forwardOverShieldHealth < 0)
        {
            forwardShieldHealth += forwardOverShieldHealth;
            forwardOverShieldHealth = 0f;
            if (forwardShieldHealth < 0 && rearShieldHealth + rearOverShieldHealth > -forwardShieldHealth)
            {
                subtractToRearShield(forwardShieldHealth);
                forwardShieldHealth = 0f;
            }
            else if (forwardShieldHealth < 0 && rearShieldHealth + rearOverShieldHealth <= -forwardShieldHealth)
            {
                forwardShieldHealth = 0f;
                rearShieldHealth = 0f;
                rearOverShieldHealth = 0f;
            }
        }
    }

    void subtractToRearShield(float amount)
    {
        Debug.Assert(amount <= 0); // amount to add to shield needs to be negative

        rearOverShieldHealth += amount;
        if (rearOverShieldHealth < 0)
        {
            rearShieldHealth += rearOverShieldHealth;
            rearOverShieldHealth = 0f;
            if (rearShieldHealth < 0 && forwardShieldHealth + forwardOverShieldHealth > -rearShieldHealth)
            {
                subtractToForwardShield(rearShieldHealth);
                rearShieldHealth = 0f;
            }
            else if (rearShieldHealth < 0 && forwardShieldHealth + forwardOverShieldHealth <= -rearShieldHealth)
            {
                rearShieldHealth = 0f;
                forwardShieldHealth = 0f;
                forwardOverShieldHealth = 0f;
            }
        }
    }

    void updateShieldCharge()
    {
        if (Time.time > shieldChargeTimestamp)
        {
            shieldChargeTimestamp = Time.time + 0.05f;
        }
        else
        {
            return;
        }

        float shieldChange = 0f;
        float shieldTransferRate = shieldTransfer2;
        if (shieldPower == 0) // loses a shield point every 2 seconds
        {
            shieldChange = shieldChange0;
            shieldTransferRate = shieldTransfer0;
        }
        else if (shieldPower == 1) // loses a shield point every 4 seconds
        {
            shieldChange = shieldChange1;
            shieldTransferRate = shieldTransfer1;
        }
        else if (shieldPower == 3) // gains a shield point every second
        {

            if (Time.time < shieldRegenDelay)
            {
                shieldChange = 0f;
            }
            else
            {
                shieldChange = shieldChange3;
            }
            shieldTransferRate = shieldTransfer3;
        }
        else if (shieldPower == 4) // gains a shield point every half second
        {

            if (Time.time < shieldRegenDelay)
            {
                shieldChange = 0f;
            }
            else
            {
                shieldChange = shieldChange4;
            }
            shieldTransferRate = shieldTransfer4;
        }


        if (shieldFocusStatus == 0 && shieldChange >= 0)
        {
            if (forwardOverShieldHealth < totalForwardShieldHealth / 2f && rearOverShieldHealth < totalRearShieldHealth / 2f) // both rear and forward are below the overshield half limit
            {
                addToForwardShield(shieldChange / 2f);
                addToRearShield(shieldChange / 2f);
            }
            else if (forwardOverShieldHealth < totalForwardShieldHealth / 2f && rearOverShieldHealth == totalRearShieldHealth / 2f) // forward shield is below the half limit, rear shield is at the limit
            {
                addToForwardShield(shieldChange);
            }
            else if (forwardOverShieldHealth == totalForwardShieldHealth / 2f && rearOverShieldHealth < totalRearShieldHealth / 2f) // forward shield is at the half limit, rear shield is below the limit
            {
                addToRearShield(shieldChange);
            }
            else if (forwardOverShieldHealth > totalForwardShieldHealth / 2f && rearOverShieldHealth < totalRearShieldHealth / 2f) // forward over shield above limit, rear overshield is below limit
            {
                if (forwardOverShieldHealth - totalForwardShieldHealth / 2f < shieldTransferRate)
                {
                    addToRearShield(shieldChange + forwardOverShieldHealth - totalForwardShieldHealth / 2f);
                    subtractToForwardShield(-(forwardOverShieldHealth - totalForwardShieldHealth / 2f));
                }
                else
                {
                    addToRearShield(shieldChange + shieldTransferRate);
                    subtractToForwardShield(-shieldTransferRate);
                }
            }
            else if (forwardOverShieldHealth < totalForwardShieldHealth / 2f && rearOverShieldHealth > totalRearShieldHealth / 2f) // forward over shield below limit, rear overshield above limit
            {
                if (rearOverShieldHealth - totalRearShieldHealth / 2f < shieldTransferRate)
                {
                    addToForwardShield(shieldChange + rearOverShieldHealth - totalRearShieldHealth / 2f);
                    subtractToRearShield(-(rearOverShieldHealth - totalRearShieldHealth / 2f));
                }
                else
                {
                    addToForwardShield(shieldChange + shieldTransferRate);
                    subtractToRearShield(-shieldTransferRate);
                }
            }
            else // if meets this condition, needs to be reset at limits
            {
                forwardShieldHealth = totalForwardShieldHealth;
                forwardOverShieldHealth = totalForwardShieldHealth / 2f;
                rearShieldHealth = totalRearShieldHealth;
                rearOverShieldHealth = totalRearShieldHealth / 2f;
            }

            // check forward overshield if at half limit
            // check rear overshield if at halflimit
            // increase rear/forward if both below half limit
            // or just update just rear or just foward if one is at limit
            // if one is above half limit, decrement it and add both to the other
        }
        else if (shieldFocusStatus == 1 && shieldChange >= 0)
        {
            if (forwardOverShieldHealth < totalForwardShieldHealth && rearShieldHealth < totalRearShieldHealth) // both rear and forward are below the limit
            {
                addToForwardShield(shieldChange / 2f);
                addToRearShield(shieldChange / 2f);
                if (rearShieldHealth < shieldTransferRate)
                {
                    addToForwardShield(rearShieldHealth);
                    subtractToRearShield(-rearShieldHealth);
                }
                else
                {
                    addToForwardShield(shieldTransferRate);
                    subtractToRearShield(-shieldTransferRate);
                }
            }
            else if (forwardOverShieldHealth < totalForwardShieldHealth && rearShieldHealth == totalRearShieldHealth && rearOverShieldHealth == 0f) // forward shield is below the half limit, rear shield is at the limit
            {
                addToForwardShield(shieldChange);
                if (rearShieldHealth < shieldTransferRate)
                {
                    addToForwardShield(rearShieldHealth);
                    subtractToRearShield(-rearShieldHealth);
                }
                else
                {
                    addToForwardShield(shieldTransferRate);
                    subtractToRearShield(-shieldTransferRate);
                }
            }
            else if (forwardOverShieldHealth == totalForwardShieldHealth && rearShieldHealth < totalRearShieldHealth) // forward shield is at the limit, rear shield is below the limit
            {
                addToRearShield(shieldChange);
            }
            else if (forwardOverShieldHealth < totalForwardShieldHealth && rearOverShieldHealth > 0) // forward over shield below limit, rear overshield above limit
            {
                if (rearOverShieldHealth + rearShieldHealth < shieldTransferRate)
                {
                    addToForwardShield(shieldChange + rearOverShieldHealth + rearShieldHealth);
                    subtractToRearShield(-(rearOverShieldHealth + rearShieldHealth));
                }
                else
                {
                    addToForwardShield(shieldChange + shieldTransferRate);
                    subtractToRearShield(-shieldTransferRate);
                }
            }
            else // if meets this condition, needs to be reset at limits
            {
                forwardShieldHealth = totalForwardShieldHealth;
                forwardOverShieldHealth = totalForwardShieldHealth;
                rearShieldHealth = totalRearShieldHealth;
                rearOverShieldHealth = 0f;
            }
        }
        else if (shieldFocusStatus == -1 && shieldChange >= 0)
        {
            if (forwardShieldHealth < totalForwardShieldHealth && rearOverShieldHealth < totalRearShieldHealth) // both rear and forward are below the limit
            {
                addToForwardShield(shieldChange / 2f);
                addToRearShield(shieldChange / 2f);
                if (forwardShieldHealth < shieldTransferRate)
                {
                    addToRearShield(forwardShieldHealth);
                    subtractToForwardShield(-forwardShieldHealth);
                }
                else
                {
                    addToRearShield(shieldTransferRate);
                    subtractToForwardShield(-shieldTransferRate);
                }
            }
            else if (rearOverShieldHealth < totalRearShieldHealth && forwardShieldHealth == totalForwardShieldHealth && forwardOverShieldHealth == 0f) // forward shield at limit, rear shield below limit
            {
                addToRearShield(shieldChange);
                if (forwardShieldHealth < shieldTransferRate)
                {
                    addToRearShield(forwardShieldHealth);
                    subtractToForwardShield(-forwardShieldHealth);
                }
                else
                {
                    addToRearShield(shieldTransferRate);
                    subtractToForwardShield(-shieldTransferRate);
                }
            }
            else if (rearOverShieldHealth == totalRearShieldHealth && forwardShieldHealth < totalForwardShieldHealth) // rear shield at limit, forward shield below limit
            {
                addToForwardShield(shieldChange);
            }
            else if (rearOverShieldHealth < totalRearShieldHealth && forwardOverShieldHealth > 0) // rear over shield below limit, forward overshield above limit
            {
                if (forwardOverShieldHealth + forwardShieldHealth < shieldTransferRate)
                {
                    addToRearShield(shieldChange + forwardOverShieldHealth + forwardShieldHealth);
                    subtractToForwardShield(-(forwardOverShieldHealth + forwardShieldHealth));
                }
                else
                {
                    addToRearShield(shieldChange + shieldTransferRate);
                    subtractToForwardShield(-shieldTransferRate);
                }
            }
            else // if meets this condition, needs to be reset at limits
            {
                forwardShieldHealth = totalForwardShieldHealth;
                forwardOverShieldHealth = 0f;
                rearShieldHealth = totalRearShieldHealth;
                rearOverShieldHealth = totalRearShieldHealth;
            }
        }
        else if (shieldFocusStatus == 0 && shieldChange < 0)
        {
            subtractToForwardShield(shieldChange / 2f);
            subtractToRearShield(shieldChange / 2f);

            if (forwardOverShieldHealth > totalForwardShieldHealth / 2f) // forward over shield above limit
            {
                if (forwardOverShieldHealth - totalForwardShieldHealth / 2f < shieldTransferRate)
                {
                    addToRearShield(forwardOverShieldHealth - totalForwardShieldHealth / 2f);
                    subtractToForwardShield(-(forwardOverShieldHealth - totalForwardShieldHealth / 2f));
                }
                else
                {
                    addToRearShield(shieldTransferRate);
                    subtractToForwardShield(-shieldTransferRate);
                }
            }
            else if (rearOverShieldHealth > totalRearShieldHealth / 2f) // rear overshield above limit
            {
                if (rearOverShieldHealth - totalRearShieldHealth / 2f < shieldTransferRate)
                {
                    addToForwardShield(rearOverShieldHealth - totalRearShieldHealth / 2f);
                    subtractToRearShield(-(rearOverShieldHealth - totalRearShieldHealth / 2f));
                }
                else
                {
                    addToForwardShield(shieldTransferRate);
                    subtractToRearShield(-shieldTransferRate);
                }
            }

        }
        else if (shieldFocusStatus == 1 && shieldChange < 0)
        {
            subtractToForwardShield(shieldChange / 2f);
            subtractToRearShield(shieldChange / 2f);
            if (forwardOverShieldHealth < totalForwardShieldHealth)
            {
                if (rearShieldHealth < shieldTransferRate)
                {
                    addToForwardShield(rearShieldHealth);
                    subtractToRearShield(-rearShieldHealth);
                }
                else
                {
                    addToForwardShield(shieldTransferRate);
                    subtractToRearShield(-shieldTransferRate);
                }
            }
        }
        else if (shieldFocusStatus == -1 && shieldChange < 0)
        {
            subtractToForwardShield(shieldChange / 2f);
            subtractToRearShield(shieldChange / 2f);
            if (rearOverShieldHealth < totalRearShieldHealth)
            {
                if (forwardShieldHealth < shieldTransferRate)
                {
                    addToRearShield(forwardShieldHealth);
                    subtractToForwardShield(-forwardShieldHealth);
                }
                else
                {
                    addToRearShield(shieldTransferRate);
                    subtractToForwardShield(-shieldTransferRate);
                }
            }
        }
    }

    protected void ShieldedPlayerFixedUpdate()
    {
        updateShieldCharge();
    }
}
