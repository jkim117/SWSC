using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIController : ShipController
{
    // MOVEMENT VALUES CONFIGURABLE
    protected float rollValue; // determines speed of rolls, derived from DPF
    protected float rollAcceleration; // acceleration of rolls, derived from G
    protected float lookRateSpeedPitch; // determines the speed of pitching/yawing, derived from DPF
    protected float lookRateSpeedYaw; // derived from DPF
    protected float forwardAcceleration; // determines main engine accleration value, derived from G
    protected float maxSpeed; // determines max forward speed, derived from kph
    protected float repulsorAcceleration; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
    protected float pitchAcceleration; // derived from DPF and G
    protected float yawAcceleration; // derived from DPF and G
    private float landedModeThreshold = 60;
    // MOVEMENT VALUES VARIABLE
    private float collisionNoMovementThreshold = 10.0f;
    protected bool driftStatus = false;
    private bool landedMode = false;
    protected Rigidbody shipRigidBody;
    private float realForwardAcceleration;
    protected float realSpeedGoal;
    private float realMaxSpeed;
    private float activeRoll;
    private float activeForwardSpeed;
    private float activeYawSpeed;
    private float activePitchSpeed;
    public float activeRepulsor;
    private float activeGravity;
    private List<Vector3> collisionVectorList;
    protected Vector3 totalVelocityVector;

    private Transform targetMarker;

    private float maxJammer = 30f;
    private float currentJammer = 0f;
    private Vector3 jammerLocation;
    protected bool jammed;

    public CapShipController parentCapShip;

    class DriftComponent
    {
        public Vector3 driftVector;
        public float driftSpeed;

        public DriftComponent(Vector3 dv, float ds)
        {
            driftVector = dv;
            driftSpeed = ds;
        }
    }
    private List<DriftComponent> driftList;

    private bool collisionRotation = false;
    private float activeYawCollisionRotation;
    private float goalYawCollisionRotation;
    private float activePitchCollisionRotation;
    private float goalPitchCollisionRotation;

    // SHIP HEALTH VALUES CONFIGURABLE
    protected float totalShipHealth;
    // SHIP HEALTH VALUES VARIABLE
    public float shipHealth;
    public float shipIonHealth;

    // Power Management Values Configurable Values
    protected int overallPowerLimit = 10;
    protected int enginePowerLimit = 8;
    protected int otherPowerLimit = 4;
    protected float laserChargeLimit = 50f;
    // POWER VALUES VARIABLE
    protected float laserCharge = 50f;
    protected float laserChargeTimestamp = 0f;
    protected int storedEnginePower;
    protected int storedLaserPower;
    protected int storedShieldPower;
    protected bool ionStatus = false;
    protected float chargeRate0 = 0.5f;
    protected float chargeRate1 = 1f;
    protected float chargeRate3 = 0.5f;
    protected float chargeRate4 = 0.25f;

    // TARGETIGN VALUES VARIABLE
    public ShipController currentTarget;
    /*public List<ShipController> enemyFighterList = new List<ShipController>();
    public List<ShipController> enemyCapitalShipTargetList = new List<ShipController>();
    public List<ShipController> friendlyFighterList = new List<ShipController>();
    public List<ShipController> friendlyCapitalShipTargetList = new List<ShipController>();
    public List<ShipController> objectiveList = new List<ShipController>();*/
    protected bool targetingComputerToggle = true;

    // EFFECTS VALUES CONFIGURABLE
    public GameObject explosionPrefab;
    public AudioClip explosionClip;
    public AudioSource shipSounds; // audio source used for ship sounds


    // Start is called before the first frame update
    protected void AIControllerStart()
    {
        if (friendly)
        {
            CrossSceneValues.friendlyFighterList.Add(this);
        }
        else
        {
            CrossSceneValues.enemyFighterList_enemy.Add(this);
        }
        
        /*targetList.Add(enemyFighterList);
        targetList.Add(enemyCapitalShipTargetList);
        targetList.Add(friendlyFighterList);
        targetList.Add(friendlyCapitalShipTargetList);
        targetList.Add(objectiveList);*/

        driftList = new List<DriftComponent>();
        collisionVectorList = new List<Vector3>();
        shipRigidBody = GetComponent<Rigidbody>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Confined;

        currentTarget = null;
        targetMarker = transform.Find("Canvas");
        targetMarker.gameObject.SetActive(false);
        parentCapShip.currentNumberFighters++;
    }

    //************************************************OVERRIDE FUNCTIONS************************************************//
    public override ShipController getTarget()
    {
        return currentTarget;
    }
    public override float getLaserCharge()
    {
        return laserCharge / laserChargeLimit;
    }
    public override float getHealthPercentage()
    {
        return (float)Mathf.Round((float)shipHealth / (float)totalShipHealth * 100f);
    }
    public override void toggleTargetMarker(bool setActive)
    {
        targetMarker.gameObject.SetActive(setActive);
    }

    public override Vector3 getVelocity()
    {
        return totalVelocityVector;
    }

    public override float getJammer()
    {
        return 100 * currentJammer / maxJammer;
    }

    public override Vector3 getJamPosition(bool otherIsFriendly, out bool otherjammed)
    {
        otherjammed = false;
        if (!otherIsFriendly && friendly && currentJammer > 0)
        {
            foreach (ShipController s in CrossSceneValues.enemyFighterList)
            {
                if (Vector3.Distance(s.transform.position, transform.position) < s.sensorBurnLimit)
                {
                    return transform.position;
                }
            }
            foreach (ShipController s in CrossSceneValues.enemyCapitalShipTargetList)
            {
                if (Vector3.Distance(s.transform.position, transform.position) < s.sensorBurnLimit)
                {
                    return transform.position;
                }
            }
            otherjammed = true;
            return transform.position + jammerLocation;
        }
        else if (otherIsFriendly && !friendly && currentJammer > 0)
        {
            foreach (ShipController s in CrossSceneValues.friendlyFighterList)
            {
                if (Vector3.Distance(s.transform.position, transform.position) < s.sensorBurnLimit)
                {
                    return transform.position;
                }
            }
            foreach (ShipController s in CrossSceneValues.friendlyCapitalShipTargetList)
            {
                if (Vector3.Distance(s.transform.position, transform.position) < s.sensorBurnLimit)
                {
                    return transform.position;
                }
            }
            otherjammed = true;
            return transform.position + jammerLocation;
        }
        return transform.position;
    }

    public override void destroyShipController()
    {
        parentCapShip.currentNumberFighters--;
        if (friendly)
        {
            CrossSceneValues.friendlyFighterList.Remove(this);
            CrossSceneValues.friendlyFighterList_enemy.Remove(this);
        }
        else
        {
            CrossSceneValues.enemyFighterList.Remove(this);
            CrossSceneValues.enemyFighterList_enemy.Remove(this);
        }
        if (lockedOn)
        {
            currentTarget.lockedOnWarning--;
            lockedOn = false;
        }
        if (lockingOn)
        {
            currentTarget.lockingOnWarning--;
            lockingOn = false;
        }

        Destroy(gameObject);
    }

    private void checkOpposingScannerDistance(bool friendly)
    {
        if (friendly)
        { 
            bool removeFromList = true;
            foreach (ShipController s in CrossSceneValues.enemyFighterList_enemy)
            {
                if (Vector3.Distance(s.transform.position, transform.position) < s.sensorRangeLimit)
                {
                    removeFromList = false;
                    break;
                }
            }
            if (removeFromList)
            {
                foreach (ShipController s in CrossSceneValues.enemyCapitalShipTargetList_enemy)
                {
                    if (Vector3.Distance(s.transform.position, transform.position) < s.sensorRangeLimit)
                    {
                        removeFromList = false;
                        break;
                    }
                }
            }
            if (removeFromList && !offOpposingList)
            {
                CrossSceneValues.friendlyFighterList_enemy.Remove(this);
                offOpposingList = true;
            }
            else if (!removeFromList && offOpposingList)
            {
                CrossSceneValues.friendlyFighterList_enemy.Add(this);
                offOpposingList = false;
            }
            
        }
        else
        {
            bool removeFromList = true;
            foreach (ShipController s in CrossSceneValues.friendlyFighterList)
            {
                if (Vector3.Distance(s.transform.position, transform.position) < s.sensorRangeLimit)
                {
                    removeFromList = false;
                    break;
                }
            }
            if (removeFromList)
            {
                foreach (ShipController s in CrossSceneValues.friendlyCapitalShipTargetList)
                {
                    if (Vector3.Distance(s.transform.position, transform.position) < s.sensorRangeLimit)
                    {
                        removeFromList = false;
                        break;
                    }
                }
            }
            if (removeFromList && !offOpposingList)
            {
                CrossSceneValues.enemyFighterList.Remove(this);
                offOpposingList = true;
            }
            else if (!removeFromList && offOpposingList)
            {
                CrossSceneValues.enemyFighterList.Add(this);
                offOpposingList = false;
            }
        }
    }

    //************************************************INPUT FUNCTIONS************************************************//
    protected void OnToggleTargetingComputer()
    {
        if (targetingComputerToggle)
        {
            targetingComputerToggle = false;
        }
        else
        {
            targetingComputerToggle = true;
        }
    }

    protected void randomCycleTargets()
    {
        if (friendly)
        {
            if (CrossSceneValues.targetList[targetListIndex].Count <= 0)
            {
                return;
            }
            targetSubListIndex = Random.Range(0, CrossSceneValues.targetList[targetListIndex].Count);
            currentTarget = CrossSceneValues.targetList[targetListIndex][targetSubListIndex];
        }
        else
        {
            if (CrossSceneValues.targetList_enemy[targetListIndex].Count <= 0)
            {
                return;
            }
            targetSubListIndex = Random.Range(0, CrossSceneValues.targetList_enemy[targetListIndex].Count);
            currentTarget = CrossSceneValues.targetList_enemy[targetListIndex][targetSubListIndex];
        }
        
        //Debug.Log(CrossSceneValues.targetList[targetListIndex].Count);

        
        //Debug.Log(targetSubListIndex);
        
    }

    protected void OnCycleTargets()
    {
        if (friendly)
        {
            if (CrossSceneValues.targetList[targetListIndex].Count <= 0)
            {
                return;
            }
            targetSubListIndex = (targetSubListIndex + 1) % CrossSceneValues.targetList[targetListIndex].Count;
            currentTarget = CrossSceneValues.targetList[targetListIndex][targetSubListIndex];
        }
        else
        {
            if (CrossSceneValues.targetList_enemy[targetListIndex].Count <= 0)
            {
                return;
            }
            targetSubListIndex = (targetSubListIndex + 1) % CrossSceneValues.targetList_enemy[targetListIndex].Count;
            currentTarget = CrossSceneValues.targetList_enemy[targetListIndex][targetSubListIndex];
        }
        

        
        
    }
    int sortByDistanceToPlayer(ShipController a, ShipController b)
    {
        return Vector3.Magnitude(transform.position - a.getPosition()).CompareTo(Vector3.Magnitude(transform.position - b.getPosition()));
    }
    /*void OnResetTargets()
    {
        if (CrossSceneValues.targetList[targetListIndex].Count <= 0)
        {
            currentTarget = null;
            return;
        }

        CrossSceneValues.targetList[targetListIndex].Sort(sortByDistanceToPlayer);
        targetSubListIndex = 0;
        currentTarget = CrossSceneValues.targetList[targetListIndex][targetSubListIndex];
    }*/
    protected void OnCycleTargetOptions()
    {
        if (friendly)
        {
            targetListIndex = (targetListIndex + 1) % CrossSceneValues.targetList.Count;
        }
        else
        {
            targetListIndex = (targetListIndex + 1) % CrossSceneValues.targetList_enemy.Count;
        }
        
        //OnResetTargets();
    }

    protected void OnThrottle(float value)
    {
        if (ionStatus)
        {
            return;
        }
        throttleStatus = value;
    }

    void OnRepulsorStabilize()
    {
        if (ionStatus)
        {
            return;
        }
        repulsorLevel = maxGravTerminalValue;
    }

    protected void OnRoll(float value)
    {
        if (ionStatus)
        {
            return;
        }
        rollStatus = value;
    }

    protected void OnPitch(float value)
    {
        if (ionStatus)
        {
            return;
        }
        pitchValue = value;
    }
    protected void OnYaw(float value)
    {
        if (ionStatus)
        {
            return;
        }
        yawValue = value;
    }
    void OnRepulsorChange(float value)
    {
        if (ionStatus)
        {
            return;
        }
        repulsorStatus = value;
    }
    void OnLasersToJammer()
    {
        if (laserCharge >= 10 && currentJammer <= maxJammer - 1)
        {
            if (currentJammer <= 0)
            {
                jammerLocation = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            }
            laserCharge = laserCharge - 10;
            currentJammer += 1;
        }
    }

    //************************************************MOVEMENT FUNCTIONS************************************************//
    Vector2 mVectorProcess(Vector2 mVector)
    {

        //mVector.x = Mathf.Pow(mVector.x / 75f, 3);
        if (mVector.x > pitchLimit)
        {
            mVector.x = pitchLimit;
        }
        else if (mVector.x < -pitchLimit)
        {
            mVector.x = -pitchLimit;
        }

        //mVector.y = Mathf.Pow(mVector.y / 75f, 3);
        if (mVector.y > yawLimit)
        {
            mVector.y = yawLimit;
        }
        else if (mVector.y < -yawLimit)
        {
            mVector.y = -yawLimit;
        }

        if (mVector.magnitude < 0.1)
        {
            mVector = new Vector2(0, 0);
        }

        return mVector;
    }

    protected virtual float calcMFactor(int ePower, int maxEPower, float tLevel)
    {
        float throttleFactor = 1f + 12f / (10f * Mathf.Sqrt(2f * Mathf.PI)) * Mathf.Exp(-Mathf.Pow(tLevel - 50, 2) / 200f);

        float mFactorBoost = 1f;
        if (driftStatus)
        {
            mFactorBoost = 1.5f;
        }

        return (ePower + 20f) / ((float)maxEPower + 20f) * throttleFactor * mFactorBoost;
    }

    void handleMovement()
    {
        Vector2 mouseVector = new Vector2(pitchValue, yawValue);
        mouseVector = mVectorProcess(mouseVector);
        mouseVector = Vector2.ClampMagnitude(mouseVector, 1f);



        throttleLevel += throttleStatus;
        if (throttleLevel <= 0)
        {
            throttleLevel = 0;
        }
        else if (throttleLevel >= 100)
        {
            throttleLevel = 100;
        }

        repulsorLevel += repulsorStatus;
        if (repulsorLevel <= 0)
        {
            repulsorLevel = 0;
        }
        else if (repulsorLevel >= maxRepulsorValue)
        {
            repulsorLevel = maxRepulsorValue;
        }

        if (landedMode && repulsorLevel <= maxGravTerminalValue)
        {
            repulsorLevel = maxGravTerminalValue;
        }

        float turningSpeedDrop = 1f - (mouseVector.magnitude * 0.3f);
        realMaxSpeed = enginePower / (float)enginePowerLimit * maxSpeed * turningSpeedDrop;
        if (throttleLevel < 100)
        {
            realSpeedGoal = Mathf.Lerp(0.0f, 0.9f * realMaxSpeed, throttleLevel / 100.0f);

        }
        else
        {
            realSpeedGoal = 1.1f * realMaxSpeed;
        }
        realForwardAcceleration = Mathf.Lerp(forwardAcceleration * 0.1f, forwardAcceleration, enginePower / (float)enginePowerLimit);


        if (enginePower == 0 && driftStatus == false)
        {
            driftList.Add(new DriftComponent(-transform.up, activeForwardSpeed));
            activeForwardSpeed = 0.0f;
            driftStatus = true;
        }
        else if (enginePower > 0 && driftStatus == true)
        {
            driftStatus = false;
            activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, realSpeedGoal, realForwardAcceleration * Time.fixedDeltaTime);
        }
        else if (enginePower > 0 && driftStatus == false)
        {
            activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, realSpeedGoal, realForwardAcceleration * Time.fixedDeltaTime);
        }

        Vector3 driftVector = new Vector3();
        List<DriftComponent> newDriftList = new List<DriftComponent>();
        foreach (DriftComponent dc in driftList)
        {

            foreach (Vector3 collisionVector in collisionVectorList)
            {
                if (Vector3.Project(dc.driftVector, collisionVector).normalized == collisionVector)
                {
                    Vector3 alteredDriftVector = dc.driftVector * dc.driftSpeed - Vector3.Project(dc.driftVector, collisionVector);
                    dc.driftSpeed = alteredDriftVector.magnitude;
                    dc.driftVector = alteredDriftVector.normalized;
                }
            }

            if (dc.driftSpeed > 0.1f)
            {
                newDriftList.Add(dc);
            }
            dc.driftSpeed = Mathf.Lerp(dc.driftSpeed, 0.0f, realForwardAcceleration * Time.fixedDeltaTime);
            driftVector += dc.driftSpeed * dc.driftVector;
        }
        driftList = newDriftList;
        float maneuverabilityFactor = calcMFactor(enginePower, enginePowerLimit, throttleLevel);
        float activeRepulsorAcceleration = repulsorAcceleration * maneuverabilityFactor;
        float gravTerminalValue = maxGravTerminalValue;

        float angleToGravitationalField = Vector3.Angle(-transform.forward, Vector3.down);
        if (angleToGravitationalField > 90 && angleToGravitationalField < 270) // reverse repulsor field if upside down
        {
            activeRepulsor = Mathf.Lerp(activeRepulsor, -repulsorLevel, activeRepulsorAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            activeRepulsor = Mathf.Lerp(activeRepulsor, repulsorLevel, activeRepulsorAcceleration * Time.fixedDeltaTime);
        }
        activeGravity = Mathf.Lerp(activeGravity, gravTerminalValue, gravAcceleration * Time.fixedDeltaTime);

        
        Vector3 forwardSpeedVector = -transform.up * activeForwardSpeed;
        Vector3 gravityVector = Vector3.down * activeGravity;
        Vector3 repulsorVector = transform.forward * activeRepulsor;



        List<Vector3> newCollisionVectorList = new List<Vector3>();
        foreach (Vector3 collisionVector in collisionVectorList)
        {

            if (Vector3.Project(forwardSpeedVector, collisionVector).normalized == collisionVector)
            {
                forwardSpeedVector = forwardSpeedVector - Vector3.Project(forwardSpeedVector, collisionVector);
                activeForwardSpeed = forwardSpeedVector.magnitude;
            }
            if (Vector3.Project(gravityVector, collisionVector).normalized == collisionVector)
            {
                gravityVector = gravityVector - Vector3.Project(gravityVector, collisionVector);
                activeGravity = gravityVector.magnitude;
            }
            if (Vector3.Project(repulsorVector, collisionVector).normalized == collisionVector)
            {
                repulsorVector = repulsorVector - Vector3.Project(repulsorVector, collisionVector);
                activeRepulsor = repulsorVector.magnitude;
            }
        }
        //totalVelocityVector = forwardSpeedVector + gravityVector + repulsorVector + driftVector; // TEMP for AI
        totalVelocityVector = forwardSpeedVector + driftVector;

        collisionVectorList = newCollisionVectorList;



        activeSpeed = totalVelocityVector.magnitude * 3.6f; //total speed converted to kph

        if (activeSpeed > landedModeThreshold)
        {
            landedMode = false;
        }
        if (!landedMode)
        {
            shipRigidBody.MovePosition(shipRigidBody.position + Time.fixedDeltaTime * totalVelocityVector);
        }


        activeRoll = Mathf.Lerp(activeRoll, rollValue * rollStatus, rollAcceleration * Time.fixedDeltaTime);


        float goalPitch = -mouseVector.x * maneuverabilityFactor * lookRateSpeedPitch;
        float goalYaw = mouseVector.y * maneuverabilityFactor * lookRateSpeedYaw;


        activePitchSpeed = Mathf.Lerp(activePitchSpeed, goalPitch, maneuverabilityFactor * pitchAcceleration * Time.fixedDeltaTime);
        activeYawSpeed = Mathf.Lerp(activeYawSpeed, goalYaw, maneuverabilityFactor * yawAcceleration * Time.fixedDeltaTime);

        activeYawCollisionRotation = Mathf.Lerp(activeYawCollisionRotation, goalYawCollisionRotation, Time.fixedDeltaTime * 10f);
        activePitchCollisionRotation = Mathf.Lerp(activePitchCollisionRotation, goalPitchCollisionRotation, Time.fixedDeltaTime * 10f);
        if (collisionRotation && Mathf.Abs(activeYawCollisionRotation - goalYawCollisionRotation) < 0.1f && Mathf.Abs(activePitchCollisionRotation - goalPitchCollisionRotation) < 0.1f)
        {
            collisionRotation = false;
            goalPitchCollisionRotation = 0f;
            goalYawCollisionRotation = 0f;
        }

        if (!landedMode)
        {
            shipRigidBody.MoveRotation(shipRigidBody.rotation * Quaternion.Euler(activePitchSpeed * Time.fixedDeltaTime + activePitchCollisionRotation, activeRoll * Time.fixedDeltaTime, activeYawSpeed * Time.fixedDeltaTime + activeYawCollisionRotation));
        }
    }

    //************************************************DAMAGE/COLLISION FUNCTIONS************************************************//
    protected virtual void takeDamage(float damage, bool frontOrRear)
    {
        shipHealth -= damage;

        if (shipHealth <= 0)
        {
            GameObject explosion = GameObject.Instantiate(explosionPrefab, transform.position, transform.rotation);
            explosion.GetComponent<ParticleSystem>().Play();
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);
            destroyShipController();
        }
    }
    protected virtual void takeIonDamage(float damage, bool frontOrRear)
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        /*foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
        if (collision.relativeVelocity.magnitude > 2)
            audioSource.Play();*/
        //Debug.Log(collision.relativeVelocity.magnitude);


        ContactPoint contact = collision.GetContact(0);
        Vector3 contactPointFromCenter = (contact.point - transform.position);

        bool frontDamage = collision.relativeVelocity.x > 0;

        if (collision.gameObject.ToString().Contains("shot_prefab"))
        {
            ShotBehavior sb = collision.gameObject.GetComponent<ShotBehavior>();
            if (sb.isIonDamage())
            {
                takeIonDamage(sb.getDamage(), frontDamage);
            }
            else
            {
                takeDamage(sb.getDamage(), frontDamage);
            }
            return;
        }

        
        // IF missile collision
        if ("concussion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(100, frontDamage);
            return;
        }
        // IF proton torp collision
        if ("proton_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(500, frontDamage);
            return;
        }
        // IF missile collision
        if ("ion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(500, frontDamage);
            return;
        }
        // IF ion torp collision
        if ("ion_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(5000, frontDamage);
            return;
        }
        // IF proton bomb collision
        if ("proton_bomb_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(1000, frontDamage);
            return;
        }
        // IF ion bomb collision
        if ("ion_bomb_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(6000, frontDamage);
            return;
        }

        takeDamage((int)(collision.relativeVelocity.magnitude * 2 / contactPointFromCenter.magnitude), frontDamage);

        collisionRotation = true;
        activeYawCollisionRotation = 0f;
        activePitchCollisionRotation = 0f;

        if (collision.relativeVelocity.magnitude > 50f)
        {
            goalYawCollisionRotation = contactPointFromCenter.x * 50f / 100f;
            goalPitchCollisionRotation = contactPointFromCenter.y * 50f / 100f;
        }
        else
        {
            goalYawCollisionRotation = contactPointFromCenter.x * collision.relativeVelocity.magnitude / 100f;
            goalPitchCollisionRotation = contactPointFromCenter.y * collision.relativeVelocity.magnitude / 100f;
        }


        //shipRigidBody.MoveRotation(Quaternion.Euler(contactPointFromCenter.x, contactPointFromCenter.y, contactPointFromCenter.z));

        if (!landedMode)
        {
            if (collision.relativeVelocity.magnitude < collisionNoMovementThreshold)
            {
                if (activeSpeed < landedModeThreshold && Vector3.Angle(collision.relativeVelocity.normalized, Vector3.up) < 45 && Mathf.Abs(transform.localRotation.eulerAngles.x - (270f)) < 30f)
                {
                    landedMode = true;
                    return;
                }
                collisionVectorList.Add(-collision.relativeVelocity.normalized);
                driftList.Add(new DriftComponent(collision.relativeVelocity.normalized, collisionNoMovementThreshold));
            }
            else if (collision.relativeVelocity.magnitude > 35f)
            {
                collisionVectorList.Add(-collision.relativeVelocity.normalized);
                driftList.Add(new DriftComponent(collision.relativeVelocity.normalized, 35.0f));
            }
            else
            {
                collisionVectorList.Add(-collision.relativeVelocity.normalized);
                //driftList.Add(new DriftComponent(collision.relativeVelocity.normalized, collisionNoMovementThreshold));
                driftList.Add(new DriftComponent(collision.relativeVelocity.normalized, collision.relativeVelocity.magnitude));
                //driftList.Add(new DriftComponent(collision.relativeVelocity.normalized, collisionNoMovementThreshold));
            }
        }

        //noMovementVector = -collision.relativeVelocity.normalized;
        //noMovementToggle = true;
        //Debug.Log(collision.collider);
    }

    // //************************************************POWER FUNCTIONS************************************************//
    protected virtual void updateLaserCharge()
    {
        if (laserPower == 0) // loses two laser charge every second
        {
            if (laserCharge == 0)
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge--;
                laserChargeTimestamp = Time.time + chargeRate0;
            }
        }
        else if (laserPower == 1) // loses a laser charge every second
        {
            if (laserCharge == 0)
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge--;
                laserChargeTimestamp = Time.time + chargeRate1;
            }
        }
        else if (laserPower == 2) // static power generation
        {
            return;
        }
        else if (laserPower == 3) // gains a lser charge every second
        {
            if (laserCharge == laserChargeLimit)
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge++;
                laserChargeTimestamp = Time.time + chargeRate3;
            }
        }
        else
        {
            if (laserCharge == laserChargeLimit) // gains two laser charge every second
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge++;
                laserChargeTimestamp = Time.time + chargeRate4;
            }
        }
    }

    private float jammerDelayFull = 1.5f;
    private float jammerActivateDelay = 0.5f;
    private void incomingMissileAction()
    {
        
        if (incomingWarning > 0 && currentJammer <= 0)
        {
            if (jammerActivateDelay <= 0)
            {
                OnLasersToJammer();
                OnLasersToJammer();
                OnLasersToJammer();
                jammerActivateDelay = Random.Range(0.5f, jammerDelayFull);
            }
            jammerActivateDelay -= Time.fixedDeltaTime;
        }
    }

    protected void AIControllerUpdate()
    {
        Transform aiCanvas = transform.Find("Canvas");
        aiCanvas.rotation = Camera.main.transform.rotation;

        float cameraDist = Vector3.Magnitude((aiCanvas.position - Camera.main.transform.position));
        aiCanvas.Find("TargetFrame").localScale = new Vector3(cameraDist, cameraDist, cameraDist) / 300;

        if (parentCapShip == null)
        {
            destroyShipController();
        }
    }

    protected void AIControllerFixedUpdate()
    {
        handleMovement();
        if (currentTarget == null)
        {
            targetSubListIndex = -1;
        }
        updateLaserCharge();
        incomingMissileAction();
        checkOpposingScannerDistance(friendly);

        if (shipIonHealth <= totalShipHealth) // restore ion health
        {
            shipIonHealth += Time.fixedDeltaTime * 2f;
            if (shipIonHealth > totalShipHealth)
            {
                shipIonHealth = totalShipHealth;
            }
        }
        if (currentJammer > 0)
        {
            currentJammer -= Time.fixedDeltaTime;
        }
        if (currentTarget != null)
        {
            if (currentTarget.offOpposingList)
            {
                currentTarget = null;
            }
        }

    }
}
