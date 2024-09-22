using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RCCars.Scripts;

public class RCPoliceCarItem : RCCarItem
{
    public List<Light> blueLights;

    private bool blueLightsAnimationRunning;

    private int actualBlueLightEnabled;
    private float blueLightsAnimationTimer;

    private float blueLightsInterval = 0.5f;

    public override void Start()
    {
        TurnOffBlueLights();
        base.Start();
    }

    public void SwitchBlueLights()
    {
        blueLights[0].enabled = actualBlueLightEnabled == 1;
        blueLights[1].enabled = actualBlueLightEnabled == 0;

        actualBlueLightEnabled = actualBlueLightEnabled == 0 ? 1 : 0;

    }

    public void TurnOffBlueLights()
    {
        blueLights.ForEach(l => l.enabled = false);
    }

    public IEnumerator RunBlueLightsAnimation()
    {
        yield return new WaitForSeconds(10f);
        blueLightsAnimationRunning = false;
        TurnOffBlueLights();
    }

    public override void Honk()
    {
        base.Honk();
        blueLightsAnimationRunning = true;
        StopCoroutine(RunBlueLightsAnimation());
        StartCoroutine(RunBlueLightsAnimation());
    }

    public override void Update()
    {
        if (blueLightsAnimationRunning)
        {
            blueLightsAnimationTimer += Time.deltaTime;
            if (blueLightsAnimationTimer >= blueLightsInterval)
            {
                SwitchBlueLights();
                blueLightsAnimationTimer = 0;
            }
        }
        base.Update();
    }
}