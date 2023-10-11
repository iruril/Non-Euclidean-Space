using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RenderPipeline = UnityEngine.Rendering.RenderPipelineManager;

public class PortalCamera : MonoBehaviour
{
    [SerializeField]
    private Portal _portalIn = null;

    [SerializeField]
    private Portal _portalOut = null;

    [SerializeField]
    private int _iterations = 7; //7 for windows environment, 2~3 is optimal

    private Camera _portalCamera;
    private RenderTexture _portalTextureIn;
    private RenderTexture _portalTextureOut;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        _portalCamera = GetComponent<Camera>();

        _portalTextureIn = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        _portalTextureOut = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
    }

    private void Start()
    {
        _portalIn.Renderer.material.SetTexture("_MainTexture", _portalTextureIn);
        _portalOut.Renderer.material.SetTexture("_MainTexture", _portalTextureOut);
    }

    private void OnEnable()
    {
        RenderPipeline.beginCameraRendering += UpdateCamera;
    }

    private void OnDisable()
    {
        RenderPipeline.beginCameraRendering -= UpdateCamera;
    }

    void UpdateCamera(ScriptableRenderContext SRC, Camera camera)
    {
        if (_portalIn.Renderer.isVisible)
        {
            _portalCamera.targetTexture = _portalTextureIn;
            for (int i = _iterations - 1; i >= 0; --i)
            {
                RenderCamera(_portalIn, _portalOut, i, SRC);
            }
        }

        if(_portalOut.Renderer.isVisible)
        {
            _portalCamera.targetTexture = _portalTextureOut;
            for (int i = _iterations - 1; i >= 0; --i)
            {
                RenderCamera(_portalOut, _portalIn, i, SRC);
            }
        }
    }

    private void RenderCamera(Portal inPortal, Portal outPortal, int iterationID, ScriptableRenderContext SRC)
    {
        Transform inTransform = inPortal.transform;
        Transform outTransform = outPortal.transform;

        Transform cameraTransform = _portalCamera.transform;
        cameraTransform.position = mainCamera.transform.position;
        cameraTransform.rotation = mainCamera.transform.rotation;

        for (int i = 0; i <= iterationID; ++i)
        {
            Vector3 relativePos = inTransform.InverseTransformPoint(cameraTransform.position);
            relativePos = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativePos;
            cameraTransform.position = outTransform.TransformPoint(relativePos);

            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * cameraTransform.rotation;
            relativeRot = Quaternion.Euler(0.0f, 180.0f, 0.0f) * relativeRot;
            cameraTransform.rotation = outTransform.rotation * relativeRot;
        }

        Plane p = new Plane(-outTransform.forward, outTransform.position);
        Vector4 clipPlaneWorldSpace = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(_portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

        Matrix4x4 newMatrix = mainCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        _portalCamera.projectionMatrix = newMatrix;

        UniversalRenderPipeline.RenderSingleCamera(SRC, _portalCamera);
    }
}
