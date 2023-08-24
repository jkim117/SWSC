using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AWingController : ShieldedPlayerController
{
    // WEAPON VALUES CONFIGURABLE
    private float singleFireRate = 0.1f; // single weapons fire rate
    private float dualFireRate = 0.19f; // dual weapons fire rate
    public GameObject blasterBoltPrefab; // prefab used for blaster bolts
    public GameObject missilePrefab;
    public GameObject ionmissilePrefab;
    public int cMissileCount = 6;
    public int ionMissileCount = 6;

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
    private float staticTimeTillNextTargetSound = 0.25f;
    // TARGETING VALUES VARIABLE
    private bool targetLockStatus = false;
    private bool targetLockReset = true; // set to false when during lock. Set to true once lock broken.
    private float timeTillLock;
    private float timeTillNextTargetSound;
    private float targetingConvergence = 300f;
    private float missileConvergence = 1000f;
    private Vector3 targetLockGuide;

    // EFFECT VALUES CONFIGURABLE
    private Light engineLightR;
    private Light engineLightL;
    private ParticleSystem.MainModule particleR;
    private ParticleSystem.MainModule particleL;
    public AudioClip blasterClip; // audio clip used for blaster sound
    public AudioClip missileClip;
    public AudioClip ionmissileClip;
    public AudioClip targetLockClip;
    public AudioClip targetingClip;

    // Start is called before the first frame update
    void Start()
    {
        ShipControllerStart();
        PlayerControllerStart();

        // Initialize Ship Specific Constants:
        rollValue = 96f * 2.0f; // determines speed of rolls, derived from DPF
        rollAcceleration = 5.1f; // acceleration of rolls, derived from G
        lookRateSpeedPitch = 96f * 1.25f; // determines the speed of pitching/yawing, derived from DPF
        lookRateSpeedYaw = 96f * 0.75f; // derived from DPF
        forwardAcceleration = 0.51f * 2.0f; // determines main engine accleration value, derived from G
        maxSpeed = 361.11f; // determines max forward speed, derived from kph
        repulsorAcceleration = 1.1f; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
        pitchAcceleration = .96f * 5.1f * 1.25f; // derived from DPF and G
        yawAcceleration = .96f * 5.1f * 0.75f; // derived from DPF and G
        totalShipHealth = 60f;
        totalForwardShieldHealth = 10;
        totalRearShieldHealth = 10;
        totalShieldLimit = 30;
        maxRepulsorValue = 96.0f; // max repulsor speed, derived from DPF
        sensorRangeLimit = 5000f;
        sensorBurnLimit = 500f;

        // Initialize targeting values
        timeTillLock = fullTimeTillLock;
        timeTillNextTargetSound = staticTimeTillNextTargetSound;

        // Initialize Health values
        shipHealth = 60f;
        shipIonHealth = totalShipHealth;
        forwardShieldHealth = 10f;
        rearShieldHealth = 10f;
        forwardOverShieldHealth = 5f;
        rearOverShieldHealth = 5f;

        shieldTransfer0 = 0.05f;
        shieldTransfer1 = 0.075f;
        shieldTransfer2 = 0.1f;
        shieldTransfer3 = 0.125f;
        shieldTransfer4 = 0.15f;
        shieldChange0 = -0.025f;
        shieldChange1 = -0.0125f;
        shieldChange3 = 0.025f;
        shieldChange4 = 0.05f;
        shieldRegenDelayFull = 7f;

        // Initialize effects
        engineLightL = transform.Find("EngineLightL").gameObject.GetComponent<Light>();
        engineLightR = transform.Find("EngineLightR").gameObject.GetComponent<Light>();
        particleR = transform.Find("EngineParticlesR").gameObject.GetComponent<ParticleSystem>().main;
        particleL = transform.Find("EngineParticlesL").gameObject.GetComponent<ParticleSystem>().main;

        // Initialize movement values
        throttleLevel = 0;
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

        //Debug.Log(transform.Find("Body").GetComponent<Renderer>().bounds.size);
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
        timeTillNextTargetSound = 0.25f;
        staticTimeTillNextTargetSound = 0.25f;
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

    // Weapon selection function
    void OnSwitchWeapon()
    {
        int oldWeaponSelection = weaponSelection;

        if (weaponSelection == 0)
        {
            weaponSelection = 1;
        }
        else if (weaponSelection == 1)
        {
            weaponSelection = 3;
        }
        else if (weaponSelection == 3)
        {
            weaponSelection = 0;
        }

        if (weaponSelection == 1 && cMissileCount == 0) // switched to concussion missiles
        {
            weaponSelection = 3;
        }
        if (weaponSelection == 3 && ionMissileCount == 0) // switched to ion missile
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
    void OnFire(InputValue value)
    {
        fireStatus = value.Get<float>();
    }

    // Cannon link function
    void OnLinkCannons(InputValue value)
    {
        if (weaponSelection == 0)
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
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLeft.position).normalized, 2000, 15, false);
            //laser.GetComponent<ShotBehavior>().setTarget(totalVelocityVector, (targetLockGuide - muzzleLeft.position).normalized, currentTarget);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLeft.position).normalized, 2000, 15, false);
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
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRight.position).normalized, 2000, 15, false);
            //laser.GetComponent<ShotBehavior>().setTarget(totalVelocityVector, (targetLockGuide - muzzleRight.position).normalized, currentTarget);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRight.position).normalized, 2000, 15, false);
        }
    }

    void shootTorpL()
    {
        Transform torpMuzzleL = transform.Find("TorpMuzzleL");
        GameObject currentPrefab = missilePrefab;
       
        if (weaponSelection == 3)
        {
            currentPrefab = ionmissilePrefab;
        }
 
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzleL.position, torpMuzzleL.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleL.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleL.position).normalized, currentTarget, weaponSelection, true);
        }

    }

    void shootTorpR()
    {
        Transform torpMuzzleR = transform.Find("TorpMuzzleR");
        GameObject currentPrefab = missilePrefab;
      
        if (weaponSelection == 3)
        {
            currentPrefab = ionmissilePrefab;
        }
  
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzleR.position, torpMuzzleR.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleR.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleR.position).normalized, currentTarget, weaponSelection, true);
        }
    }

    void fireCannonsUpdate()
    {
        if (fireStatus == 1f && Time.time > fireRateTimestamp && weaponSelection == 0)
        {
            if (laserCharge > 0)
            {
                shipSounds.PlayOneShot(blasterClip);
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
        else if (fireStatus == 1f && weaponSelection != 0)
        {
            if ((getCurrentWeaponAmmoCount() > 0 && torpFireLinkStatus == 0) || getCurrentWeaponAmmoCount() == 1)
            {
                if (weaponSelection == 1)
                {
                    shipSounds.PlayOneShot(missileClip);
                    cMissileCount--;
                }
                else if (weaponSelection == 3)
                {
                    shipSounds.PlayOneShot(ionmissileClip);
                    ionMissileCount--;
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
                else if (weaponSelection == 3)
                {
                    shipSounds.PlayOneShot(ionmissileClip);
                    ionMissileCount -= 2;
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
                if (weaponSelection == 0) // lasers
                {
                    targetingConvergenceDifference += Mathf.Abs(targetDistance - targetingConvergence);
                    // The closer the targeting convergence to actual distance and the closer to the center of sight, the faster the lock
                    targetingRateFactor = targetingConvergenceDifference * targetAngle / 300f;
                }
                else
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
                    shipSounds.PlayOneShot(targetLockClip);
                    targetCircle.gameObject.SetActive(false);
                    crosshair.GetComponent<Image>().color = new Color32(255, 0, 0, 100);

                    return;
                }


                float targetCircleScale = Mathf.Lerp(7f, 5f, (fullTimeTillLock - timeTillLock) / fullTimeTillLock);
                targetCircle.localScale = new Vector3(targetCircleScale, targetCircleScale, 1);

                float targetSoundRateFactor = Mathf.Log(1 / (targetingConvergenceDifference * targetAngle / 100f), 2f);
                if (targetSoundRateFactor > 1)
                {

                    timeTillNextTargetSound = timeTillNextTargetSound - Time.fixedDeltaTime * targetSoundRateFactor; // targeting beep increases if targeting rate is high
                }
                else
                {
                    timeTillNextTargetSound = timeTillNextTargetSound - Time.fixedDeltaTime;
                }

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
            Color particleColor = new Color(238, 219, 84, Mathf.Lerp(0, 255, enginePower / (float)enginePowerLimit));
            particleL.startColor = particleColor;
            particleR.startColor = particleColor;
        }

        //Update Engine Light Intensity
        float engineLightIntensity = Mathf.Lerp(0.0f, 1.0f, realSpeedGoal / maxSpeed);
        engineLightL.intensity = engineLightIntensity;
        engineLightR.intensity = engineLightIntensity;
    }

    //************************************************OVERRIDE FUNCTIONS************************************************//
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

    // Override function to retrieve current weapon selection
    public override int getCurrentWeaponSystem()
    {
        return weaponSelection;
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
        else if (weaponSelection == 3)
        {
            return ionMissileCount;
        }
        else
        {
            return 0;
        }
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
        ShieldedPlayerFixedUpdate();
    }
}
