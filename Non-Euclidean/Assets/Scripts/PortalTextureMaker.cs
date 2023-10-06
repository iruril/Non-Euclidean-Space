using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureMaker : MonoBehaviour
{
    public List<Camera> Cameras = new();
    public List<Material> Materials = new();

    void Start()
    {
        for (int i = 0; i < Cameras.Count; i++)
        {
            if (Cameras[i].targetTexture != null)
            {
                Cameras[i].targetTexture.Release();
            }
            Cameras[i].targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            Materials[i].mainTexture = Cameras[i].targetTexture;
        }
    }
}
