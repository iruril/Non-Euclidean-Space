using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalableObject : MonoBehaviour
{
    [SerializeField] private Material _cloneMaterial = null;
    private Material[] _myMaterials = null;
    private Material[] _myCloneMaterials = null;
    private GameObject cloneObject;

    private int _inPortalCount = 0;
    
    private Portal _inPortal;
    private Portal _outPortal;

    private Rigidbody _myRigid;
    protected Collider _myCollider;

    private float _colRadius = 0;
    private float _portalColBoundSizeZ = 0;

    private static readonly Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    protected virtual void Awake()
    {
        cloneObject = new GameObject();
        cloneObject.SetActive(false);
        MeshFilter meshFilter = cloneObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cloneObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = GetComponent<MeshFilter>().mesh;
        _myMaterials = GetComponent<MeshRenderer>().materials;
        _myCloneMaterials = _myMaterials;
        for(int i = 0; i < _myMaterials.Length; i++)
        {
            _myCloneMaterials[i] = _cloneMaterial;
            _myCloneMaterials[i].SetTexture("_MainTexture", GetComponent<MeshRenderer>().materials[i].GetTexture("_MainTexture"));
        }
        meshRenderer.materials = _myCloneMaterials;
        _myCloneMaterials = cloneObject.GetComponent<MeshRenderer>().materials;
        cloneObject.transform.localScale = transform.localScale;

        _myRigid = GetComponent<Rigidbody>();
        _myCollider = GetComponent<Collider>();
        _colRadius = _myCollider.bounds.min.magnitude;
    }

    private void FixedUpdate()
    {
        if (_inPortal != null && _outPortal != null)
        {
            if (_inPortal.PortalPair.CheckThisTravellerEndedJourney(this.GetComponent<PortalableObject>()))
            {
                SetSliceValueInit();
            }
        }

        if (cloneObject.activeSelf)
        {
            SetMySliceValueOnIn();
            SetCloneSliceValueOnIn();
        }
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

    private void SetCloneSliceValueOnIn() //for Clone
    {
        _portalColBoundSizeZ = _outPortal.GetComponent<Collider>().bounds.size.z;

        Vector3 sliceNormal = _outPortal.transform.forward;
        Vector3 sliceCenter = _outPortal.transform.position;
        if (Physics.Raycast(cloneObject.transform.position, -sliceNormal,out RaycastHit hitInfo ,_colRadius + 1.0f, 1 << LayerMask.NameToLayer("Portal")))
        {
            float dist = Vector3.Distance(hitInfo.point, cloneObject.transform.position);
            float sliceOffset = (dist / _colRadius) + _portalColBoundSizeZ;
            for(int i =0;i< _myCloneMaterials.Length; i++)
            {
                _myCloneMaterials[i].SetVector("_SliceNormal", -sliceNormal);
                _myCloneMaterials[i].SetVector("_SliceCenter", sliceCenter);
                _myCloneMaterials[i].SetFloat("_SliceOffset", sliceOffset);
            }
        }
    }

    private void SetMySliceValueOnIn() //for Me
    {
        _portalColBoundSizeZ = _outPortal.GetComponent<Collider>().bounds.size.z;

        Vector3 sliceNormal = _inPortal.transform.forward;
        Vector3 sliceCenter = _inPortal.transform.position;
        if (Physics.Raycast(this.transform.position, sliceNormal, out RaycastHit hitInfo, _colRadius + 1.0f, 1 << LayerMask.NameToLayer("Portal")))
        {
            float dist = Vector3.Distance(hitInfo.point, this.transform.position);
            float sliceOffset = (dist / _colRadius) + _portalColBoundSizeZ;
            for (int i = 0; i < _myMaterials.Length; i++)
            {
                GetComponent<MeshRenderer>().materials[i].SetVector("_SliceNormal", -sliceNormal);
                GetComponent<MeshRenderer>().materials[i].SetVector("_SliceCenter", sliceCenter);
                GetComponent<MeshRenderer>().materials[i].SetFloat("_SliceOffset", sliceOffset);
            }
        }
    }

    public void SetSliceValueInit()
    {
        for (int i = 0; i < _myCloneMaterials.Length; i++)
        {
            _myCloneMaterials[i].SetVector("_SliceNormal", Vector3.zero);
            GetComponent<MeshRenderer>().materials[i].SetVector("_SliceNormal", Vector3.zero);
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

        SetCloneSliceValueOnIn();
    }

    int SideOfPortal(Vector3 myPos, Transform portalTranform)
    {
        return System.Math.Sign(Vector3.Dot((myPos - portalTranform.position).normalized, portalTranform.forward));
    }
}
