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

    public AudioSource SfxAudioSource;
    public AudioSource drivingAudioSource;

    public AudioClip honkAudio;
    public AudioClip drivingLoop;

    public GrabbableObject itemHeld;

    public float speed = 0.4f;
    public float syncInterval = 0.5f;

    public bool playerIsDriving;
    public bool playerIsLocal;

    private PlayerControllerB playerDriving;

    private Camera playerCamera;
    
    private Ray interactRay;
    private RaycastHit hit;
    private Vector3 dropPos;

    private bool shouldBeDropPos;

    private bool carIsMoving;
    private bool lastFrameCarIsMoving;

    private float interactTimer;
    private float honkTimer;
    private float posSyncTimer;
    
    

    public void RegisterCar()
    {
        if (RCCarsPlugin.instance.RegistredCars.ContainsKey(NetworkObjectId))
        {
            RCCarsPlugin.instance.RegistredCars.Remove(NetworkObjectId);
        }

        RegistredCar registredCar = new RegistredCar();
        registredCar.networkObjectId = NetworkObjectId;
        registredCar.rcCarItem = this;
        
        RCCarsPlugin.instance.RegistredCars.Add(NetworkObjectId, registredCar);

    }


    public override void Start()
    {
        base.Start();
        EnableCamera(false);
        CarLights(false);
        navMeshAgent.speed = 30;
        navMeshAgent.enabled = false;
        RegisterCar();

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
        var controlsTips = new List<string>() { "[G] Leave RCCar", "[LMB] Honk"};

        if(itemHeld != null) controlsTips.Add($"[E] Drop {itemHeld.itemProperties.itemName}");
        HUDManager.Instance.ClearControlTips();
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
            honkTimer = 0;
            playerDriving = player;

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
            parentObject = null;
            
            transform.position = dropPos + Vector3.up * 3;
            targetFloorPosition = transform.position;
            startFallingPosition = transform.position;
            reachedFloorTarget = false;
            
            FallToGround();
            DropHeldItem();
            drivingAudioSource.Stop();
            playerIsLocal = false;
        }
        
    }

    public void OnStopUsingCar()
    {
        if(playerIsLocal) ChangePlayerControls(GameNetworkManager.Instance.localPlayerController, false);
        else ChangePlayerControls(playerDriving, false);
    }

    public void DropHeldItem()
    {
        if(itemHeld == null) return;
        itemHeld.reachedFloorTarget = false;
        itemHeld.transform.position = itemHeld.transform.position;
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

    public void HonkOnEveryClient()
    {
        RCCarNetwork.CarHonkServerRpc(NetworkObjectId);
    }
    public void Honk()
    {
        SfxAudioSource.clip = honkAudio;
        SfxAudioSource.Play();
        honkTimer = 0;
    }

    public void SyncPositionClient(Vector3 pos)
    {
        if (playerDriving && !playerIsLocal)
        {
            navMeshAgent.SetDestination(pos);
        }
    }

    public void StopDrivingSoundClient(bool turnOff)
    {
        if (turnOff)
        {
            drivingAudioSource.Stop();
        }
        else
        {
            drivingAudioSource.Play();
        }
    }

    public override void Update()
    {

        if (playerDriving && !playerIsLocal)
        {
            if (carIsMoving != lastFrameCarIsMoving)
            {
                if(!carIsMoving)
                {
                    drivingAudioSource.clip = drivingLoop;
                    drivingAudioSource.Play();
                }
                else
                {
                    drivingAudioSource.Stop();
                }
            }
        }
        
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
            honkTimer += Time.deltaTime;
            posSyncTimer += Time.deltaTime;
            
            float honk = IngamePlayerSettings.Instance.playerInput.actions.FindAction("ActivateItem", false).ReadValue<float>();
 
            if (honk > 0 && honkTimer >= 1)
            {
                HonkOnEveryClient();
            }
            
            Vector3 velocity = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();

            
            if (velocity.x != 0 || velocity.y != 0)
            {
                carIsMoving = true;
                if(!drivingAudioSource.isPlaying)
                {
                    drivingAudioSource.clip = drivingLoop;
                    drivingAudioSource.Play();
                }
                
                if(velocity.y > 0) navMeshAgent.Move(transform.forward * speed);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + velocity.x * 5, 0);
            }
            else
            {
                carIsMoving = false;
                drivingAudioSource.Stop();
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
            RCCarNetwork.CarDropItemServerRpc(NetworkObjectId);
            ChangeToolTips();
        }
        
        
        var drop = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Discard", false).ReadValue<float>();
            
        if (drop > 0)
        {
            RCCarNetwork.StopUseCarServerRpc(NetworkObjectId);
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
                RCCarNetwork.CarGrabItemServerRpc(NetworkObjectId, component.NetworkObjectId);
            }
        }

        if (posSyncTimer >= syncInterval)
        {
            posSyncTimer = 0;
            RCCarNetwork.SyncCarPositionServerRpc(NetworkObjectId, transform.position);
        }
        
    }

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false,
        int hitID = -1)
    {
        Debug.Log("HIT");
        return true;
    }
}