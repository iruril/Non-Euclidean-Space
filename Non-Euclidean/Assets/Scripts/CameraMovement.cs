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

    private Vector3 _playerVelocity = Vector3.zero;

    private CharacterController _myCharacter;
    [SerializeField] private GameObject _myBody;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _myCharacter = _myBody.GetComponent<CharacterController>();
    }

    void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        CameraLookAround();
        _myCharacter.Move(_playerVelocity * Time.deltaTime);
    }

    void FixedUpdate()
    {
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
        Vector3 moveVelocity = transform.TransformDirection(new Vector3(_horizontalInput, 0f, _verticalInput));
        moveVelocity = new Vector3(moveVelocity.x, 0, moveVelocity.z);
        Vector3 moveDirection = moveVelocity.normalized;

        float moveSpeed = Mathf.Min(moveVelocity.magnitude, 1.0f) * MyPlayerSpeed;
        Vector3 _playerMove = moveDirection * moveSpeed;
        float gravity = 0;
        if (!_myCharacter.isGrounded)
        {
            gravity = _playerVelocity.y - 9.8f * Time.fixedDeltaTime;
        }
        else
        {
            gravity = Mathf.Max(0.0f, _playerVelocity.y);
        }

        _playerVelocity = new Vector3(_playerMove.x, gravity, _playerMove.z);
    }
}
