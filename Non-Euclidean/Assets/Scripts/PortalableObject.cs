using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalableObject : MonoBehaviour
{
    private GameObject cloneObject;

    private int _inPortalCount = 0;
    
    private Portal _inPortal;
    private Portal _outPortal;

    private Rigidbody _myRigid;
    protected Collider _myCollider;

    private static readonly Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    protected virtual void Awake()
    {
        cloneObject = new GameObject();
        cloneObject.SetActive(false);
        MeshFilter meshFilter = cloneObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cloneObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer.materials = GetComponent<MeshRenderer>().materials;
        cloneObject.transform.localScale = transform.localScale;

        _myRigid = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        if(_inPortal == null || _outPortal == null)
        {
            return;
        }

        if(cloneObject.activeSelf)
        {
            Transform inTransform = _inPortal.transform;
            Transform outTransform = _outPortal.transform;

            Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
            relativePos = halfTurn * relativePos;
            cloneObject.transform.position = outTransform.TransformPoint(relativePos);

            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
            relativeRot = halfTurn * relativeRot;
            cloneObject.transform.rotation = outTransform.rotation * relativeRot;
        }
        else
        {
            cloneObject.transform.position = new Vector3(-1000.0f, 1000.0f, -1000.0f);
        }
    }

    public void SetIsInPortal(Portal inPortal, Portal outPortal)
    {
        this._inPortal = inPortal;
        this._outPortal = outPortal;

        cloneObject.SetActive(true);

        ++_inPortalCount;
    }

    public void ExitPortal()
    {
        --_inPortalCount;

        if (_inPortalCount == 0)
        {
            cloneObject.SetActive(false);
        }
    }

    public virtual void Warp()
    {
        Transform inTransform = _inPortal.transform;
        Transform outTransform = _outPortal.transform;

        Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
        relativePos = halfTurn * relativePos;
        transform.position = outTransform.TransformPoint(relativePos);

        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
        relativeRot = halfTurn * relativeRot;
        transform.rotation = outTransform.rotation * relativeRot;

        Vector3 relativeVel = inTransform.InverseTransformDirection(_myRigid.velocity);
        relativeVel = halfTurn * relativeVel;
        _myRigid.velocity = outTransform.TransformDirection(relativeVel);

        Portal tmp = _inPortal;
        _inPortal = _outPortal;
        _outPortal = tmp;
    }
}
