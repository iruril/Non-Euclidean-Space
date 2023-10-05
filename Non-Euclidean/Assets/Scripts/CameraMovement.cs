using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private float _xMove = 0;
    private float _yMove = 0;

    private float _horizontalInput = 0;
    private float _verticalInput = 0;

    public float MyPlayerSpeed = 10.0f;
    public float MouseSensativityX = 1.0f;
    public float MouseSensativityY = 1.0f;

    private Rigidbody _myRigid;
    [SerializeField] private GameObject _myBody;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _myRigid = _myBody.GetComponent<Rigidbody>();
    }

    void Update()
    {
        CameraLookAround();
        CameraMoveAround();
    }

    private void CameraLookAround()
    {
        _xMove += Input.GetAxis("Mouse X") * MouseSensativityX;
        _yMove -= Input.GetAxis("Mouse Y") * MouseSensativityY;

        _yMove = Mathf.Clamp(_yMove, -55f, 55f);
        _myBody.transform.localRotation = Quaternion.Euler(0, _xMove, 0);
        this.transform.rotation = Quaternion.Euler(_yMove, _xMove, 0);
    }

    private void CameraMoveAround()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.TransformDirection(new Vector3(_horizontalInput, 0f, _verticalInput));
        moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
        moveDirection.Normalize();

        Vector3 moveOffset = moveDirection * (MyPlayerSpeed * Time.fixedDeltaTime);
        _myRigid.MovePosition(_myRigid.position + moveOffset);
    }
}
