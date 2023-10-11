using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7.5f;
    [SerializeField] private float _cameraSpeed = 3.0f;

    public Quaternion TargetRotation { private set; get; }
    
    private Vector3 _moveVector = Vector3.zero;
    private float _moveY = 0.0f;

    private Rigidbody _myRigid;

    private void Awake()
    {
        _myRigid = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        TargetRotation = transform.rotation;
    }

    private void Update()
    {
        // Rotate the camera.
        Vector2 rotation = new Vector2(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        Vector3 targetEuler = TargetRotation.eulerAngles + (Vector3)rotation * _cameraSpeed;
        if(targetEuler.x > 180.0f)
        {
            targetEuler.x -= 360.0f;
        }
        targetEuler.x = Mathf.Clamp(targetEuler.x, -75.0f, 75.0f);
        TargetRotation = Quaternion.Euler(targetEuler);

        transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, 
            Time.deltaTime * 15.0f);

        // Move the camera.
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        _moveVector = new Vector3(x, 0.0f, z) * _moveSpeed;
    }

    private void FixedUpdate()
    {
        Vector3 newVelocity = transform.TransformDirection(_moveVector);
        newVelocity.y += _moveY * _moveSpeed;
        _myRigid.velocity = newVelocity;
    }

    public void ResetTargetRotation()
    {
        TargetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }
}
