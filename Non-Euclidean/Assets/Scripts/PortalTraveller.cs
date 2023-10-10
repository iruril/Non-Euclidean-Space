using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{
    public GameObject GraphicsObject;
    public GameObject GraphicsClone { get; set; }
    public Vector3 PreviousOffsetFromPortal { get; set; }

    public Material[] OriginalMaterials { get; set; }
    public Material[] CloneMaterials { get; set; }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        Debug.Log(fromPortal.name + " To " + toPortal.name + ", Pos : " + pos + ", Rot : " + rot.eulerAngles);
        transform.position = pos;
        transform.rotation = rot;
    }

    public virtual void EnterPortalThreshold() //포탈에 접촉 시 호출. 잘려진 클론 생성을 위한 함수.
    {
        if (GraphicsClone == null)
        {
            GraphicsClone = Instantiate(GraphicsObject);
            GraphicsClone.transform.parent = GraphicsObject.transform.parent;
            GraphicsClone.transform.localScale = GraphicsObject.transform.localScale;
            OriginalMaterials = GetMaterials(GraphicsObject);
            CloneMaterials = GetMaterials(GraphicsClone);
        }
        else
        {
            GraphicsClone.SetActive(true);
        }
    }

    public virtual void ExitPortalThreshold() //포탈에 접촉 해제 시 호출. 이전에 생성된 잘려진 클론 삭제를 위한 함수.
    {
        GraphicsClone.SetActive(false);
        // 슬라이싱 비활성화
        for (int i = 0; i < OriginalMaterials.Length; i++)
        {
            OriginalMaterials[i].SetVector("sliceNormal", Vector3.zero);
        }
    }

    public void SetSliceOffsetDst(float dst, bool clone)
    {
        for (int i = 0; i < OriginalMaterials.Length; i++)
        {
            if (clone)
            {
                CloneMaterials[i].SetFloat("sliceOffsetDst", dst);
            }
            else
            {
                OriginalMaterials[i].SetFloat("sliceOffsetDst", dst);
            }
        }
    }

    Material[] GetMaterials(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<MeshRenderer>();
        var matList = new List<Material>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                matList.Add(mat);
            }
        }
        return matList.ToArray();
    }
}
