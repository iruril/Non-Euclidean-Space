using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [field:SerializeField]
    public Portal OtherPortal { get; private set; }

    private List<PortalableObject> _portalObjects = new();

    public Renderer Renderer { get; private set; }
    private void Awake()
    {
        Renderer = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();
    }

    private void Update()
    {
        for (int i = 0; i < _portalObjects.Count; ++i)
        {
            Vector3 objPos = transform.InverseTransformPoint(_portalObjects[i].transform.position);

            if (objPos.z > 0.0f)
            {
                _portalObjects[i].Warp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PortalableObject obj = other.GetComponent<PortalableObject>();
        if (obj != null)
        {
            _portalObjects.Add(obj);
            obj.SetIsInPortal(this, OtherPortal);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalableObject obj = other.GetComponent<PortalableObject>();

        if(_portalObjects.Contains(obj))
        {
            _portalObjects.Remove(obj);
            obj.ExitPortal();
        }
    }
}
