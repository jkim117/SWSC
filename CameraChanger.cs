using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraChanger : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera rearCam;

    void OnLookRear(InputValue value)
    {
        CrossSceneValues.camStatus = value.Get<float>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CrossSceneValues.camStatus == 0.0f)
        {
            rearCam.Priority = 0;
        }
        else
        {
            rearCam.Priority = 15;
        }
    }
}
