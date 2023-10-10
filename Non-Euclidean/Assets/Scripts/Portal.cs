using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [field: SerializeField]
    public Portal OtherPortal { get; private set; }

    [SerializeField]
    //private Renderer outlineRenderer;

    [field: SerializeField]
    public Color PortalColour { get; private set; }

    [SerializeField]
    private LayerMask placementMask;

    [SerializeField]
    private Transform testTransform;

    private List<PortalableObject> portalObjects = new List<PortalableObject>();
    public bool IsPlaced { get; private set; } = true;
    private Collider wallCollider;

    // Components.
    public Renderer Renderer { get; private set; }
    private new BoxCollider collider;

    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        Renderer = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();
        testTransform = this.transform;
    }

    private void Start()
    {
        //outlineRenderer.material.SetColor("_OutlineColour", PortalColour);
        
        //gameObject.SetActive(false);
    }

    private void Update()
    {
        Renderer.enabled = OtherPortal.IsPlaced;

        for (int i = 0; i < portalObjects.Count; ++i)
        {
            Vector3 objPos = transform.InverseTransformPoint(portalObjects[i].transform.position);

            if (objPos.z > 0.0f)
            {
                portalObjects[i].Warp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PortalableObject obj = other.GetComponent<PortalableObject>();
        if (obj != null)
        {
            portalObjects.Add(obj);
            obj.SetIsInPortal(this, OtherPortal, wallCollider);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalableObject obj = other.GetComponent<PortalableObject>();

        if(portalObjects.Contains(obj))
        {
            portalObjects.Remove(obj);
            obj.ExitPortal(wallCollider);
        }
    }

    public bool PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        testTransform.position = pos;
        testTransform.rotation = rot;
        testTransform.position -= testTransform.forward * 0.001f;

        FixOverhangs();
        FixIntersects();

        if (CheckOverlap())
        {
            this.wallCollider = wallCollider;
            transform.position = testTransform.position;
            transform.rotation = testTransform.rotation;

            gameObject.SetActive(true);
            IsPlaced = true;
            return true;
        }

        return false;
    }

    // Ensure the portal cannot extend past the edge of a surface.
    private void FixOverhangs()
    {
        List<Vector3> testPoints = new List<Vector3>
        {
            new Vector3(-1.1f,  0.0f, 0.1f),
            new Vector3( 1.1f,  0.0f, 0.1f),
            new Vector3( 0.0f, -2.1f, 0.1f),
            new Vector3( 0.0f,  2.1f, 0.1f)
        };

        List<Vector3> testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        for(int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = testTransform.TransformPoint(testPoints[i]);
            Vector3 raycastDir = testTransform.TransformDirection(testDirs[i]);

            if(Physics.CheckSphere(raycastPos, 0.05f, placementMask))
            {
                break;
            }
            else if(Physics.Raycast(raycastPos, raycastDir, out hit, 2.1f, placementMask))
            {
                Vector3 offset = hit.point - raycastPos;
                testTransform.Translate(offset, Space.World);
            }
        }
    }

    // Ensure the portal cannot intersect a section of wall.
    private void FixIntersects()
    {
        List<Vector3> testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        List<float> testDists = new List<float> { 1.1f, 1.1f, 2.1f, 2.1f };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = testTransform.TransformPoint(0.0f, 0.0f, -0.1f);
            Vector3 raycastDir = testTransform.TransformDirection(testDirs[i]);

            if (Physics.Raycast(raycastPos, raycastDir, out hit, testDists[i], placementMask))
            {
                Vector3 offset = (hit.point - raycastPos);
                Vector3 newOffset = -raycastDir * (testDists[i] - offset.magnitude);
                testTransform.Translate(newOffset, Space.World);
            }
        }
    }

    // Once positioning has taken place, ensure the portal isn't intersecting anything.
    private bool CheckOverlap()
    {
        Vector3 checkExtents = new Vector3(0.9f, 1.9f, 0.05f);

        Vector3[] checkPositions = new Vector3[]
        {
            testTransform.position + testTransform.TransformVector(new Vector3( 0.0f,  0.0f, -0.1f)),

            testTransform.position + testTransform.TransformVector(new Vector3(-1.0f, -2.0f, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3(-1.0f,  2.0f, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3( 1.0f, -2.0f, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3( 1.0f,  2.0f, -0.1f)),

            testTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.2f))
        };

        Collider[] intersections = Physics.OverlapBox(checkPositions[0], checkExtents, testTransform.rotation, placementMask);

        if(intersections.Length > 1)
        {
            return false;
        }
        else if(intersections.Length == 1) 
        {
            if (intersections[0] != collider)
            {
                return false;
            }
        }

        bool isOverlapping = true;

        for(int i = 1; i < checkPositions.Length - 1; ++i)
        {
            isOverlapping &= Physics.Linecast(checkPositions[i], 
                checkPositions[i] + checkPositions[checkPositions.Length - 1], placementMask);
        }

        return isOverlapping;
    }

    public void RemovePortal()
    {
        gameObject.SetActive(false);
        IsPlaced = false;
    }
}
