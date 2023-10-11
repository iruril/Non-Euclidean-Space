using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PortalableObject
{
    private CameraMove _cameraMove;

    protected override void Awake()
    {
        base.Awake();

        _cameraMove = GetComponent<CameraMove>();
    }

    public override void Warp()
    {
        base.Warp();
        _cameraMove.ResetTargetRotation();
    }
}
