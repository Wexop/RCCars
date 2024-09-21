using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.AI;

namespace RCCars.Scripts;

public class RCCarItem : PhysicsProp, IHittable
{
    public Rigidbody rigidbody;
    public NavMeshAgent navMeshAgent;

    public List<Light> carLights;
    public Color carLightsColor;
    public Camera carCamera;

    public Transform itemHeldPosition;

    public GrabbableObject itemHeld;

    public float speed = 0.4f;

    public bool playerIsDriving;
    public bool playerIsLocal;

    private Camera playerCamera;
    
    private Ray interactRay;
    private RaycastHit hit;
    private Vector3 dropPos;

    private bool shouldBeDropPos;

    private float interactTimer;


    public override void Start()
    {
        base.Start();
        EnableCamera(false);
        CarLights(false);
        navMeshAgent.enabled = false;

    }

    public void EnableCamera(bool enable)
    {
        carCamera.gameObject.SetActive(enable);
    }

    public void CarLights(bool on)
    {
        carLights.ForEach(l =>
        {
            l.color = on ? carLightsColor : Color.black;
        });
    }

    public void ChangeToolTips()
    {
        var controlsTips = new List<string>() { "[G] Leave RCCar"};

        if(itemHeld != null) controlsTips.Add($"[E] Drop {itemHeld.itemProperties.itemName}");

        HUDManager.Instance.ChangeControlTipMultiple(controlsTips.ToArray());
    }

    public void ChangePlayerControls(PlayerControllerB player, bool driving)
    {
        dropPos = transform.position;
        playerIsLocal = player.playerClientId == GameNetworkManager.Instance.localPlayerController.playerClientId;
        playerIsDriving = driving;
        grabbable = !driving;
        CarLights(driving);
        
        
        
        rigidbody.useGravity = driving;
        navMeshAgent.enabled = driving;

        if (driving)
        {

            if (playerIsLocal)
            {
                EnableCamera(true);
                player.DiscardHeldObject();
                playerCamera = player.gameplayCamera;
                player.gameplayCamera = carCamera;
                player.disableMoveInput = true;
                player.disableLookInput = true;
                ChangeToolTips();
            }
            targetFloorPosition = dropPos;
            startFallingPosition = dropPos;
            isInShipRoom = false;
            transform.position = dropPos;
            parentObject = null;
            shouldBeDropPos = true;

        }
        else
        {

            if (playerIsLocal)
            {
                EnableCamera(false);
                if(playerCamera != null) player.gameplayCamera = playerCamera;
                player.disableMoveInput = false;
                player.disableLookInput = false;
                HUDManager.Instance.ClearControlTips();
            }
            
            targetFloorPosition = transform.position;
            startFallingPosition = transform.position;
            reachedFloorTarget = false;
            transform.position = dropPos;
            FallToGround();
            DropHeldItem();
            
        }
        
    }

    public void DropHeldItem()
    {
        if(itemHeld == null) return;
        itemHeld.reachedFloorTarget = false;
        itemHeld.transform.position = transform.position;
        itemHeld.targetFloorPosition = itemHeld.transform.position;
        itemHeld.startFallingPosition = itemHeld.transform.position;
        itemHeld.FallToGround();
        itemHeld.grabbable = true;
        itemHeld = null;
        ChangeToolTips();

    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        
        if(!playerHeldBy) return;
        
        CarLights(true);
        ChangePlayerControls(playerHeldBy, true);
 
    }

    public void GrabItem(GrabbableObject item)
    {
        DropHeldItem();
        itemHeld = item;
        itemHeld.grabbable = false;
        ChangeToolTips();
    }

    public override void Update()
    {

        if (shouldBeDropPos)
        {
            transform.position = dropPos;
            shouldBeDropPos = false;
        }
        
        if (!playerIsDriving)
        {
            base.Update();
        }
        if(!playerIsDriving || !playerIsLocal) return;
        if (playerIsDriving )
        {

            interactTimer += Time.deltaTime;
            
            Vector3 velocity = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();
            if (velocity.x != 0 || velocity.y != 0)
            {
                
                if(velocity.y > 0) navMeshAgent.Move(transform.forward * speed);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + velocity.x * 5, 0);
            }

        }
    }

    public override void LateUpdate()
    {
        if (!playerIsDriving)
        {
            base.LateUpdate();
        }
        
        if (itemHeld != null)
        {
            reachedFloorTarget = false;
            itemHeld.targetFloorPosition = itemHeldPosition.position;
            itemHeld.startFallingPosition = itemHeldPosition.position;
            itemHeld.gameObject.transform.position = itemHeldPosition.position;
        }
        
        if(!playerIsDriving || !playerIsLocal) return;
        
        var interact = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact", false).ReadValue<float>();
        
        if (interact > 0 && itemHeld != null && interactTimer >= 1)
        {
            interactTimer = 0;
            DropHeldItem();
            ChangeToolTips();
        }
        
        
        var drop = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Discard", false).ReadValue<float>();
            
        if (drop > 0)
        {
            ChangePlayerControls(GameNetworkManager.Instance.localPlayerController, false);
        }

        interactRay = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(interactRay, out hit, 10, 1073742656) && hit.collider.gameObject.layer != 8 &&
            hit.collider.gameObject.layer != 30)
        {

            GrabbableObject component = hit.collider.gameObject.GetComponent<GrabbableObject>();
            GameNetworkManager.Instance.localPlayerController.cursorTip.text = "Grab : [E]";

            if (interact > 0)
            {
                interactTimer = 0;
                GrabItem(component);
            }
        }
    }

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false,
        int hitID = -1)
    {
        Debug.Log("HIT");
        return true;
    }
}