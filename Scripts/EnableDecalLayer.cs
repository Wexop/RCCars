using System;
using UnityEngine;

namespace RCCars.Scripts;

public class EnableDecalLayer : MonoBehaviour
{
    private void Start()
    {
        var component = GetComponent<MeshRenderer>();
        component.renderingLayerMask = 1 << 0 | 1 << 8;
    }
}