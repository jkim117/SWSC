using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TieINController : UnShieldedPlayerController
{
    // WEAPON VALUES CONFIGURABLE
    private float singleFireRate = 0.1f; // single weapons fire rate
    private float dualFireRate = 0.19f; // dual weapons fire rate
    private float quadFireRate = 0.36f; // quad fire rate
    public GameObject blasterBoltPrefab; // prefab used for blaster bolts
    // WEAPON VALUES VARIABLE
    private float fireStatus = 0f;
    private float fireRateTimestamp;
    private float fireRate;
    private int fireLinkStatus;
    private int fireLinkRotation;

    // TARGETING VALUES CONFIGURABLE
    private float fullTimeTillLock = 5.0f;
    private float staticTimeTillNextTargetSound = 0.08f;
    // TARGETING VALUES VARIABLE
    private bool targetLockStatus = false;
    private bool targetLockReset = true; // set to false when during lock. Set to true once lock broken.
    private float timeTillLock;
    private float timeTillNextTargetSound;
    private float targetingConvergence = 300f;
    private Vector3 targetLockGuide;

    // EFFECTS VALUES CONFIGURABLE
    private Light engineLightR;
    private Light engineLightL;
    private ParticleSystem.MainModule particleR;
    private ParticleSystem.MainModule particleL;
    public AudioSource blasterSound; // audio source used for blaster sounds
    public AudioClip blasterClip; // audio clip used for blaster sound
    public AudioClip targetLockClip;
    public AudioClip targetingClip;

    // Start is called before the first frame update
    void Start()
    {
        ShipControllerStart();
        PlayerControllerStart();
        overallPowerLimit = 8;

        // Initialize Ship Specific Constants
        rollValue = 104f * 2.0f; // determines speed of rolls, derived from DPF
        rollAcceleration = 4.24f; // acceleration of rolls, derived from G
        lookRateSpeedPitch = 104f * 1.25f; // determines the speed of pitching/yawing, derived from DPF
        lookRateSpeedYaw = 104f * 0.75f; // derived from DPF
        forwardAcceleration = 0.424f * 2.0f; // determines main engine accleration value, derived from G
        maxSpeed = 347.22f; // determines max forward speed, derived from kph
        repulsorAcceleration = 1.1f; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
        pitchAcceleration = 1.04f * 4.24f * 1.25f; // derived from DPF and G
        yawAcceleration = 1.04f * 4.24f * 0.75f; // derived from DPF and G
        totalShipHealth = 90f;
        maxRepulsorValue = 104.0f; // max repulsor speed, derived from DPF
        sensorRangeLimit = 700f;
        sensorBurnLimit = 300f;

        // Initialize targeting values
        timeTillLock = fullTimeTillLock;
        timeTillNextTargetSound = staticTimeTillNextTargetSound;

        // Initialize Health values
        shipHealth = 90f;
        shipIonHealth = totalShipHealth;

        // Initialize effects
        engineLightL = transform.Find("EngineLightL").gameObject.GetComponent<Light>();
        engineLightR = transform.Find("EngineLightR").gameObject.GetComponent<Light>();
        particleR = transform.Find("EngineParticlesR").gameObject.GetComponent<ParticleSystem>().main;
        particleL = transform.Find("EngineParticlesL").gameObject.GetComponent<ParticleSystem>().main;

        // Initialize Movement Values
        throttleLevel = 0;
        throttleStatus = 0;
        repulsorStatus = 0;

        // Initialize Power values
        enginePower = 4;
        laserPower = 2;
        overallPower = 6;

        // Initialize Weapon Values
        fireRate = singleFireRate;
        fireLinkStatus = 0; // 1 for dual fire, 0 for single fire

        //Debug.Log(GetComponent<Renderer>().bounds.size);
    }

    //************************************************INPUT FUNCTIONS************************************************//
    // Input function to adjust targeting convergence
    void OnAdjustTargeting(InputValue value)
    {
        if (value.Get<float>() < 0)
        {
            changeTargetingConvergence(-1f);
        }
        else if (value.Get<float>() > 0)
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
        timeTillNextTargetSound = staticTimeTillNextTargetSound;
        targetLockReset = true;
        targetCircle.gameObject.SetActive(false);
        crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);

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

    void OnFire(InputValue value)
    {
        fireStatus = value.Get<float>();
    }
    void OnLinkCannons(InputValue value)
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
            targetCircle.gameObject.SetActive(false);
            return;
        }
        Vector3 shipToTarget = currentTarget.getJamPosition(true, out jammed) - transform.position;
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
                    targetCircle.gameObject.SetActive(true);
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
                    shipSounds.PlayOneShot(targetLockClip);
                    targetCircle.gameObject.SetActive(false);
                    crosshair.GetComponent<Image>().color = new Color32(255, 0, 0, 100);

                    return;
                }


                float targetCircleScale = Mathf.Lerp(7f, 5f, (fullTimeTillLock - timeTillLock) / fullTimeTillLock);
                targetCircle.localScale = new Vector3(targetCircleScale, targetCircleScale, 1);


                timeTillNextTargetSound = timeTillNextTargetSound - Time.fixedDeltaTime;

                if (timeTillNextTargetSound <= 0f)
                {
                    shipSounds.PlayOneShot(targetingClip);
                    timeTillNextTargetSound = staticTimeTillNextTargetSound;
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
            targetGuide.gameObject.SetActive(false);
            targetLockStatus = false;
            crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
            return;
        }
        Vector3 targetLocation = currentTarget.getPosition();
        Vector3 targetVelocity = currentTarget.getVelocity();
        Vector3 laserVelocity = totalVelocityVector - 750f * transform.up;

        float time_laser_travel = Vector3.Distance(targetLocation, transform.position) / laserVelocity.magnitude;
        Vector3 targetFutureLocation = targetLocation + targetVelocity * time_laser_travel;
        //targetLockGuide = transform.TransformPoint(transform.InverseTransformPoint(targetFutureLocation).normalized * targetingConvergence);
        targetLockGuide = targetFutureLocation;
    }

    //************************************************WEAPON SYSTEM FUNCTIONS************************************************//

    void shootRayLB()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleLB = transform.Find("MuzzleLeftBottom");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleLB.position, muzzleLB.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLB.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLB.position).normalized, 2000, 10, false);
        }
    }

    void shootRayRB()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleRB = transform.Find("MuzzleRightBottom");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleRB.position, muzzleRB.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRB.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRB.position).normalized, 2000, 10, false);
        }

    }

    void shootRayLT()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleLT = transform.Find("MuzzleLeftTop");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleLT.position, muzzleLT.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLT.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLT.position).normalized, 2000, 10, false);
        }

    }

    void shootRayRT()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleRT = transform.Find("MuzzleRightTop");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzleRT.position, muzzleRT.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRT.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRT.position).normalized, 2000, 10, false);
        }

    }

    void fireCannonsUpdate()
    {
        if (fireStatus == 1f && Time.time > fireRateTimestamp)
        {
            if (laserCharge > 0)
            {
                blasterSound.PlayOneShot(blasterClip);
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
    public override float getLaserConvergence()
    {
        return targetingConvergence;
    }

    void Update()
    {
        fireCannonsUpdate();
        PlayerControllerUpdate();
    }


    void FixedUpdate()
    {
        engineLightIntensity();
        calcTargetGuide(currentTarget != null);
        targetLock(currentTarget != null);
        PlayerControllerFixedUpdate();
    }
}
