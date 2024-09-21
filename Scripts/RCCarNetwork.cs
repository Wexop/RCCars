using StaticNetcodeLib;
using Unity.Netcode;
using UnityEngine;

namespace RCCars.Scripts;

[StaticNetcode]
public class RCCarNetwork
{

    public static RegistredCar GetRegistredCar(ulong networkId)
    {
        RegistredCar registredCar = null;
        
        if (RCCarsPlugin.instance.RegistredCars.ContainsKey(networkId))
        {
            registredCar = RCCarsPlugin.instance.RegistredCars[networkId];
        }
        
        if(registredCar == null) Debug.LogError($"COULD NOT FOUND CAR WITH ID {networkId}");

        return registredCar;
    }
    
    [ServerRpc]
    public static void CarHonkServerRpc(ulong networkId)
    {
        CarHonkClientRpc(networkId);

    }
    
    [ClientRpc]
    public static void CarHonkClientRpc(ulong networkId)
    {
        var car = GetRegistredCar(networkId);
        if(car == null) return;
        car.rcCarItem.Honk();
        
    }
    
    [ServerRpc]
    public static void StopUseCarServerRpc(ulong networkId)
    {
        StopUseCarClientRpc(networkId);
    }
    
    [ClientRpc]
    public static void StopUseCarClientRpc(ulong networkId)
    {
        var car = GetRegistredCar(networkId);
        if(car == null) return;
        
        car.rcCarItem.OnStopUsingCar();
        
    }
}