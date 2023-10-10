using UnityEngine;

public class MainCamRender : MonoBehaviour
{
    private PortalControl[] _portals;

    private void Awake()
    {
        _portals = FindObjectsOfType<PortalControl>();
    }

    private void Update()
    {
        for (int i = 0; i < _portals.Length; i++)
        {
            _portals[i].PrePortalRender();
        }
        for (int i = 0; i < _portals.Length; i++)
        {
            _portals[i].Render();
        }

        for (int i = 0; i < _portals.Length; i++)
        {
            _portals[i].PostPortalRender();
        }
    }
}
