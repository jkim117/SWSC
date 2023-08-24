using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ShipController : MonoBehaviour
{
    // MOVEMENT VALUES CONFIGURABLE
    protected float gravAcceleration = 0.7f;
    protected float maxGravTerminalValue = 53.0f;
    protected float yawLimit = 0.75f;
    protected float pitchLimit = 1.0f;
    public float maxRepulsorValue;
    //MOVEMENT VALUES VARIABLE
    public float throttleLevel;
    public float activeSpeed;
    protected float throttleStatus; // -1 for decreasing, 0 for static, 1 for increasing
    protected float pitchValue;
    protected float yawValue;
    protected float rollStatus;
    protected float repulsorStatus; // -1 for decreasing, 0 for static, 1 for increasing
    protected float repulsorLevel;
    
    // POWER VALUES VARIABLE
    public int enginePower; // value from 0 to 8 inclusive
    public int laserPower; // value from 0 to 4 (no power, losing power, maintenance, increased, max)
    public int shieldPower; // value from 0 to 4 (no power, losing power, maintenance, increased, max)
    public int overallPower; // determines the intensity of power generation. Max value of 12

    // TARGETIGN VALUES VARIABLE
    //public List<List<ShipController>> targetList = new List<List<ShipController>>();
    public int targetListIndex = 0;
    public int targetSubListIndex = -1; // not selected anything at start
    public int targetComponentIndex = -1;

    public int lockingOnWarning = 0;
    public int lockedOnWarning = 0;
    public int incomingWarning = 0;

    public bool friendly;

    protected bool lockingOn = false;
    protected bool lockedOn = false;
    public float sensorBurnLimit = 0;
    public float sensorRangeLimit = 1000f;
    public bool offOpposingList = true;
    protected float despawnThreshold = SpawnGameAssets.despawnThreshold;

    public List<SubComponentController> subcomponentList;

    // Start is called before the first frame update
    protected void ShipControllerStart()
    {
        repulsorLevel = maxGravTerminalValue;
    }

    protected void ShipControllerUpdate()
    {
        // automatically despawn shipcontroller if outside a certain threshold of distance
        if (Vector2.Distance(Vector2.zero, new Vector2(transform.position.x, transform.position.z)) > despawnThreshold)
        {
            destroyShipController();
        }
    }

    public virtual void destroyShipController()
    {
        return;
    }

    public virtual void toggleTargetMarker(bool setActive)
    {
        return;
    }

    public virtual Vector3 getVelocity()
    {
        return new Vector3(0, 0, 0);
    }

    public virtual Vector3 getPosition()
    {
        return transform.position;
    }

    public virtual Vector3 getJamPosition(bool friendly, out bool jammed)
    {
        jammed = false;
        return transform.position;
    }

    public virtual float getHealthPercentage()
    {
        return 0f;
    }

    public virtual float getShieldPercentage()
    {
        return -1f;
    }

    public virtual float getRepulsorPercentage()
    {
        return 100 * repulsorLevel / maxRepulsorValue;
    }

    public virtual string getShipName()
    {
        return "";
    }

    public virtual ShipController getTarget()
    {
        return null;
    }

    public virtual float getLaserCharge()
    {
        return 0f;
    }

    public virtual float getJammer()
    {
        return 0f;
    }    

    public virtual float getLaserConvergence()
    {
        return 300f;
    }

    public virtual float getMissileConvergence()
    {
        return 1000f;
    }

    public virtual int getCurrentWeaponSystem()
    {
        return 0;
    }

    public virtual int getCurrentWeaponAmmoCount()
    {
        return 0;
    }

    public virtual float getFrontShield()
    {
        return 0f;
    }

    public virtual float getFrontOverShield()
    {
        return 0f;
    }

    public virtual float getRearShield()
    {
        return 0f;
    }
    public virtual float getRearOverShield()
    {
        return 0f;
    }

    public virtual int getShieldFocusStatus()
    {
        return 0;
    }

}
