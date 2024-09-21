using System.Linq;
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
    public static GrabbableObject GetItem(ulong networkId)
    {
        
        var items = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None).ToList();
        var itemFound = items.Find(e => e.NetworkObjectId == networkId);
        
        if(itemFound == null) Debug.LogError($"COULD NOT FOUND ITEM WITH ID {networkId}");

        return itemFound;
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
    public static void StopUseCarServerRpc(ulong networkId,Vector3 pos)
    {
        StopUseCarClientRpc(networkId, pos);
    }
    
    [ClientRpc]
    public static void StopUseCarClientRpc(ulong networkId, Vector3 pos)
    {
        var car = GetRegistredCar(networkId);
        if(car == null) return;
        
        car.rcCarItem.OnStopUsingCar(pos);
        
    }
    
    [ServerRpc]
    public static void UpdateDrivingSoundServerRpc(ulong networkId, bool value)
    {
        UpdateDrivingSoundClientRpc(networkId, value);
    }
    
    [ClientRpc]
    public static void UpdateDrivingSoundClientRpc(ulong networkId, bool value)
    {
        var car = GetRegistredCar(networkId);
        if(car == null) return;
        
        car.rcCarItem.StopDrivingSoundClient(value);
        
    }
    
    [ServerRpc]
    public static void CarGrabItemServerRpc(ulong networkId, ulong itemNetworkId)
    {
        CarGrabItemClientRpc(networkId, itemNetworkId);
    }
    
    [ClientRpc]
    public static void CarGrabItemClientRpc(ulong networkId, ulong itemNetworkId)
    {
        var car = GetRegistredCar(networkId);
        if(car == null) return;

        var item = GetItem(itemNetworkId);
        if(item == null) return;
        
        car.rcCarItem.GrabItem(item);
        
    }
    
    [ServerRpc]
    public static void CarDropItemServerRpc(ulong networkId)
    {
        CarDropItemClientRpc(networkId);
    }
    
    [ClientRpc]
    public static void CarDropItemClientRpc(ulong networkId)
    {
        var car = GetRegistredCar(networkId);
        if(car == null) return;
        
        car.rcCarItem.DropHeldItem();
        
    }
    
    [ServerRpc]
    public static void SyncCarPositionServerRpc(ulong networkId, Vector3 pos)
    {
        SyncCarPositionClientRpc(networkId, pos);
    }
    
    [ClientRpc]
    public static void SyncCarPositionClientRpc(ulong networkId, Vector3 pos)
    {
        var car = GetRegistredCar(networkId);
        if(car == null) return;
        
        car.rcCarItem.SyncPositionClient(pos);
        
    }
}