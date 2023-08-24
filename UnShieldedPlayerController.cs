using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class UnShieldedPlayerController : PlayerController
{
    // POWER VALUES VARIABLE
    private bool engineDown = false;
    private bool laserDown = false;
    private bool engineActionPerformed = false;
    private bool laserActionPerformed = false;

    protected override void takeIonDamage(float damage, bool frontOrRear)
    {
        shipIonHealth -= damage;

        if (shipIonHealth <= 0)
        {
            shipIonHealth = 0;
            for (int i = 0; i < 10; i++)
            {
                OnDecreaseOverallPower();
            }
            ionStatus = true;
            laserCharge = 0;
        }
    }

    ////************************************************INPUT FUNCTIONS************************************************//
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

        if (!engineDown && !laserDown && !engineActionPerformed)
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
        if (!laserDown && !engineDown && !laserActionPerformed)
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
        }

    }

}
