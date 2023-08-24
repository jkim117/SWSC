using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TLController : MonoBehaviour
{
    public ShipController currentTarget;
    private float fireRateMax = 5.5f;
    private float fireRateMin = 2.5f;
    private float turnSpeed = 50f;
    public ShipController parent;
    private Transform muzzle;
    public GameObject blasterBoltPrefab;
    private float fireRateTimestamp = 0;

    private float targetDistanceLimit = 10000;
    private float targetAngleOffsetLimit = 20;
    public AudioSource blasterSound;
    public AudioClip blasterClip;

    public void alterRateOfFire(float minRate, float maxRate, float fireTurnSpeed)
    {
        fireRateMax = maxRate;
        fireRateMin = minRate;
        turnSpeed = fireTurnSpeed;
    }

    bool targetInFieldOfFire()
    {
        Vector3 targetLocalPosition = transform.InverseTransformPoint(currentTarget.getPosition());

        // target must be forward and above of turbolaser position
        if (targetLocalPosition.y >= 0 && targetLocalPosition.z >= 0)
        {
            return true;
        }
        
        return false;
    }

    private void FixedUpdate()
    {
        if (currentTarget == null)
        {
            return;
        }
        if (!targetInFieldOfFire())
        {
            currentTarget = null;
            return;
        }
        Vector3 targetLocation = currentTarget.getPosition();
        Vector3 targetVelocity = currentTarget.getVelocity();
        Vector3 laserVelocity = parent.getVelocity() + 750f * transform.forward;

        float time_laser_travel = Vector3.Distance(targetLocation, transform.position) / laserVelocity.magnitude;
        Vector3 targetFutureLocation = targetLocation + targetVelocity * time_laser_travel;

        Vector3 vectorToTarget = targetFutureLocation - transform.position;

        muzzle.rotation = Quaternion.RotateTowards(muzzle.rotation, Quaternion.LookRotation(vectorToTarget), Time.fixedDeltaTime * turnSpeed);

        float targetDistance = Vector3.Distance(targetFutureLocation, transform.position);
        float targetAngle = Vector3.Angle(vectorToTarget, muzzle.forward);
        if (targetDistance < targetDistanceLimit && targetAngle < targetAngleOffsetLimit && fireRateTimestamp < Time.time) // fire
        {
            //blasterSound.PlayOneShot(blasterClip);
            GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle.position, muzzle.rotation) as GameObject;
            laser.GetComponent<ShotBehavior>().setVelocity(parent.getVelocity(), muzzle.forward, 10000, 25, false);
            fireRateTimestamp = Time.time + Random.Range(fireRateMin, fireRateMax);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        muzzle = transform.Find("TLMuzzle");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
