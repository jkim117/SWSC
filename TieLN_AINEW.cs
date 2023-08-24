using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TieLN_AINEW : UnShieldedAIController
{
    // WEAPON VALUES CONFIGURABLE
    private float singleFireRate = 0.1f; // single weapons fire rate
    private float dualFireRate = 0.18f; // dual weapons fire rate
    public GameObject blasterBoltPrefab; // prefab used for blaster bolts
    // WEAPON VALUES VARIABLE
    private float fireStatus = 0f;
    private float fireRateTimestamp;
    private float fireRate;
    private int fireLinkStatus;
    private int fireLinkRotation;

    // TARGETING VALUES CONFIGURABLE
    private float fullTimeTillLock = 5.0f;
    // TARGETING VALUES VARIABLE
    private bool targetLockStatus = false;
    private bool targetLockReset = true; // set to false when during lock. Set to true once lock broken.
    private float timeTillLock;
    private float targetingConvergence = 300f;
    private Vector3 targetLockGuide;

    // EFFECTS VALUES CONFIGURABLE
    private Light engineLightR;
    private Light engineLightL;
    private ParticleSystem.MainModule particleR;
    private ParticleSystem.MainModule particleL;
    public AudioSource blasterSound; // audio source used for blaster sounds
    public AudioClip blasterClip; // audio clip used for blaster sound

    // Start is called before the first frame update
    void Start()
    {
        ShipControllerStart();
        AIControllerStart();
        overallPowerLimit = 8;

        // Initialize Ship Specific Constants:
        rollValue = 100f * 2.0f; // determines speed of rolls, derived from DPF
        rollAcceleration = 4.1f; // acceleration of rolls, derived from G
        lookRateSpeedPitch = 100f * 1.25f; // determines the speed of pitching/yawing, derived from DPF
        lookRateSpeedYaw = 100f * 0.75f; // derived from DPF
        forwardAcceleration = 0.41f * 2.0f; // determines main engine accleration value, derived from G
        maxSpeed = 333.33f; // determines max forward speed, derived from kph
        repulsorAcceleration = 1.0f; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
        pitchAcceleration = 4.1f * 1.25f; // derived from DPF and G
        yawAcceleration = 4.1f * 0.75f; // derived from DPF and G
        totalShipHealth = 100f;
        maxRepulsorValue = 100.0f;
        sensorRangeLimit = 500f;
        sensorBurnLimit = 200f;

        // Initialize targeting values
        timeTillLock = fullTimeTillLock;

        // Initialize Health values
        shipHealth = 100f;
        shipIonHealth = totalShipHealth;

        // Initialize effects
        engineLightL = transform.Find("EngineLightL").gameObject.GetComponent<Light>();
        engineLightR = transform.Find("EngineLightR").gameObject.GetComponent<Light>();
        particleR = transform.Find("EngineParticlesR").gameObject.GetComponent<ParticleSystem>().main;
        particleL = transform.Find("EngineParticlesL").gameObject.GetComponent<ParticleSystem>().main;

        // Initialize movement values
        throttleLevel = 90;
        throttleStatus = 0;
        repulsorStatus = 0;

        // Initialize power values
        enginePower = 4;
        laserPower = 2;
        overallPower = 6;

        // Initialize weapon values
        fireRate = dualFireRate;
        fireLinkStatus = 1; // 0 for single fire, 1 for dual fire

        //Debug.Log(GetComponent<Renderer>().bounds.size);
        transformStartPosition = transform.position;
        centerPosition = transform.position;
        defaultPatrolRoute = new Vector3[] { transform.position + new Vector3(1000, UnityEngine.Random.Range(100, 300), 1000), transform.position + new Vector3(-1000, UnityEngine.Random.Range(100, 300), 1000), transform.position + new Vector3(-1000, UnityEngine.Random.Range(100, 300), -1000), transform.position + new Vector3(1000, UnityEngine.Random.Range(100, 300), -1000) };
        if (!friendly)
        {
            if (CrossSceneValues.difficulty == 0)
            {
                skillLevel = 1;
                maxSpeed = 275f;
            }
            else if (CrossSceneValues.difficulty == 1)
            {
                skillLevel = 3;
                maxSpeed = 300;
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

            setThrottle(70f);

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


        //Debug.Log(Vector3.SignedAngle(transform.forward, Vector3.Cross(transform.up, Vector3.Cross(Vector3.up, transform.up))));
        // If ship is not level with the ground, roll until it is
        float rollAngle = Vector3.SignedAngle(transform.forward, Vector3.Cross(transform.up, Vector3.Cross(Vector3.up, transform.up)), transform.up);
        /*if (Math.Abs(transform.localRotation.eulerAngles.x - (270f)) > 30f)
        {
            OnRoll(1f);
        }
        else
        {
            OnRoll(0f);
        }*/
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

        
        distanceToTarget = targetDistance;
    }

    void attackPlayerStart()
    {
        // Adjust power to full power. 3 power pips to lasers, rest to engines
        OnOverallPower();
        OnOverallPower();
        //OnLaserPower(1f);
        //OnLaserPower(0f);
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
        for (float phi = 0; phi <= 2*Mathf.PI; phi += Mathf.PI/24)
        {
            for (float theta = 0; theta <= Mathf.PI; theta += Mathf.PI/24)
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
                bool hitBool = Physics.Raycast(transform.position, rayCastVector, out hit, rayDistance);

                if (hitBool)
                {
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

        if (targetingConvergence - distanceToTarget > 51)
        {
            OnAdjustTargeting(-1f);
        }
        else if (distanceToTarget - targetingConvergence > 51)
        {
            OnAdjustTargeting(1f);
        }

        if (currentTarget != null)
        {
            if (targetLockStatus || (Vector3.Angle(-transform.up, currentTarget.transform.position - transform.position) < 10 && distanceToTarget <= 500))
            {
                OnFire(1f);
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

        if (targetingConvergence - distanceToTarget > 51)
        {
            OnAdjustTargeting(-1f);
        }
        else if (distanceToTarget - targetingConvergence > 51)
        {
            OnAdjustTargeting(1f);
        }

        if (targetLockStatus || (recoveryMode == 0 && !isTurning && distanceToTarget <= 500) || (Vector3.Angle(-transform.up, currentTarget.transform.position - transform.position) < 10 && distanceToTarget <= 500))
        //if (targetLockStatus)
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
        transform.Find("TargetingConvergence").position = transform.position + -300f * transform.up;
        targetingConvergence = 300f;
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

    void OnFire(float value)
    {
        fireStatus = value;
    }

    void OnLinkCannons()
    {
        fireLinkRotation = 0;
        if (fireLinkStatus == 0)
        {
            fireLinkStatus = 1;
            fireRate = dualFireRate;
        }
        else
        {
            fireLinkStatus = 0;
            fireRate = singleFireRate;
        }
    }

    //************************************************TARGETING SYSTEM FUNCTIONS************************************************//
    // Change targeting convergence distance
    void changeTargetingConvergence(float changeValue)
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
    bool targetLockBreak(float targetAngle, float targetDistance)
    {
        if (targetAngle > 15f || !targetingComputerToggle)
        {
            return true;
        }

        if (targetDistance > 1000f || Mathf.Abs(targetingConvergence - targetDistance) > 300f)
        {
            return true;
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

                targetingConvergenceDifference += Mathf.Abs(targetDistance - targetingConvergence);
                // The closer the targeting convergence to actual distance and the closer to the center of sight, the faster the lock
                targetingRateFactor = targetingConvergenceDifference * targetAngle / 300f;


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

        float d_t = 1.1f * Mathf.Sqrt(k * k / (Mathf.Pow(laserVelocity.magnitude, 2f) / Mathf.Pow(targetPlaneVelocity.magnitude, 2f) - 1));
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

    //************************************************WEAPON SYSTEM FUNCTIONS************************************************//
    void shootRayL()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleLeft = transform.Find("MuzzleLeft");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleLeft.position, muzzleLeft.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLeft.position).normalized, 2000, 7.5f, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLeft.position).normalized, 2000, 7.5f, false);
        }
    }

    void shootRayR()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleRight = transform.Find("MuzzleRight");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleRight.position, muzzleRight.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRight.position).normalized, 2000, 7.5f, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRight.position).normalized, 2000, 7.5f, false);
        }
    }

    void fireCannonsUpdate()
    {
        if (fireStatus == 1f && Time.time > fireRateTimestamp)
        {
            if (laserCharge > 0)
            {
                blasterSound.PlayOneShot(blasterClip);
                if (fireLinkStatus == 1)
                {
                    shootRayL();
                    shootRayR();
                }
                else
                {
                    if (fireLinkRotation == 0)
                    {
                        shootRayL();
                        fireLinkRotation++;
                    }
                    else
                    {
                        shootRayR();
                        fireLinkRotation--;
                    }
                }
                fireRateTimestamp = Time.time + fireRate;
            }

        }
    }

    //************************************************MISC FUNCTIONS************************************************//
    void engineLightIntensity()
    {
        if (throttleLevel < 100)
        {
            particleR.startColor = new Color(0, 0, 0, 0);
            particleL.startColor = new Color(0, 0, 0, 0);
        }
        else
        {
            Color particleColor = new Color(255, 0, 0, Mathf.Lerp(0, 20, enginePower / (float)enginePowerLimit));
            particleL.startColor = particleColor;
            particleR.startColor = particleColor;
        }
        //Update Engine Light Intensity
        float engineLightIntensity = Mathf.Lerp(0.0f, 0.1f, realSpeedGoal / maxSpeed);
        engineLightL.intensity = engineLightIntensity;
        engineLightR.intensity = engineLightIntensity;
    }

    //************************************************OVERRIDE FUNCTIONS************************************************//
    // Override function to get laser convergence distance
    public override string getShipName()
    {
        return "TIE/LN";
    }

    public override float getLaserConvergence()
    {
        return targetingConvergence;
    }

    void Update()
    {
        AIControllerUpdate();
        fireCannonsUpdate();
        ShipControllerUpdate();
    }



    void FixedUpdate()
    {
        

        engineLightIntensity();
        calcTargetGuide(currentTarget != null);
        targetLock(currentTarget != null);
        AIControllerFixedUpdate();
        UnShieldedAIFixedUpdate();

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
