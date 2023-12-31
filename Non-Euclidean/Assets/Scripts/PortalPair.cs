﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPair : MonoBehaviour
{
    public Portal[] Portals { private set; get; }

    private void Awake()
    {
        Portals = GetComponentsInChildren<Portal>();

        if(Portals.Length != 2)
        {
            Debug.LogError("포탈의 짝이 맞지 않습니다.");
        }
    }

    public bool CheckThisTravellerEndedJourney(PortalableObject Travellar)
    {
        foreach(Portal portal in Portals)
        {
            if (portal.IsContainThisTraveller(Travellar)) return false;
        }
        return true;
    }
}
