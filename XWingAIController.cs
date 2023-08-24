using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class XWingAIController : ShieldedAIController
{
    // WEAPON VALUES CONFIGURABLE
    private float singleFireRate = 0.1f; // single weapons fire rate
    private float dualFireRate = 0.19f; // dual weapons fire rate
    private float quadFireRate = 0.36f; // quad fire rate
    public GameObject blasterBoltPrefab; // prefab used for blaster bolts
    public GameObject missilePrefab;
    public GameObject ionmissilePrefab;
    public GameObject iontorpPrefab;
    public GameObject protontorpPrefab;
    public int cMissileCount = 3;
    public int protonTorpCount = 3;
    public int ionMissileCount = 0;
    public int ionTorpCount = 0;

    // WEAPON VALUES VARIABLE
    private float fireStatus = 0f;
    private float fireRateTimestamp;
    private float fireRate;
    private int fireLinkStatus;
    private int fireLinkRotation;
    private int weaponSelection = 0; // 0 for lasers, 1 for missiles
    private int torpFireLinkStatus;

    // TARGETING VALUES CONFIGURABLE
    private float fullTimeTillLock = 7.0f;
    // TARGETING VALUES VARIABLE
    private bool targetLockStatus = false;
    private bool targetLockReset = true; // set to false when during lock. Set to true once lock broken.
    private float timeTillLock;
    private float targetingConvergence = 300f;
    private float missileConvergence = 1000f;
    private Vector3 targetLockGuide;

    // EFFECT VALUES CONFIGURABLE
    private Light engineLightRT;
    private Light engineLightRB;
    private Light engineLightLT;
    private Light engineLightLB;
    private ParticleSystem.MainModule particleRT;
    private ParticleSystem.MainModule particleRB;
    private ParticleSystem.MainModule particleLT;
    private ParticleSystem.MainModule particleLB;
    public AudioClip blasterClip; // audio clip used for blaster sound
    public AudioClip missileClip;
    public AudioClip protonTorpClip;
    public AudioClip ionmissileClip;
    public AudioClip iontorpClip;
    public AudioClip sFoilCloseClip;
    public AudioClip sFoilOpenClip;

    // S FOIL VALUES CONFIGURABLE
    private float sFoilAngle = 11f;
    private float sFoilTransitionTime = 2.5f; // Time for SFoils to transition
    // S FOIL VALUES VARIABLE
    private bool sFoilToggle; // true is open, false is closed
    private bool sFoilTransitionStatus; // true is in transition, false is not in transition
    private float sFoilActiveTime = 0f; // current time in transition

    // Start is called before the first frame update
    void Start()
    {
        ShipControllerStart();
        AIControllerStart();

        // Initialize Ship Specific Constants:
        rollValue = 75f * 2.0f; // determines speed of rolls, derived from DPF
        rollAcceleration = 3.7f; // acceleration of rolls, derived from G
        lookRateSpeedPitch = 75f * 1.25f; // determines the speed of pitching/yawing, derived from DPF
        lookRateSpeedYaw = 75f * 0.75f; // derived from DPF
        forwardAcceleration = 0.37f * 2.0f; // determines main engine accleration value, derived from G
        maxSpeed = 300f; // determines max forward speed, derived from kph
        repulsorAcceleration = 1.0f; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
        pitchAcceleration = .75f * 3.7f * 1.25f; // derived from DPF and G
        yawAcceleration = .75f * 3.7f * 0.75f; // derived from DPF and G
        totalShipHealth = 75f;
        maxRepulsorValue = 75.0f;
        totalForwardShieldHealth = 20;
        totalRearShieldHealth = 20;
        totalShieldLimit = 60;
        sensorRangeLimit = 5000f;
        sensorBurnLimit = 500f;

        // Initialize targeting values
        timeTillLock = fullTimeTillLock;

        // Initialize Health values
        shipHealth = 75f;
        shipIonHealth = totalShipHealth;
        forwardShieldHealth = 20f;
        rearShieldHealth = 20f;
        forwardOverShieldHealth = 10f;
        rearOverShieldHealth = 10f;

        // Initialize effects
        engineLightLB = transform.Find("LeftWingBottom").Find("EngineLightLB").gameObject.GetComponent<Light>();
        engineLightRB = transform.Find("RightWingBottom").Find("EngineLightRB").gameObject.GetComponent<Light>();
        engineLightLT = transform.Find("LeftWingTop").Find("EngineLightLT").gameObject.GetComponent<Light>();
        engineLightRT = transform.Find("RightWingTop").Find("EngineLightRT").gameObject.GetComponent<Light>();
        particleRT = transform.Find("RightWingTop").Find("EngineParticlesRT").gameObject.GetComponent<ParticleSystem>().main;
        particleLT = transform.Find("LeftWingTop").Find("EngineParticlesLT").gameObject.GetComponent<ParticleSystem>().main;
        particleRB = transform.Find("RightWingBottom").Find("EngineParticlesRB").gameObject.GetComponent<ParticleSystem>().main;
        particleLB = transform.Find("LeftWingBottom").Find("EngineParticlesLB").gameObject.GetComponent<ParticleSystem>().main;

        // Initialize movement values
        throttleLevel = 90;
        throttleStatus = 0;
        repulsorStatus = 0;

        // Initialize power values
        enginePower = 4;
        laserPower = 2;
        shieldPower = 2;
        overallPower = 8;

        // Initialize weapon values
        fireRate = singleFireRate;
        fireLinkStatus = 0; // 0 for single fire, 1 for dual fire, 2 for quad fire
        torpFireLinkStatus = 0; // 0 for single, 1 for dual

        // Initialize S Foil values
        sFoilToggle = true;
        sFoilTransitionStatus = false;

        //Debug.Log(GetComponent<Renderer>().bounds.size);
        transformStartPosition = transform.position;
        centerPosition = transform.position;
        defaultPatrolRoute = new Vector3[] { transform.position + new Vector3(1000, UnityEngine.Random.Range(100, 300), 1000), transform.position + new Vector3(-1000, UnityEngine.Random.Range(100, 300), 1000), transform.position + new Vector3(-1000, UnityEngine.Random.Range(100, 300), -1000), transform.position + new Vector3(1000, UnityEngine.Random.Range(100, 300), -1000) };

        if (!friendly)
        {
            if (CrossSceneValues.difficulty == 0)
            {
                skillLevel = 1;
                maxSpeed = 270f;
            }
            else if (CrossSceneValues.difficulty == 1)
            {
                skillLevel = 3;
                maxSpeed = 280;
            }
            else
            {
                skillLevel = 5;
            }
            // switch to targeting friendly fighters
            OnCycleTargetOptions();
            OnCycleTargetOptions();
        }
        randomCycleTargets();
    }

    // AI CONTROL VALUES VARIABLE
    private bool attackPlayerFirstCall = true;
    private float distanceToObj;
    private int resetStatus = 0; // 0 chase player, 1 too close to player, 2 reset to start position, 3 reset to greater height
    private Vector3 reset1GoalPosition;
    private float timeStampReset1;
    private Vector3 transformStartPosition;

    //************************************************AI CONTROL FUNCTIONS************************************************//
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

    void SlowCirclePatrol()
    {
        //OnThrottle(1f);
        //OnYaw(0.25f);
        OnFire(0f);


        if (Vector3.Magnitude(transform.position - transformStartPosition) > 500)
        {
            float distanceToTarget;
            bool isTurning;
            turnTorwardTarget(transformStartPosition, out distanceToTarget, out isTurning);
        }
        else
        {
            OnYaw(0.25f);

            setThrottle(90f);

            // If ship is not level with the ground, roll until it is
            if (transform.localRotation.eulerAngles.x - 270f < -30f)
            {
                OnRoll(1f);
            }
            else if (transform.localRotation.eulerAngles.x - 270f > 30f)
            {
                OnRoll(-1f);
            }
            else
            {
                OnRoll(0f);
            }
        }
    }

    void turnTorwardTarget(Vector3 target, out float distanceToTarget, out bool isTurning)
    {

        Vector3 target_local = transform.InverseTransformVector(target - transform.position); // target coordinates in terms of local coordinates
        Vector3 target_local_xy = new Vector3(target_local.x, target_local.y);
        Vector3 target_local_yz = new Vector3(0f, target_local.y, target_local.z);


        float targetAngle_x = Vector3.Angle(-Vector3.up, target_local_xy);
        float targetAngle_z = Vector3.Angle(-Vector3.up, target_local_yz);
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

        // Blue axis (forward) will determine pitch (z) - in reality it is up for the ship
        if (targetAngle_z > 7.0f)
        {
            if (target_local.z > 0)
            {
                OnPitch(1f);
                turning = true;
            }
            else
            {
                OnPitch(-1f);
                turning = true;
            }
        }
        else
        {
            OnPitch(0f);
        }
        // If ship is not level with the ground, roll until it is
        /*if (Math.Abs(transform.localRotation.eulerAngles.x - (270f)) > 30f)
        {
            OnRoll(1f);
        }
        else
        {
            OnRoll(0f);
        }*/
        float rollAngle = Vector3.SignedAngle(transform.forward, Vector3.Cross(transform.up, Vector3.Cross(Vector3.up, transform.up)), transform.up);
        if (rollAngle < -30)
        {
            OnRoll(-1f);
        }
        else if (rollAngle > 30)
        {
            OnRoll(1f);
        }
        else
        {
            OnRoll(0f);
        }

        if (turning)
        {
            setThrottle(turnThrottle);
        }
        else
        {
            setThrottle(forwardThrottle);
        }

        isTurning = turning;
        if (currentTarget == null)
        {
            distanceToTarget = 1000;
            return;
        }
        Vector3 shipToTarget = currentTarget.getJamPosition(friendly, out jammed) - transform.position;
        //Vector3 shipForwardVector = -transform.up;
        //float targetAngle = Vector3.Angle(shipForwardVector, shipToTarget);
        float targetDistance = Vector3.Magnitude(shipToTarget);

        isTurning = turning;
        distanceToTarget = targetDistance;
    }

    void attackPlayerStart()
    {
        // Adjust power to full power. 3 power pips to lasers, rest to engines
        OnOverallPower();
        OnOverallPower();

        attackPlayerFirstCall = false;
    }

    private Vector3 previousTargetVector;

    Vector3 centerPosition;
    private float fighterRange = 10000f;
    private float recoveryRange = 8000f;
    private int recoveryMode = 0; // 0 for normal, 1 for range, 2 for target
    private float turnThrottle = 50f;
    private float forwardThrottle = 70f;

    private float tooCloseToTarget = 150f;
    private float targetRecoveryRange = 500f;
    private Vector3 targetRecoveryPosition;
    public int skillLevel = 5;
    private bool patrolActive = false;
    private float skillCheckTimer = 5;
    private float fullSkilCheckTimer = 5f;
    private float skillCheckIterations = 0;
    private int patrolRouteIndex = 0;
    private Vector3[] checkpoints;
    private Vector3[] defaultPatrolRoute;

    Vector3 collisionWarningDetection(out bool forwardWarning)
    {
        Vector3 collisionAvoidanceVector = Vector3.zero;
        /*if (Vector3.Distance(transform.position, currentTarget.transform.position) < 50)
        {
            collisionAvoidanceVector += -(currentTarget.transform.position - transform.position).normalized;
        }*/
        forwardWarning = false;
        for (float phi = 0; phi <= 2 * Mathf.PI; phi += Mathf.PI / 24)
        {
            for (float theta = 0; theta <= Mathf.PI; theta += Mathf.PI / 24)
            {
                float x = Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = Mathf.Sin(theta) * Mathf.Sin(phi);
                float z = Mathf.Cos(theta);
                //Vector3 rayCastVector = transform.TransformVector(new Vector3(x, y, z));
                Vector3 rayCastVector = (transform.TransformPoint(new Vector3(x, y, z)) - transform.position).normalized;
                RaycastHit hit;
                float rayDistance = 50f;
                if (y < -0.4)
                {
                    //Debug.Log(-transform.up);
                    //Debug.Log((transform.TransformPoint(new Vector3(x, y, z)) - transform.position).normalized);
                    rayDistance = 150f;
                }
                bool hitBool = Physics.Raycast(transform.position + rayCastVector * 15, rayCastVector, out hit, rayDistance);

                if (hitBool)
                {
                    //Debug.Log(hit.collider.attachedRigidbody);
                    //Debug.Log(hit.collider.name);
                    //collisionAvoidanceVector += -rayCastVector.normalized * 100f / hit.distance;
                    collisionAvoidanceVector += -rayCastVector.normalized;

                    if (y < -0.4)
                    {
                        forwardWarning = true;
                    }
                }


            }
        }
        return collisionAvoidanceVector.normalized;
    }

    private void patrolRoute(bool switchToAttack)
    {
        aiPowerManagement();
        bool forwardCollisionWarning;
        Vector3 collisionRepulseVector = collisionWarningDetection(out forwardCollisionWarning);

        float distanceToTarget;
        bool isTurning;



        if (collisionRepulseVector == Vector3.zero)
        {
            if (forwardCollisionWarning)
            {
                turnThrottle = 15;
                forwardThrottle = 0;
            }
            else
            {
                turnThrottle = 70f;
                forwardThrottle = 100f;
            }

            //turnTorwardTarget(transform.position + ((previousTargetVector - transform.position).normalized + (currentTarget.transform.position - transform.position).normalized).normalized * 100f, out distanceToTarget, out isTurning);
            //previousTargetVector = transform.position + ((previousTargetVector - transform.position).normalized + (currentTarget.transform.position - transform.position).normalized).normalized * 100f;
            if (switchToAttack)
            {
                turnTorwardTarget(checkpoints[patrolRouteIndex], out distanceToTarget, out isTurning);
                previousTargetVector = checkpoints[patrolRouteIndex];
                if (Vector3.Distance(checkpoints[patrolRouteIndex], transform.position) < 150)
                {
                    patrolRouteIndex++;
                    patrolRouteIndex = patrolRouteIndex % checkpoints.Length;
                }
            }
            else
            {
                turnTorwardTarget(defaultPatrolRoute[patrolRouteIndex], out distanceToTarget, out isTurning);
                previousTargetVector = defaultPatrolRoute[patrolRouteIndex];
                if (Vector3.Distance(defaultPatrolRoute[patrolRouteIndex], transform.position) < 150)
                {
                    patrolRouteIndex++;
                    patrolRouteIndex = patrolRouteIndex % defaultPatrolRoute.Length;
                }
            }


        }
        else
        {
            if (forwardCollisionWarning)
            {
                turnThrottle = 20f;
                forwardThrottle = 10f;
            }
            else
            {
                turnThrottle = 50f;
                forwardThrottle = 70f;
            }

            //Debug.Log(currentTarget.transform.position - transform.position);
            //Debug.Log(collisionRepulseVector);
            //turnTorwardTarget(transform.position + ((previousTargetVector - transform.position).normalized + collisionRepulseVector).normalized * 100f, out distanceToTarget, out isTurning);
            //previousTargetVector = transform.position + ((previousTargetVector - transform.position).normalized + collisionRepulseVector).normalized * 100f;
            turnTorwardTarget(transform.position + collisionRepulseVector.normalized * 1000f, out distanceToTarget, out isTurning);
            previousTargetVector = transform.position + collisionRepulseVector.normalized * 1000f;
        }


        distanceToObj = distanceToTarget;
        switchWeaponLogic(distanceToTarget);
        if (weaponSelection == 0)
        {
            if (targetingConvergence - distanceToTarget > 51)
            {
                OnAdjustTargeting(-1f);
            }
            else if (distanceToTarget - targetingConvergence > 51)
            {
                OnAdjustTargeting(1f);
            }
        }
        else
        {
            if (missileConvergence - distanceToTarget > 101)
            {
                OnAdjustTargeting(-1f);
            }
            else if (distanceToTarget - missileConvergence > 101)
            {
                OnAdjustTargeting(1f);
            }
        }
        

        if (currentTarget != null)
        {
            if (targetLockStatus || (Vector3.Angle(-transform.up, currentTarget.transform.position - transform.position) < 10 && distanceToTarget <= 500 && weaponSelection == 0))
            {
                OnFire(1f);
                if (weaponSelection != 0)
                {
                    timeSinceLastMissile = 0;
                }
            }
            else
            {
                OnFire(0f);
            }
        }
        else
        {
            OnFire(0f);
        }

        if (switchToAttack)
        {
            skillCheckTimer -= Time.fixedDeltaTime;
            if (skillCheckTimer < 0)
            {
                skillCheckTimer = fullSkilCheckTimer;
                skillCheckIterations++;
                float chanceToSwitch = Mathf.Log10(skillCheckIterations + 1f) / (1f + (10f - skillLevel) / 10f);
                if (UnityEngine.Random.Range(0f, 1f) < chanceToSwitch)
                {
                    patrolActive = false;
                    skillCheckIterations = 0;
                    patrolRouteIndex = 0;

                }

            }
        }
    }

    private void aiPowerManagement()
    {
        if (attackPlayerFirstCall)
        {
            attackPlayerStart();
        }
        if (laserCharge / laserChargeLimit < 0.5 && laserPower == 2) // increase lasers to 3 pips
        {
            OnLaserPower(1f);
            OnLaserPower(0f);
        }
        if (laserCharge / laserChargeLimit < 0.1 && laserPower == 3) // increase lasers to 4 pips
        {
            OnLaserPower(1f);
            OnLaserPower(0f);
        }
        if (laserCharge / laserChargeLimit > 0.1 && laserPower == 4) // decrease lasers to 3 pips
        {
            OnLaserPower(1f);
            OnEnginePower(1f);
            OnEnginePower(0f);
            OnLaserPower(0f);
        }
        if (laserCharge / laserChargeLimit > 0.5 && laserPower == 3) // decrease lasers to 2 pips
        {
            OnLaserPower(1f);
            OnEnginePower(1f);
            OnEnginePower(0f);
            OnLaserPower(0f);
        }
    }

    private float timeSinceLastMissile = 10;
    private void switchWeaponLogic(float distanceToTarget)
    {
        if (weaponSelection == 0 && distanceToTarget > 500 && timeSinceLastMissile > 10 && cMissileCount > 0) // switch to missiles
        {
            OnSwitchWeapon();
        }
        else if (weaponSelection == 1)
        {
            if (distanceToTarget < 500 || timeSinceLastMissile < 10 || cMissileCount <= 0) // switch back to lasers
            {
                OnSwitchWeapon();
                OnSwitchWeapon();
            }
        }
        timeSinceLastMissile += Time.fixedDeltaTime;
    }

    protected void attackPlayer()
    {
        aiPowerManagement();

        bool forwardCollisionWarning;
        Vector3 collisionRepulseVector = collisionWarningDetection(out forwardCollisionWarning);

        float distanceToTarget;
        bool isTurning;


        if (collisionRepulseVector == Vector3.zero)
        {
            if (forwardCollisionWarning)
            {
                turnThrottle = 15;
                forwardThrottle = 0;
            }
            else
            {
                turnThrottle = 70f;
                forwardThrottle = 100f;
            }

            if (Vector3.Distance(transform.position, currentTarget.transform.position) < tooCloseToTarget && recoveryMode != 2)
            {
                recoveryMode = 2;
                targetRecoveryPosition = -(currentTarget.transform.position - transform.position).normalized;
            }
            else if (Vector3.Distance(transform.position, centerPosition) > fighterRange && recoveryMode != 1)
            {
                recoveryMode = 1;
            }
            else if (Vector3.Distance(transform.position, centerPosition) < recoveryRange && Vector3.Distance(transform.position, currentTarget.transform.position) > targetRecoveryRange && recoveryMode != 0)
            {
                recoveryMode = 0;
            }


            if (recoveryMode == 1)
            {
                turnTorwardTarget(transform.position + ((previousTargetVector - transform.position).normalized + (centerPosition - transform.position).normalized).normalized * 100f, out distanceToTarget, out isTurning);
                previousTargetVector = transform.position + ((previousTargetVector - transform.position).normalized + (centerPosition - transform.position).normalized).normalized * 100f;
                //turnTorwardTarget(centerPosition, out distanceToTarget, out isTurning);
                //previousTargetVector = centerPosition;
            }
            else if (recoveryMode == 2)
            {
                turnTorwardTarget(transform.position + targetRecoveryPosition * 1000f, out distanceToTarget, out isTurning);
                previousTargetVector = transform.position + targetRecoveryPosition * 1000f;
            }
            else
            {
                //turnTorwardTarget(transform.position + ((previousTargetVector - transform.position).normalized + (currentTarget.transform.position - transform.position).normalized).normalized * 100f, out distanceToTarget, out isTurning);
                //previousTargetVector = transform.position + ((previousTargetVector - transform.position).normalized + (currentTarget.transform.position - transform.position).normalized).normalized * 100f;
                turnTorwardTarget(currentTarget.getJamPosition(friendly, out jammed), out distanceToTarget, out isTurning);
                previousTargetVector = currentTarget.getJamPosition(friendly, out jammed);
            }

        }
        else
        {
            if (forwardCollisionWarning)
            {
                turnThrottle = 20f;
                forwardThrottle = 10f;
            }
            else
            {
                turnThrottle = 50f;
                forwardThrottle = 70f;
            }

            //Debug.Log(currentTarget.transform.position - transform.position);
            //Debug.Log(collisionRepulseVector);
            //turnTorwardTarget(transform.position + ((previousTargetVector - transform.position).normalized + collisionRepulseVector).normalized * 100f, out distanceToTarget, out isTurning);
            //previousTargetVector = transform.position + ((previousTargetVector - transform.position).normalized + collisionRepulseVector).normalized * 100f;
            turnTorwardTarget(transform.position + collisionRepulseVector.normalized * 1000f, out distanceToTarget, out isTurning);
            previousTargetVector = transform.position + collisionRepulseVector.normalized * 1000f;
        }


        distanceToObj = distanceToTarget;

        switchWeaponLogic(distanceToTarget);
        if (weaponSelection == 0)
        {
            if (targetingConvergence - distanceToTarget > 51)
            {
                OnAdjustTargeting(-1f);
            }
            else if (distanceToTarget - targetingConvergence > 51)
            {
                OnAdjustTargeting(1f);
            }
        }
        else
        {
            if (missileConvergence - distanceToTarget > 101)
            {
                OnAdjustTargeting(-1f);
            }
            else if (distanceToTarget - missileConvergence > 101)
            {
                OnAdjustTargeting(1f);
            }
        }
        

        
        if (targetLockStatus)
        {
            OnFire(1f);
            if (weaponSelection != 0)
            {
                timeSinceLastMissile = 0;
            }
        }
        else if (weaponSelection == 0 && (recoveryMode == 0 && !isTurning && distanceToTarget <= 500) || (Vector3.Angle(-transform.up, currentTarget.transform.position - transform.position) < 10 && distanceToTarget <= 500))
        {
            OnFire(1f);
        }
        else
        {
            OnFire(0f);
        }

        skillCheckTimer -= Time.fixedDeltaTime;
        if (skillCheckTimer < 0)
        {
            skillCheckTimer = fullSkilCheckTimer;
            skillCheckIterations++;
            float chanceToSwitch = Mathf.Log10(skillCheckIterations + 1f) / (1f + skillLevel / 10f);
            if (UnityEngine.Random.Range(0f, 1f) < chanceToSwitch)
            {
                patrolActive = true;
                skillCheckIterations = 0;
                patrolRouteIndex = 0;
                checkpoints = new Vector3[] { transform.position + new Vector3(500, UnityEngine.Random.Range(100, 300), 500), transform.position + new Vector3(-500, UnityEngine.Random.Range(100, 300), 500), transform.position + new Vector3(-500, UnityEngine.Random.Range(100, 300), -500), transform.position + new Vector3(500, UnityEngine.Random.Range(100, 300), -500) };
            }

        }
    }

    //************************************************INPUT FUNCTIONS************************************************//
    // S Foil input function
    void OnSFoils()
    {
        if (!sFoilTransitionStatus)
        {
            sFoilTransitionStatus = true;
            sFoilToggle = !sFoilToggle;

            if (sFoilToggle)
            {
                shipSounds.PlayOneShot(sFoilOpenClip);
            }
            else
            {
                shipSounds.PlayOneShot(sFoilCloseClip);
            }
        }
    }

    // Input function to adjust targeting convergence
    void OnAdjustTargeting(float value)
    {
        if (value < 0)
        {
            changeTargetingConvergence(-1f);
        }
        else if (value > 0)
        {
            changeTargetingConvergence(1f);
        }
    }

    void OnResetTargeting()
    {
        if (weaponSelection == 0) // lasers
        {
            transform.Find("TargetingConvergence").position = transform.position + -300f * transform.up;
            targetingConvergence = 300f;
        }
        else // missiles
        {
            transform.Find("TargetingConvergence").position = transform.position + -1000f * transform.up;
            missileConvergence = 1000f;
        }
    }
    void targetLockCancel()
    {
        // target lock reset
        targetLockStatus = false;
        timeTillLock = fullTimeTillLock;
        targetLockReset = true;
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
    }

    // Weapon selection function
    void OnSwitchWeapon()
    {
        int oldWeaponSelection = weaponSelection;

        weaponSelection = (weaponSelection + 1) % 5;


        if (weaponSelection == 1 && cMissileCount == 0) // switched to concussion missiles
        {
            weaponSelection = 2;
        }
        if (weaponSelection == 2 && protonTorpCount == 0) // switched to proton torpedoes
        {
            weaponSelection = 3;
        }
        if (weaponSelection == 3 && ionMissileCount == 0) // switched to ion missile
        {
            weaponSelection = 4;
        }
        if (weaponSelection == 4 && ionTorpCount == 0) // switched to ion torp
        {
            weaponSelection = 0;
        }

        // If switched from lasers to munitions or vice versa cancel target lock
        if ((oldWeaponSelection == 0 && weaponSelection != 0) || (oldWeaponSelection != 0 && weaponSelection == 0))
        {
            targetLockCancel();
        }

        if (weaponSelection == 0) // lasers
        {
            transform.Find("TargetingConvergence").position = transform.position + -targetingConvergence * transform.up;
        }
        else // missiles
        {
            transform.Find("TargetingConvergence").position = transform.position + -missileConvergence * transform.up;
        }
    }

    // Input function for firing weapon system
    void OnFire(float value)
    {
        fireStatus = value;
    }

    // Cannon link function
    void OnLinkCannons()
    {
        if (weaponSelection == 0)
        {
            fireLinkRotation = 0;
            if (fireLinkStatus == 0)
            {
                fireLinkStatus = 1;
                fireRate = dualFireRate;
            }
            else if (fireLinkStatus == 1)
            {
                fireLinkStatus = 2;
                fireRate = quadFireRate;
            }
            else
            {
                fireLinkStatus = 0;
                fireRate = singleFireRate;
            }
        }
        else
        {
            if (torpFireLinkStatus == 0)
            {
                torpFireLinkStatus = 1;
            }
            else
            {
                torpFireLinkStatus = 0;
            }
        }

    }

    //************************************************WEAPON SYSTEM FUNCTIONS************************************************//
    void shootRayLB()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleLB = transform.Find("LeftWingBottom").Find("MuzzleLeftBottom");

        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleLB.position, muzzleLB.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLB.position).normalized, 2000, 15, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLB.position).normalized, 2000, 15, false);
        }
    }

    void shootRayRB()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleRB = transform.Find("RightWingBottom").Find("MuzzleRightBottom");

        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleRB.position, muzzleRB.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRB.position).normalized, 2000, 15, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRB.position).normalized, 2000, 15, false);
        }
    }

    void shootRayLT()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleLT = transform.Find("LeftWingTop").Find("MuzzleLeftTop");

        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleLT.position, muzzleLT.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLT.position).normalized, 2000, 15, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLT.position).normalized, 2000, 15, false);
        }
    }

    void shootRayRT()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleRT = transform.Find("RightWingTop").Find("MuzzleRightTop");

        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleRT.position, muzzleRT.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRT.position).normalized, 2000, 15, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRT.position).normalized, 2000, 15, false);
        }
    }

    void shootTorpL()
    {
        Transform torpMuzzleL = transform.Find("TorpMuzzleL");
        GameObject currentPrefab = missilePrefab;
        if (weaponSelection == 2)
        {
            currentPrefab = protontorpPrefab;
        }
        else if (weaponSelection == 3)
        {
            currentPrefab = ionmissilePrefab;
        }
        else if (weaponSelection == 4)
        {
            currentPrefab = iontorpPrefab;
        }
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzleL.position, torpMuzzleL.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleL.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleL.position).normalized, currentTarget, weaponSelection, friendly);
        }

    }

    void shootTorpR()
    {
        Transform torpMuzzleR = transform.Find("TorpMuzzleR");
        GameObject currentPrefab = missilePrefab;
        if (weaponSelection == 2)
        {
            currentPrefab = protontorpPrefab;
        }
        else if (weaponSelection == 3)
        {
            currentPrefab = ionmissilePrefab;
        }
        else if (weaponSelection == 4)
        {
            currentPrefab = iontorpPrefab;
        }
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzleR.position, torpMuzzleR.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleR.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleR.position).normalized, currentTarget, weaponSelection, friendly);
        }
    }

    // Function called in Update() to fire cannons
    void fireCannonsUpdate()
    {
        if (fireStatus == 1f && Time.time > fireRateTimestamp && weaponSelection == 0)
        {
            if (laserCharge > 0)
            {
                shipSounds.PlayOneShot(blasterClip);
                if (fireLinkStatus == 2)
                {
                    shootRayLB();
                    shootRayRB();
                    shootRayLT();
                    shootRayRT();
                }
                else if (fireLinkStatus == 1)
                {
                    if (fireLinkRotation == 0)
                    {
                        shootRayRT();
                        shootRayLB();
                        fireLinkRotation++;
                    }
                    else
                    {
                        shootRayRB();
                        shootRayLT();
                        fireLinkRotation--;
                    }
                }
                else
                {
                    if (fireLinkRotation == 0)
                    {
                        shootRayRT();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 1)
                    {
                        shootRayLB();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 2)
                    {
                        shootRayLT();
                        fireLinkRotation++;
                    }
                    else
                    {
                        shootRayRB();
                        fireLinkRotation = 0;
                    }
                }

                fireRateTimestamp = Time.time + fireRate;
            }
        }
        else if (fireStatus == 1f && weaponSelection != 0)
        {
            if ((getCurrentWeaponAmmoCount() > 0 && torpFireLinkStatus == 0) || getCurrentWeaponAmmoCount() == 1)
            {
                if (weaponSelection == 1)
                {
                    shipSounds.PlayOneShot(missileClip);
                    cMissileCount--;
                }
                else if (weaponSelection == 2)
                {
                    shipSounds.PlayOneShot(protonTorpClip);
                    protonTorpCount--;
                }
                else if (weaponSelection == 3)
                {
                    shipSounds.PlayOneShot(ionmissileClip);
                    ionMissileCount--;
                }
                else if (weaponSelection == 4)
                {
                    shipSounds.PlayOneShot(iontorpClip);
                    ionTorpCount--;
                }
                shootTorpL();
            }
            else if (getCurrentWeaponAmmoCount() > 1 && torpFireLinkStatus == 1)
            {
                if (weaponSelection == 1)
                {
                    shipSounds.PlayOneShot(missileClip);
                    cMissileCount -= 2;
                }
                else if (weaponSelection == 2)
                {
                    shipSounds.PlayOneShot(protonTorpClip);
                    protonTorpCount -= 2;
                }
                else if (weaponSelection == 3)
                {
                    shipSounds.PlayOneShot(ionmissileClip);
                    ionMissileCount -= 2;
                }
                else if (weaponSelection == 4)
                {
                    shipSounds.PlayOneShot(iontorpClip);
                    ionTorpCount -= 2;
                }
                shootTorpL();
                shootTorpR();
            }
            fireStatus = 0f;
        }
    }

    //************************************************TARGETING SYSTEM FUNCTIONS************************************************//
    // Change targeting convergence distance
    void changeTargetingConvergence(float changeValue)
    {

        if (weaponSelection == 0) // lasers
        {
            if (targetingConvergence <= 100f && changeValue < 0f)
            {
                targetingConvergence = 100f;
                return;
            }
            else if (targetingConvergence >= 1000f && changeValue > 0f)
            {
                targetingConvergence = 1000f;
                return;
            }
            transform.Find("TargetingConvergence").position = transform.Find("TargetingConvergence").position - 50f * changeValue * transform.up;
            targetingConvergence = targetingConvergence + 50f * changeValue;
        }
        else // missiles
        {
            if (missileConvergence <= 500f && changeValue < 0f)
            {
                missileConvergence = 500f;
                return;
            }
            else if (missileConvergence >= 5000f && changeValue > 0f)
            {
                missileConvergence = 5000f;
                return;
            }
            transform.Find("TargetingConvergence").position = transform.Find("TargetingConvergence").position - 100f * changeValue * transform.up;
            missileConvergence = missileConvergence + 100f * changeValue;
        }
    }
    bool targetLockBreak(float targetAngle, float targetDistance)
    {
        if (targetAngle > 15f || !targetingComputerToggle)
        {
            return true;
        }

        if (weaponSelection <= 0)
        {
            if (targetDistance > 1000f || Mathf.Abs(targetingConvergence - targetDistance) > 300f)
            {
                return true;
            }
        }
        else
        {
            if (targetDistance > 5000f || targetDistance < 300f || Mathf.Abs(missileConvergence - targetDistance) > 500f)
            {
                return true;
            }
        }
        return false;
    }

    // Target Lock function
    void targetLock(bool onTarget)
    {
        if (!onTarget || jammed)
        {
            return;
        }
        Vector3 shipToTarget = currentTarget.getJamPosition(friendly, out jammed) - transform.position;
        Vector3 shipForwardVector = -transform.up;
        float targetAngle = Vector3.Angle(shipForwardVector, shipToTarget);
        float targetDistance = Vector3.Magnitude(shipToTarget);

        if (targetLockBreak(targetAngle, targetDistance) || jammed)
        {
            targetLockCancel();
        }
        else
        {
            if (!targetLockStatus)
            {
                if (targetLockReset)
                {
                    targetLockReset = false;
                }
                if (!lockingOn)
                {
                    lockingOn = true;
                    currentTarget.lockingOnWarning++;
                }

                float targetingConvergenceDifference = 10f;
                float targetingRateFactor = 1f;
                if (weaponSelection == 0) // lasers
                {
                    targetingConvergenceDifference += Mathf.Abs(targetDistance - targetingConvergence);
                    // The closer the targeting convergence to actual distance and the closer to the center of sight, the faster the lock
                    targetingRateFactor = targetingConvergenceDifference * targetAngle / 300f;
                }
                else // missiles
                {
                    targetingConvergenceDifference += Mathf.Abs(targetDistance - missileConvergence);
                    // The closer the targeting convergence to actual distance and the closer to the center of sight, the faster the lock
                    targetingRateFactor = targetingConvergenceDifference * targetAngle / 200f;
                }

                timeTillLock = timeTillLock - Time.fixedDeltaTime / targetingRateFactor;
                if (timeTillLock <= 0f)
                {
                    targetLockStatus = true;
                    if (!lockedOn)
                    {
                        lockedOn = true;
                        currentTarget.lockedOnWarning++;
                    }
                    //targetGuide.gameObject.SetActive(true);

                    return;
                }

            }
        }
    }

    // Calculate target guide position
    void calcTargetGuide(bool onTarget)
    {
        /*Vector3 cameraPosition = GameObject.Find("Main Camera").transform.position;

        if (!onTarget || !targetLockStatus)
        {
            targetGuide.position = cameraPosition;
            targetGuide.gameObject.SetActive(false);
            crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
            return;
        }
        Vector3 targetLocation = currentTarget.getPosition();
        Vector3 targetVelocity = currentTarget.getVelocity();
        Vector3 laserVelocity = totalVelocityVector - 750f * transform.up;
        Vector3 targetPlaneVelocity = Vector3.ProjectOnPlane(targetVelocity, transform.up);

        Vector3 shipLocation = transform.position;
        Vector3 shipToTargetLocation = targetLocation - shipLocation; 
        float k = shipToTargetLocation.magnitude;

        float d_t = 1.1f*Mathf.Sqrt(k * k / (Mathf.Pow(laserVelocity.magnitude, 2f) / Mathf.Pow(targetPlaneVelocity.magnitude, 2f) - 1));
        Vector3 targetGuideRealPosition = targetLocation + targetPlaneVelocity.normalized * d_t;
        //targetGuide.position = targetGuideRealPosition;
        targetGuide.position = (targetGuideRealPosition - cameraPosition).normalized * 300f + cameraPosition;*/

        /*float shipToTargetVelocityAngle = Vector3.Angle(shipToTargetLocation, shipVelocity);
        Vector3.ProjectOnPlane(targetVelocity, transform.up);
        float v_t = targetVelocity.magnitude;
        float v_s = shipVelocity.magnitude;
        float a = 1 + Mathf.Pow(v_t, 2f) / Mathf.Pow(v_s, 2f);
        float b = 2 * k * v_t * Mathf.Cos(shipToTargetVelocityAngle) / v_s;
        float c = -Mathf.Pow(k, 2);
        float x_s = (-b + Mathf.Sqrt(Mathf.Pow(b, 2f) - 4 * a * c)) / (2 * a);
        float x_t = x_s * v_t / v_s;
        targetGuide.position = targetLocation + targetVelocity.normalized * x_t;
        //targetGuide.position = targetLocation;*/
        if (!onTarget || !targetLockStatus)
        {
            targetLockStatus = false;
            return;
        }
        Vector3 targetLocation = currentTarget.getPosition();
        Vector3 targetVelocity = currentTarget.getVelocity();
        Vector3 laserVelocity = totalVelocityVector - 750f * transform.up;

        float time_laser_travel = Vector3.Distance(targetLocation, transform.position) / laserVelocity.magnitude;
        Vector3 targetFutureLocation = targetLocation + targetVelocity * time_laser_travel;
        //Vector3 targetFutureLocation = targetLocation;
        //targetLockGuide = transform.TransformPoint(transform.InverseTransformPoint(targetFutureLocation).normalized * targetingConvergence);
        targetLockGuide = targetFutureLocation;
    }

    //************************************************MISC FUNCTIONS************************************************//
    // S Foil Transition function
    void sFoilTransition()
    {
        if (sFoilTransitionStatus)
        {
            sFoilActiveTime += Time.fixedDeltaTime;

            float sFoilAngleSet;
            if (sFoilActiveTime >= sFoilTransitionTime)
            {
                if (sFoilToggle)
                {
                    sFoilAngleSet = sFoilAngle;
                }
                else
                {
                    sFoilAngleSet = 0;
                }
                sFoilActiveTime = 0f;
                sFoilTransitionStatus = false;

            }
            else
            {
                if (sFoilToggle)
                {
                    sFoilAngleSet = Mathf.Lerp(0, sFoilAngle, sFoilActiveTime / sFoilTransitionTime);
                }
                else
                {
                    sFoilAngleSet = Mathf.Lerp(sFoilAngle, 0, sFoilActiveTime / sFoilTransitionTime);
                }

            }
            transform.Find("LeftWingTop").localRotation = Quaternion.Euler(0, sFoilAngleSet, 0);
            transform.Find("LeftWingBottom").localRotation = Quaternion.Euler(0, -sFoilAngleSet, 0);
            transform.Find("RightWingTop").localRotation = Quaternion.Euler(0, -sFoilAngleSet, 0);
            transform.Find("RightWingBottom").localRotation = Quaternion.Euler(0, sFoilAngleSet, 0);
        }
    }

    // Adjust engine light intensity/particle effects
    void engineLightIntensity()
    {
        if (throttleLevel < 100)
        {
            particleLB.startColor = new Color(0, 0, 0, 0);
            particleLT.startColor = new Color(0, 0, 0, 0);
            particleRB.startColor = new Color(0, 0, 0, 0);
            particleRT.startColor = new Color(0, 0, 0, 0);
        }
        else
        {
            Color particleColor = new Color(238, 84, 163, Mathf.Lerp(0, 255, enginePower / (float)enginePowerLimit));
            particleLB.startColor = particleColor;
            particleLT.startColor = particleColor;
            particleRB.startColor = particleColor;
            particleRT.startColor = particleColor;
        }
        //Update Engine Light Intensity
        float engineLightIntensity = Mathf.Lerp(0.0f, 1.0f, realSpeedGoal / maxSpeed);
        engineLightLB.intensity = engineLightIntensity;
        engineLightLT.intensity = engineLightIntensity;
        engineLightRB.intensity = engineLightIntensity;
        engineLightRT.intensity = engineLightIntensity;
    }

    //************************************************OVERRIDE FUNCTIONS************************************************//
    // Override function to update laser charge, taking into account S-Foils
    public override string getShipName()
    {
        return "X-WING";
    }

    protected override void updateLaserCharge()
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
                laserChargeTimestamp = Time.time + 0.5f;
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
                laserChargeTimestamp = Time.time + 1f;
            }
        }
        else if (laserPower == 2) // static power generation
        {
            return;
        }
        else if (laserPower == 3) // gains a lser charge every second
        {
            if (laserCharge == 50 || !sFoilToggle)
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge++;
                laserChargeTimestamp = Time.time + 1f;
            }
        }
        else
        {
            if (laserCharge == 50 || !sFoilToggle) // gains two laser charge every second
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge++;
                laserChargeTimestamp = Time.time + 0.5f;
            }
        }
    }

    // Override function to get laser convergence distance
    public override float getLaserConvergence()
    {
        return targetingConvergence;
    }

    // Override function to get missile convergence distance
    public override float getMissileConvergence()
    {
        return missileConvergence;
    }
    public override int getCurrentWeaponAmmoCount()
    {
        if (weaponSelection == 0)
        {
            return 0;
        }
        else if (weaponSelection == 1)
        {
            return cMissileCount;
        }
        else if (weaponSelection == 2)
        {
            return protonTorpCount;
        }
        else if (weaponSelection == 3)
        {
            return ionMissileCount;
        }
        else if (weaponSelection == 4)
        {
            return ionTorpCount;
        }
        else
        {
            return 0;
        }
    }

    // Override function to retrieve current weapon selection
    public override int getCurrentWeaponSystem()
    {
        return weaponSelection;
    }

    // X-Wing maneuverability factor calculation, taking into account s-foil maneuverability boost
    protected override float calcMFactor(int ePower, int maxEPower, float tLevel)
    {
        float throttleFactor = 1f + 12f / (10f * Mathf.Sqrt(2f * Mathf.PI)) * Mathf.Exp(-Mathf.Pow(tLevel - 50, 2) / 200f);

        float mFactorBoost = 1f;
        if (driftStatus)
        {
            mFactorBoost = 1.5f;
        }
        if (!sFoilToggle) // 20% boost in maneuverability if sfoils are closed
        {
            mFactorBoost *= 1.2f;
        }

        return (ePower + 20f) / ((float)maxEPower + 20f) * throttleFactor * mFactorBoost;
    }

    void Update()
    {
        AIControllerUpdate();
        fireCannonsUpdate();
        ShipControllerUpdate();
    }

    void FixedUpdate()
    {
        sFoilTransition();
        engineLightIntensity();
        calcTargetGuide(currentTarget != null);
        targetLock(currentTarget != null);
        AIControllerFixedUpdate();
        ShieldedAIFixedUpdate();

        if (currentTarget == null)
        {
            randomCycleTargets();
            if (currentTarget == null)
            {
                //SlowCirclePatrol();
                patrolRoute(false);
            }
            else
            {
                if (patrolActive)
                {
                    patrolRoute(true);
                }
                else
                {
                    attackPlayer();
                }
            }
        }
        else
        {
            //SlowCirclePatrol();
            if (patrolActive)
            {
                patrolRoute(true);
            }
            else
            {
                attackPlayer();
            }
        }
    }
}
