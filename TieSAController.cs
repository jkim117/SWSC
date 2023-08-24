using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TieSAController : UnShieldedPlayerController
{
    // WEAPON VALUES CONFIGURABLE
    private float singleFireRate = 0.2f; // single weapons fire rate
    private float dualFireRate = 0.38f; // dual weapons fire rate
    public GameObject blasterBoltPrefab; // prefab used for blaster bolts
    public GameObject missilePrefab;
    public GameObject ionmissilePrefab;
    public GameObject iontorpPrefab;
    public GameObject protontorpPrefab;
    public GameObject protonbombPrefab;
    public GameObject ionbombPrefab;
    public int cMissileCount = 3;
    public int protonTorpCount = 3;
    public int ionMissileCount = 2;
    public int ionTorpCount = 2;
    public int protonBombCount = 5;
    public int ionBombCount = 5;
    // WEAPON VALUES VARIABLE
    private float fireStatus = 0f;
    private float fireRateTimestamp;
    private float fireRate;
    private int fireLinkStatus;
    private int fireLinkRotation;
    private int weaponSelection = 0; // 0 for lasers, 1 for missiles

    // TARGETING VALUES CONFIGURABLE
    private float fullTimeTillLock = 5.0f;
    private float staticTimeTillNextTargetSound = 0.08f;
    // TARGETING VALUES VARIABLE
    private bool targetLockStatus = false;
    private bool targetLockReset = true; // set to false when during lock. Set to true once lock broken.
    private float timeTillLock;
    private float timeTillNextTargetSound;
    private float targetingConvergence = 300f;
    private float missileConvergence = 1000f;
    private Vector3 targetLockGuide;

    // EFFECT VALUES CONFIGURABLE
    private Light engineLight1;
    private Light engineLight2;
    private Light engineLight3;
    private Light engineLight4;
    private ParticleSystem.MainModule particle1;
    private ParticleSystem.MainModule particle2;
    private ParticleSystem.MainModule particle3;
    private ParticleSystem.MainModule particle4;
    public AudioClip blasterClip; // audio clip used for blaster sound
    public AudioClip missileClip;
    public AudioClip protonTorpClip;
    public AudioClip ionmissileClip;
    public AudioClip iontorpClip;
    public AudioClip protonbombClip;
    public AudioClip ionbombClip;
    public AudioClip targetLockClip;
    public AudioClip targetingClip;

    // Start is called before the first frame update
    void Start()
    {
        ShipControllerStart();
        PlayerControllerStart();
        overallPowerLimit = 8;

        // Initialize Ship Specific Constants:
        rollValue = 86f * 2.0f; // determines speed of rolls, derived from DPF
        rollAcceleration = 2.38f; // acceleration of rolls, derived from G
        lookRateSpeedPitch = 86f * 1.25f; // determines the speed of pitching/yawing, derived from DPF
        lookRateSpeedYaw = 86f * 0.75f; // derived from DPF
        forwardAcceleration = 0.238f * 2.0f; // determines main engine accleration value, derived from G
        maxSpeed = 236.111f; // determines max forward speed, derived from kph
        repulsorAcceleration = 0.9f; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
        pitchAcceleration = .86f * 2.38f * 1.25f; // derived from DPF and G
        yawAcceleration = .86f * 2.38f * 0.75f; // derived from DPF and G
        totalShipHealth = 200f;
        maxRepulsorValue = 70.0f; // max repulsor speed, derived from DPF
        sensorRangeLimit = 1000f;
        sensorBurnLimit = 500f;

        // Initialize targeting values
        timeTillLock = fullTimeTillLock;
        timeTillNextTargetSound = staticTimeTillNextTargetSound;

        // Initialize Health values
        shipHealth = 200f;
        shipIonHealth = totalShipHealth;

        // Initialize effects
        engineLight1 = transform.Find("EngineLight1").gameObject.GetComponent<Light>();
        engineLight2 = transform.Find("EngineLight2").gameObject.GetComponent<Light>();
        engineLight3 = transform.Find("EngineLight3").gameObject.GetComponent<Light>();
        engineLight4 = transform.Find("EngineLight4").gameObject.GetComponent<Light>();
        particle1 = transform.Find("EngineParticles1").gameObject.GetComponent<ParticleSystem>().main;
        particle2 = transform.Find("EngineParticles2").gameObject.GetComponent<ParticleSystem>().main;
        particle3 = transform.Find("EngineParticles3").gameObject.GetComponent<ParticleSystem>().main;
        particle4 = transform.Find("EngineParticles4").gameObject.GetComponent<ParticleSystem>().main;

        // Initialize movement values
        throttleLevel = 0;
        throttleStatus = 0;
        repulsorStatus = 0;

        // Initialize power values
        enginePower = 4;
        laserPower = 2;
        overallPower = 6;

        // Initialize weapon values
        fireRate = dualFireRate;
        fireLinkStatus = 1; // 0 for single fire, 1 for dual fire, 2 for quad fire
        laserChargeLimit = 75f;
        laserCharge = 75f;
        chargeRate3 = 0.3f;
        chargeRate4 = 0.2f;

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
        if (weaponSelection == 0) // lasers
        {
            transform.Find("TargetingConvergence").position = transform.position + -300f * transform.up;
            targetingConvergence = 300f;
        }
        else if (weaponSelection < 5) // missiles
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

    // Weapon selection function
    void OnSwitchWeapon()
    {
        int oldWeaponSelection = weaponSelection;

        weaponSelection = (weaponSelection + 1) % 7;


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
            weaponSelection = 5;
        }
        if (weaponSelection == 5 && protonBombCount == 0)
        {
            weaponSelection = 6;
        }
        if (weaponSelection == 6 && ionBombCount == 0)
        {
            weaponSelection = 0;
        }

        // If switched from lasers to munitions or vice versa cancel target lock
        if ((oldWeaponSelection == 0 && weaponSelection != 0) || (oldWeaponSelection != 0 && weaponSelection == 0) || weaponSelection == 5 || weaponSelection == 6)
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
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLeft.position).normalized, 2000, 22, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLeft.position).normalized, 2000, 22, false);
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
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRight.position).normalized, 2000, 22, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRight.position).normalized, 2000, 22, false);
        }
    }

    void shootTorp()
    {
        Transform torpMuzzle = transform.Find("TorpMuzzle");
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
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzle.position, torpMuzzle.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, currentTarget, weaponSelection, true);
        }

    }
    void shootBomb()
    {
        Transform bombMuzzle = transform.Find("BombMuzzle");

        GameObject bombPrefab = protonbombPrefab;
        if (weaponSelection == 6)
        {
            bombPrefab = ionbombPrefab;
        }

        GameObject bomb = GameObject.Instantiate(bombPrefab, bombMuzzle.position, bombMuzzle.rotation) as GameObject;
        bomb.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, bombMuzzle.forward, weaponSelection);
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
        else if (fireStatus == 1f && weaponSelection > 0 && weaponSelection < 5)
        {
            if (getCurrentWeaponAmmoCount() > 0)
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
                shootTorp();
            }
            fireStatus = 0;
        }
        else if (fireStatus == 1f && weaponSelection >= 5)
        {
            if (getCurrentWeaponAmmoCount() > 0)
            {
                if (weaponSelection == 5)
                {
                    shipSounds.PlayOneShot(protonbombClip);
                    protonBombCount--;
                }
                else if (weaponSelection == 6)
                {
                    shipSounds.PlayOneShot(ionbombClip);
                    ionBombCount--;
                }
                shootBomb();
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
        else if (weaponSelection < 5) // missiles
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
        if (weaponSelection >= 5)
        {
            return;
        }
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
                else if (weaponSelection < 5)
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
            crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
            targetLockStatus = false;
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
            particle1.startColor = new Color(0, 0, 0, 0);
            particle2.startColor = new Color(0, 0, 0, 0);
            particle3.startColor = new Color(0, 0, 0, 0);
            particle4.startColor = new Color(0, 0, 0, 0);
        }
        else
        {
            Color particleColor = new Color(255, 0, 0, Mathf.Lerp(0, 20, enginePower / (float)enginePowerLimit));
            particle1.startColor = particleColor;
            particle2.startColor = particleColor;
            particle3.startColor = particleColor;
            particle4.startColor = particleColor;
        }

        //Update Engine Light Intensity
        float engineLightIntensity = Mathf.Lerp(0.0f, 0.1f, realSpeedGoal / maxSpeed);
        engineLight1.intensity = engineLightIntensity;
        engineLight2.intensity = engineLightIntensity;
        engineLight3.intensity = engineLightIntensity;
        engineLight4.intensity = engineLightIntensity;
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
        else if (weaponSelection == 5)
        {
            return protonBombCount;
        }
        else if (weaponSelection == 6)
        {
            return ionBombCount;
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