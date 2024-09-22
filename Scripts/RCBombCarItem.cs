using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RCCars.Scripts;

public class RCBombCarItem : RCCarItem
{

    public override void Honk()
    {
        SetNewHealth(0);
    }
}