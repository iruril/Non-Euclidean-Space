using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalControl : MonoBehaviour
{
    [Header("메인 설정")]
    public PortalControl LinkedPairPortal = null;
    public MeshRenderer PortalScreen = null;
    public int RecursionDrawLimit = 5;

    [Header("세부 설정")]
    public float CameraNearClipOffset = 0.05f;
    public float CameraNearClipLimit = 0.2f;

    private RenderTexture _viewTexture;
    private Camera _portalCam;
    private Camera _playerCam;
    private List<PortalTraveller> _trackedTravellers;
    private MeshFilter _portalScreenMeshFilter;
    private Vector3 portalCamPos
    {
        get
        {
            return _portalCam.transform.position;
        }
    }

    private void Awake()
    {
        _playerCam = Camera.main;
        _portalCam = this.GetComponentInChildren<Camera>();
        _portalCam.enabled = false;
        _trackedTravellers = new List<PortalTraveller>();
        _portalScreenMeshFilter = PortalScreen.GetComponent<MeshFilter>();
        PortalScreen.material.SetInt("displayMask", 1);
    }

    private void LateUpdate()
    {
        HandleTravellers();
    }

    private void HandleTravellers()
    {
        for (int i = 0; i < _trackedTravellers.Count; i++)
        {
            PortalTraveller traveller = _trackedTravellers[i];
            Transform travellerT = traveller.transform;
            Matrix4x4 m = LinkedPairPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.PreviousOffsetFromPortal, transform.forward));

            if (portalSide != portalSideOld) //이동한다
            {
                Vector3 positionOld = travellerT.position;
                var rotOld = travellerT.rotation;
                traveller.Teleport(transform, LinkedPairPortal.transform, m.GetColumn(3), m.rotation);
                traveller.GraphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);

                // OnTriggerEnter/Exit 에 의존할 수 없다. FixedUpdate 호출 주기에 맞춰지기 때문에, 매 프레임의 연산에 맞지 않다.
                LinkedPairPortal.OnTravellerEnterPortal(traveller);
                _trackedTravellers.RemoveAt(i);
                i--;

            }
            else
            {
                traveller.GraphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                traveller.PreviousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    public void PrePortalRender()
    {
        foreach (var traveller in _trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
    }

    public void Render()
    {
        if(!CameraUtility.VisibleFromCamera(LinkedPairPortal.PortalScreen, _playerCam))
        {
            return;
        }

        CreateViewTexture();

        Matrix4x4 localToWorldMatrix = _playerCam.transform.localToWorldMatrix;
        Vector3[] renderPosition = new Vector3[RecursionDrawLimit];
        Quaternion[] renderRotation = new Quaternion[RecursionDrawLimit];

        int startIndex = 0;
        for (int i = 0; i < RecursionDrawLimit; i++)
        {
            if(i > 0)
            {
                if (!CameraUtility.BoundsOverlap(_portalScreenMeshFilter, LinkedPairPortal._portalScreenMeshFilter, _portalCam))
                {
                    break; //만약 카메라의 시야 안에 포탈이 있지 않으면 그려줄 필요가 없다.
                }
            }

            localToWorldMatrix = transform.localToWorldMatrix * LinkedPairPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = RecursionDrawLimit - i - 1;
            renderPosition[renderOrderIndex] = localToWorldMatrix.GetColumn(3); // Z - Axis Value on 4x4 Matrix
            renderRotation[renderOrderIndex] = localToWorldMatrix.rotation;

            _portalCam.transform.SetPositionAndRotation(renderPosition[renderOrderIndex], renderRotation[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        PortalScreen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        LinkedPairPortal.PortalScreen.material.SetInt("displayMask", 0);

        for (int i = startIndex; i < RecursionDrawLimit; i++)
        {
            _portalCam.transform.SetPositionAndRotation(renderPosition[i], renderRotation[i]);
            //Clipping Fields
            SetNearClipPlane();
            TravellerSlicingClipping();

            _portalCam.Render();

            if(i == startIndex)
            {
                LinkedPairPortal.PortalScreen.material.SetInt("displayMask", 1);
            }
        }

        PortalScreen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    private void CreateViewTexture()
    {
        if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height)
        {
            if (_viewTexture != null)
            {
                _viewTexture.Release();
            }
            _viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
            _portalCam.targetTexture = _viewTexture;
            LinkedPairPortal.PortalScreen.material.SetTexture("_MainTex", _viewTexture);
            Debug.Log("Created RenderTexture 3");
        }
    }

    public void PostPortalRender()
    {
        foreach (var traveller in _trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
        ProtectScreenFromClipping(_playerCam.transform.position);
    }

    void UpdateSliceParams(PortalTraveller traveller)
    {
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = LinkedPairPortal.transform.forward * side;

        // Calculate slice centre
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = LinkedPairPortal.transform.position;

        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = PortalScreen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal(_playerCam.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller)
        {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != LinkedPairPortal.SideOfPortal(_playerCam.transform.position);
        if (!playerSameSideAsCloneAppearing)
        {
            cloneSliceOffsetDst = -screenThickness;
        }

        // Apply parameters
        for (int i = 0; i < traveller.OriginalMaterials.Length; i++)
        {
            traveller.OriginalMaterials[i].SetVector("sliceCentre", slicePos);
            traveller.OriginalMaterials[i].SetVector("sliceNormal", sliceNormal);
            traveller.OriginalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            traveller.CloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            traveller.CloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            traveller.CloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);
        }
    }

    #region Clip-Logic Fields
    private void SetNearClipPlane()
    {
        Transform clipPlane = this.transform;
        int dotProduct = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - _portalCam.transform.position));

        Vector3 camSpacePosition = _portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dotProduct;
        float camSpaceDistance = -Vector3.Dot(camSpacePosition, camSpaceNormal) + CameraNearClipOffset;

        if(Mathf.Abs(camSpaceDistance) > CameraNearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDistance);
            _portalCam.projectionMatrix = _playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            _portalCam.projectionMatrix = _playerCam.projectionMatrix;
        }
    }

    private float ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = _playerCam.nearClipPlane * Mathf.Tan(_playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * _playerCam.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, _playerCam.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = PortalScreen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        return screenThickness;
    }

    private void TravellerSlicingClipping()
    {
        const float hideDistance = -1000;
        const float showDistance = 1000;
        float screenThickness = LinkedPairPortal.ProtectScreenFromClipping(_portalCam.transform.position);

        foreach (PortalTraveller traveller in _trackedTravellers)
        {
            if (SameSideOfPortal(traveller.transform.position, portalCamPos))
            {
                traveller.SetSliceOffsetDst(hideDistance, false);
            }
            else
            {
                traveller.SetSliceOffsetDst(showDistance, false);
            }

            int cloneSideOfLinkedPortal = -SideOfPortal(traveller.transform.position);
            if (LinkedPairPortal.SideOfPortal(portalCamPos) == cloneSideOfLinkedPortal)
            {
                traveller.SetSliceOffsetDst(screenThickness, true);
            }
            else
            {
                traveller.SetSliceOffsetDst(-screenThickness, true);
            }
        }

        Vector3 offsetFromPortalToCam = portalCamPos - this.transform.position;
        foreach(PortalTraveller linkedPairTraveller in LinkedPairPortal._trackedTravellers)
        {
            Vector3 travellerPos = linkedPairTraveller.GraphicsObject.transform.position;
            Vector3 clonePos = linkedPairTraveller.GraphicsClone.transform.position;

            if(LinkedPairPortal.SideOfPortal(travellerPos) != SideOfPortal(portalCamPos))
            {
                linkedPairTraveller.SetSliceOffsetDst(hideDistance, true);
            }
            else
            {
                linkedPairTraveller.SetSliceOffsetDst(showDistance, true);
            }

            if(LinkedPairPortal.SameSideOfPortal(linkedPairTraveller.transform.position, portalCamPos))
            {
                linkedPairTraveller.SetSliceOffsetDst(screenThickness, false);
            }
            else
            {
                linkedPairTraveller.SetSliceOffsetDst(-screenThickness, false);
            }
        }
    }
    #endregion

    #region Portal Trigger Action Fields
    private void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!_trackedTravellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();
            traveller.PreviousOffsetFromPortal = traveller.transform.position - transform.position;
            _trackedTravellers.Add(traveller);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller)
        {
            OnTravellerEnterPortal(traveller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller && _trackedTravellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold();
            _trackedTravellers.Remove(traveller);
        }
    }
    #endregion

    #region Get Value Function Fields
    private int SideOfPortal(Vector3 pos)
    {
        return System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
    }

    private bool SameSideOfPortal(Vector3 posA, Vector3 posB)
    {
        return SideOfPortal(posA) == SideOfPortal(posB);
    }
    #endregion
}
