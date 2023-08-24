using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerMove : MonoBehaviour
{
    public delegate void ResetWorldAction(float xShift, float zShift);
    public static event ResetWorldAction onReset;
    public static float totalxShift;
    public static float totalzShift;

    // Start is called before the first frame update
    void Start()
    {
        // start position
        totalxShift = -1000;
        totalzShift = -1000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.MovePosition(rb.position + Time.fixedDeltaTime * new Vector3(1000f, 0, 1000f));

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), Vector2.zero) > 10000)
        {
            float xShift = -transform.position.x;
            float zShift = -transform.position.z;
            totalxShift += xShift;
            totalzShift += zShift;
            totalxShift = GenFunctions.mod(totalxShift, EndlessTerrain.worldDim);
            totalzShift = GenFunctions.mod(totalzShift, EndlessTerrain.worldDim);

            if (onReset != null)
            {
                onReset(xShift, zShift);
            }

            rb.MovePosition(new Vector3(0, rb.position.y, 0));
        }
    }
}
