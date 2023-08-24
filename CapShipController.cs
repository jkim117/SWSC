using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapShipController : ShipController
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
    // MOVEMENT VALUES VARIABLE
    protected Rigidbody shipRigidBody;
    protected float realSpeedGoal;
    private float realMaxSpeed;
    private float activeRoll;
    private float activeForwardSpeed;
    private float activeYawSpeed;
    private float activePitchSpeed;
    protected Vector3 totalVelocityVector;

    // SHIP HEALTH VALUES CONFIGURABLE
    public float totalShipHealth;
    public float shipHealth;

    protected bool ionStatus = false;
    
    // EFFECTS VALUES CONFIGURABLE
    public GameObject explosionPrefab;
    public AudioClip explosionClip;
    public AudioSource shipSounds; // audio source used for ship sounds

    private Transform targetMarker;
    public ShipController player;

    public List<TLController> TLList;
    public GameObject shipFires1;
    public GameObject shipFires2;
    public GameObject shipFires3;

    private float fighterProbCutOff = 0.5f;
    private float capShipProbCutOff = 1f;
    public string shipName;

    private int aiStatus = 0; // 0 is defensive, 1 is aggressive, 2 is retreat
    private float turnThrottle = 50f;
    private float forwardThrottle = 70f;
    private ShipController aggressiveStatusTarget;
    private Vector3 retreatPoint;

    private float deathTimer = 0f;

    protected int maxNumberFighters = 4;
    public int currentNumberFighters = 0;
    public GameObject fighterPrefab;
    protected Vector3 fighterSpawnLocation;
    protected Quaternion fighterSpawnOrientation;
    private float fighterSpawnTimer = 0;
    private float fighterSpawnTimerInterval = 30;

    // Start is called before the first frame update
    protected void CapShipControllerStart()
    {
        if (friendly)
        {
            CrossSceneValues.friendlyCapitalShipTargetList.Add(this);
            CrossSceneValues.friendlyCapitalShipTargetList_enemy.Add(this);
        }
        else
        {
            CrossSceneValues.enemyCapitalShipTargetList.Add(this);
            CrossSceneValues.enemyCapitalShipTargetList_enemy.Add(this);
        }
        setAIStatus(1, transform.position);
        shipRigidBody = GetComponent<Rigidbody>();

        targetMarker = transform.Find("Canvas");
        targetMarker.gameObject.SetActive(false);
        offOpposingList = false;

        shipFires1.SetActive(false);
        shipFires2.SetActive(false);
        shipFires3.SetActive(false);
    }
    
    protected ShipController randomCycleTargets()
    {
        float rand = Random.Range(0f, 1f);
        int targetOption = 2;
        if (rand < fighterProbCutOff)
        {
            targetOption = 0;
        }
        else if (rand < capShipProbCutOff)
        {
            targetOption = 1;
        }

        if (targetOption == 0)
        {
            if (friendly)
            {
                if (CrossSceneValues.enemyFighterList.Count <= 0)
                {
                    targetOption = 1;
                }
                else
                {
                    targetSubListIndex = Random.Range(0, CrossSceneValues.enemyFighterList.Count);
                    return CrossSceneValues.enemyFighterList[targetSubListIndex];
                }
                
            }
            else
            {
                if (CrossSceneValues.friendlyFighterList_enemy.Count <= 0)
                {
                    targetOption = 1;
                }
                else
                {
                    targetSubListIndex = Random.Range(0, CrossSceneValues.friendlyFighterList_enemy.Count);
                    return CrossSceneValues.friendlyFighterList_enemy[targetSubListIndex];
                }
            }
        }
        if (targetOption == 1)
        {
            if (friendly)
            {
                if (CrossSceneValues.enemyCapitalShipTargetList.Count <= 0)
                {
                    targetOption = 2;
                }
                else
                {
                    targetSubListIndex = Random.Range(0, CrossSceneValues.enemyCapitalShipTargetList.Count);
                    return CrossSceneValues.enemyCapitalShipTargetList[targetSubListIndex];
                }

            }
            else
            {
                if (CrossSceneValues.friendlyCapitalShipTargetList_enemy.Count <= 0)
                {
                    targetOption = 2;
                }
                else
                {
                    targetSubListIndex = Random.Range(0, CrossSceneValues.friendlyCapitalShipTargetList_enemy.Count);
                    return CrossSceneValues.friendlyCapitalShipTargetList_enemy[targetSubListIndex];
                }
            }
        }
        if (targetOption == 2)
        {
            return null;
            /*if (friendly)
            {
                if (CrossSceneValues.objectiveList.Count <= 0)
                {
                    return null;
                }
                else
                {
                    targetSubListIndex = Random.Range(0, CrossSceneValues.objectiveList.Count);
                    return CrossSceneValues.objectiveList[targetSubListIndex];
                }

            }
            else
            {
                if (CrossSceneValues.objectiveList_enemy.Count <= 0)
                {
                    return null;
                }
                else
                {
                    targetSubListIndex = Random.Range(0, CrossSceneValues.objectiveList_enemy.Count);
                    return CrossSceneValues.objectiveList_enemy[targetSubListIndex];
                }
            }*/
        }
        return null;
    }

    protected void StaticControllerUpdate()
    {
        Transform aiCanvas = transform.Find("Canvas");
        aiCanvas.rotation = Camera.main.transform.rotation;

        float cameraDist = Vector3.Magnitude((aiCanvas.position - Camera.main.transform.position));
        aiCanvas.Find("TargetFrame").localScale = new Vector3(cameraDist, cameraDist, cameraDist) / 300;

        fighterSpawnTimer += Time.deltaTime;
        if (fighterSpawnTimer >= fighterSpawnTimerInterval)
        {
            fighterSpawnTimer = 0;
            if (currentNumberFighters < maxNumberFighters)
            {
                AIController newFighter = GameObject.Instantiate(fighterPrefab, transform.position + fighterSpawnLocation, fighterSpawnOrientation).GetComponent<AIController>();
                newFighter.friendly = this.friendly;
                newFighter.parentCapShip = this;
            }
        }
    }

    public override void toggleTargetMarker(bool setActive)
    {
        targetMarker.gameObject.SetActive(setActive);
    }

    public override float getHealthPercentage()
    {
        return (float)Mathf.Round((float)shipHealth / (float)totalShipHealth * 100f);
    }

    public override string getShipName()
    {
        return shipName;
    }
    public override Vector3 getVelocity()
    {
        return totalVelocityVector;
    }
    public override void destroyShipController() // makes entire gameobject disappear, don't use when cap ship is actually destroyed in-game, only for despawning
    {
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
        Destroy(gameObject);
    }
    protected void OnThrottle(float value)
    {
        if (ionStatus)
        {
            return;
        }
        throttleStatus = value;
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

    protected virtual float calcMFactor(float tLevel)
    {
        float throttleFactor = 1f + 12f / (10f * Mathf.Sqrt(2f * Mathf.PI)) * Mathf.Exp(-Mathf.Pow(tLevel - 50, 2) / 200f);

        return throttleFactor;
    }

    protected void handleMovement()
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

        float turningSpeedDrop = 1f - (mouseVector.magnitude * 0.3f);
        realMaxSpeed = maxSpeed * turningSpeedDrop;
        if (throttleLevel < 100)
        {
            realSpeedGoal = Mathf.Lerp(0.0f, 0.9f * realMaxSpeed, throttleLevel / 100.0f);

        }
        else
        {
            realSpeedGoal = 1.1f * realMaxSpeed;
        }

        float maneuverabilityFactor = calcMFactor(throttleLevel);

        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, realSpeedGoal, forwardAcceleration * Time.fixedDeltaTime);
        Vector3 forwardSpeedVector = -transform.up * activeForwardSpeed;

        totalVelocityVector = forwardSpeedVector;

        activeSpeed = totalVelocityVector.magnitude * 3.6f; //total speed converted to kph

        shipRigidBody.MovePosition(shipRigidBody.position + Time.fixedDeltaTime * totalVelocityVector);


        activeRoll = Mathf.Lerp(activeRoll, rollValue * rollStatus, rollAcceleration * Time.fixedDeltaTime);


        float goalPitch = -mouseVector.x * maneuverabilityFactor * lookRateSpeedPitch;
        float goalYaw = mouseVector.y * maneuverabilityFactor * lookRateSpeedYaw;


        activePitchSpeed = Mathf.Lerp(activePitchSpeed, goalPitch, maneuverabilityFactor * pitchAcceleration * Time.fixedDeltaTime);
        activeYawSpeed = Mathf.Lerp(activeYawSpeed, goalYaw, maneuverabilityFactor * yawAcceleration * Time.fixedDeltaTime);


        shipRigidBody.MoveRotation(shipRigidBody.rotation * Quaternion.Euler(activePitchSpeed * Time.fixedDeltaTime, activeRoll * Time.fixedDeltaTime, activeYawSpeed * Time.fixedDeltaTime));
        
    }

    protected virtual void takeDamage(float damage, Vector3 hitPoint, string thisCollider)
    {
    }

    protected virtual void takeIonDamage(float damage, Vector3 hitPoint, string thisCollider)
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
        Vector3 hitPoint = transform.InverseTransformPoint(contact.point);
        string thisColliderName = contact.thisCollider.ToString();
        // contact point with z > 170 is bridge deflector
        // contact y < -60 is front deflector
        // contact y > -60 and < 450 is mid deflector
        // contact y > 450 is rear deflector

        /*for (int i = 0; i < collision.contactCount; i++)
        {
            GameObject shieldFlare = GameObject.Instantiate(shieldFlarePrefab, collision.GetContact(i).point, Quaternion.FromToRotation(contact.normal, Vector3.up));
            shieldFlare.GetComponent<ParticleSystem>().Play();
        }*/
        if (collision.gameObject.ToString().Contains("shot_prefab"))
        {
            ShotBehavior sb = collision.gameObject.GetComponent<ShotBehavior>();
            if (sb.isIonDamage())
            {
                takeIonDamage(sb.getDamage(), hitPoint, thisColliderName);
            }
            else
            {
                takeDamage(sb.getDamage(), hitPoint, thisColliderName);
            }
            return;
        }

        // IF missile collision
        if ("concussion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(100, hitPoint, thisColliderName);
            return;
        }
        // IF proton torp collision
        if ("proton_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(500, hitPoint, thisColliderName);
            return;
        }
        // IF missile collision
        if ("ion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(500, hitPoint, thisColliderName);
            return;
        }
        // IF ion torp collision
        if ("ion_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(5000, hitPoint, thisColliderName);
            return;
        }
        // IF proton bomb collision
        if ("proton_bomb_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(1000, hitPoint, thisColliderName);
            return;
        }
        // IF ion bomb collision
        if ("ion_bomb_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(6000, hitPoint, thisColliderName);
            return;
        }
    }

    public void setAIStatus(int newStatus, Vector3 retreatGrid)
    {
        aiStatus = newStatus;

        // aggressive
        if (aiStatus == 1)
        {
            if (friendly)
            {
                if (CrossSceneValues.enemyCapitalShipTargetList.Count == 0)
                {
                    aiStatus = 0;
                    return;
                }
                aggressiveStatusTarget = CrossSceneValues.enemyCapitalShipTargetList[Random.Range(0, CrossSceneValues.enemyCapitalShipTargetList.Count)];
            }
            else
            {
                if (CrossSceneValues.friendlyCapitalShipTargetList_enemy.Count == 0)
                {
                    aiStatus = 0;
                    return;
                }
                aggressiveStatusTarget = CrossSceneValues.friendlyCapitalShipTargetList_enemy[Random.Range(0, CrossSceneValues.friendlyCapitalShipTargetList_enemy.Count)];
            }
        }
        else if (aiStatus == 2)
        {
            retreatPoint = retreatGrid;
        }
    }

    protected void handleAIMovement()
    {
        if (aiStatus == 0)
        {
            setThrottle(0f);
            OnYaw(0f);
        }
        else if (aiStatus == 1)
        {
            float distanceToTarget;
            turnTorwardTarget(aggressiveStatusTarget.getPosition(), out distanceToTarget);
            if (distanceToTarget < 5000f)
            {
                aiStatus = 0;
            }
        }
        else if (aiStatus == 2)
        {
            float distanceToRetreatPoint;
            turnTorwardTarget(retreatPoint, out distanceToRetreatPoint);
            if (distanceToRetreatPoint < 1000f)
            {
                aiStatus = 0;
            }
        }

        if (deathTimer < 20f && shipHealth <= 0f)
        {
            setThrottle(forwardThrottle);
            OnRoll(0.1f);
            OnPitch(-0.1f);
            deathTimer += Time.fixedDeltaTime;
        }
        else if (deathTimer >= 20f && deathTimer <= 120f)
        {
            setThrottle(forwardThrottle);
            OnRoll(0f);
            OnPitch(0f);
            deathTimer += Time.fixedDeltaTime;
        }
        else if (deathTimer > 120f)
        {
            setThrottle(0f);
        }
    }

    // regardless of aistatus, targeting priorities are always the same
    // If defensive, hold position, do nothing
    // if aggressive, turn towards randomized capital ship target (if none, default to defensive status). Move until within 5000 meters of target, then switch to defensive status
    // If retreat, turn torwards provided retreat Grid. Move until within 4000 meters of target, then switch to defensive status

    void setThrottle(float goalThrottle) // goalThrottle between 0 and 100
    {
        if (throttleLevel > goalThrottle)
        {
            OnThrottle(-1f);
        }
        else if (throttleLevel < goalThrottle)
        {
            OnThrottle(1f);
        }
        else
        {
            OnThrottle(0f);
        }
    }

    void turnTorwardTarget(Vector3 target, out float distanceToTarget)
    {

        Vector3 target_local = transform.InverseTransformVector(target - transform.position); // target coordinates in terms of local coordinates
        Vector3 target_local_xy = new Vector3(target_local.x, target_local.y);


        float targetAngle_x = Vector3.Angle(-Vector3.up, target_local_xy);
        bool turning = false;

        // Red axis (right) will determine yaw (x) - in reality it is right for the ship
        if (targetAngle_x > 7.0f)
        {
            if (target_local.x > 0)
            {
                OnYaw(1f);
                turning = true;
            }
            else
            {
                OnYaw(-1f);
                turning = true;
            }
        }
        else
        {
            OnYaw(0f);
        }

        if (turning)
        {
            setThrottle(turnThrottle);
        }
        else
        {
            setThrottle(forwardThrottle);
        }
        distanceToTarget = Vector3.Distance(target, transform.position);
    }
}
