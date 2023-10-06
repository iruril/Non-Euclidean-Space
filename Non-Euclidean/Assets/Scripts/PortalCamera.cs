using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    [SerializeField] private Transform _playerCamera;
    private Transform _myPortal;
    [SerializeField] private Transform _anotherPortal;

    private void Start()
    {
        _myPortal = this.transform.parent.gameObject.transform;
    }

    private void LateUpdate()
    {
        Vector3 playerOffsetFromPrtal = _playerCamera.position - _anotherPortal.position;

        transform.position = _myPortal.position + playerOffsetFromPrtal;

        float angularDiff = Quaternion.Angle(_myPortal.rotation, _anotherPortal.rotation);
        Quaternion portalRotDiff = Quaternion.AngleAxis(angularDiff, Vector3.up);
        Vector3 newCamDir = portalRotDiff * _playerCamera.forward;

        transform.rotation = Quaternion.LookRotation(newCamDir, Vector3.up);
    }
}
