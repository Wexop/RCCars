using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace RCCars.Scripts;

public class RCCarNetwork
{
    
    
    public static GrabbableObject GetItem(ulong networkId)
    {
        
        var items = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None).ToList();
        var itemFound = items.Find(e => e.NetworkObjectId == networkId);
        
        if(itemFound == null) Debug.LogError($"COULD NOT FOUND ITEM WITH ID {networkId}");

        return itemFound;
    }
    

    


}