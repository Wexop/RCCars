using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace RCCars.Scripts;

public class RCCarItem : PhysicsProp, IHittable
{
    public Rigidbody rigidbody;
    public NavMeshAgent navMeshAgent;
    public TextMeshProUGUI playerText;

    public List<Light> carLights;
    public Light carVisionLight;
    public Color carLightsColor;
    public Camera carCamera;

    public Transform itemHeldPosition;

    public AudioSource SfxAudioSource;
    public AudioSource drivingAudioSource;

    public GameObject carBody;

    public AudioClip honkAudio;
    public AudioClip drivingLoop;

    public GrabbableObject itemHeld;

    public float speed = 0.4f;
    public float syncInterval = 0.5f;

    public bool playerIsDriving;
    public bool playerIsLocal;

    public float honkInterval = 1;
    public float rotationSpeed = 10;

    public int MaxHealth = 2;
    public int Health = 2;
    public ParticleSystem smokeParticules;
    public ParticleSystem explosion;
    public AudioClip explosionAudio;
    public float explosionRange;

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
    private float hitTimer;
    
    

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

    public void RefreshPluginValues()
    {
        drivingAudioSource.volume = RCCarsPlugin.instance.engineVolume.Value;
        SfxAudioSource.volume = RCCarsPlugin.instance.honkVolume.Value;
        rotationSpeed = RCCarsPlugin.instance.rotationSpeed.Value;
        syncInterval = RCCarsPlugin.instance.syncInterval.Value;
    }

    public override void Start()
    {
        base.Start();
        EnableCamera(false);
        CarLights(false);
        navMeshAgent.speed = 50;
        navMeshAgent.enabled = false;
        RegisterCar();
        RefreshPluginValues();
        playerText.text = "";

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
        carVisionLight.enabled = on;
    }

    public void ChangeToolTips()
    {
        var controlsTips = new List<string>() { "[G] Leave RCCar", "[LMB] Honk"};

        if(itemHeld != null) controlsTips.Add($"[E] Drop {itemHeld.itemProperties.itemName}");
        HUDManager.Instance.ClearControlTips();
        HUDManager.Instance.ChangeControlTipMultiple(controlsTips.ToArray());
    }

    public IEnumerator SetPlayerBack(PlayerControllerB player)
    {
        yield return new WaitForSeconds(Health <= 0 ? 0.8f : 0f);
        EnableCamera(false);
        if(playerCamera != null) player.gameplayCamera = playerCamera;
        player.disableMoveInput = false;
        player.disableLookInput = false;
        player.disableInteract = false;
        HUDManager.Instance.ClearControlTips();
    }

    public void ChangePlayerControls(PlayerControllerB player, bool driving)
    {
        
        RefreshPluginValues();
        playerIsLocal = player.playerClientId == GameNetworkManager.Instance.localPlayerController.playerClientId;
        
        if (player.isInHangarShipRoom && driving)
        {
            if(playerIsLocal) HUDManager.Instance.DisplayTip("Warning", "You can't drive in the ship !");
            return;
        }
        
        if (RoundManager.Instance.currentLevel.PlanetName.Contains("Gordion") && driving)
        {
            if(playerIsLocal) HUDManager.Instance.DisplayTip("Warning", "You can't drive in company building !");
            return;
        }
        
        dropPos = transform.localPosition;
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
                player.disableInteract = true;
                ChangeToolTips();
            }
            else
            {
                playerText.text = player.playerUsername;
            }
            targetFloorPosition = GetItemFloorPosition(transform.localPosition);;
            parentObject = null;
            honkTimer = honkInterval - 1f;
            playerDriving = player;

        }
        else
        {
            if (playerIsLocal)
            {
                StartCoroutine(SetPlayerBack(player));
            }
            parentObject = null;
            playerText.text = "";
            reachedFloorTarget = false;
            transform.localPosition = dropPos;
            startFallingPosition = dropPos;
            FallToGround();
            grabbable = true;
            

            DropHeldItem();
            drivingAudioSource.Stop();
            playerIsLocal = false;
        }
        
    }

    public void OnStopUsingCar(Vector3 pos)
    {
        if(!playerDriving) return;
        transform.position = pos;
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
    public virtual void Honk()
    {
        RefreshPluginValues();
        SfxAudioSource.clip = honkAudio;
        SfxAudioSource.Play();
        honkTimer = 0;
    }

    public void SyncPositionClient(Vector3 pos)
    {
        if (playerDriving && !playerIsLocal && navMeshAgent.enabled)
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
        hitTimer += Time.deltaTime;
        if (playerDriving && !playerIsLocal)
        {
            carIsMoving = !navMeshAgent.velocity.Equals(Vector3.zero);
            if(carIsMoving != lastFrameCarIsMoving)
            {
                lastFrameCarIsMoving = carIsMoving;
                if(!carIsMoving)
                {
                    drivingAudioSource.Stop();
                }
                else
                {
                    drivingAudioSource.clip = drivingLoop;
                    drivingAudioSource.Play();
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
        if (playerDriving.isPlayerDead)
        {
            RCCarNetwork.StopUseCarServerRpc(NetworkObjectId, transform.position);
        }
        if (playerIsDriving )
        {

            interactTimer += Time.deltaTime;
            honkTimer += Time.deltaTime;
            posSyncTimer += Time.deltaTime;
            
            float honk = IngamePlayerSettings.Instance.playerInput.actions.FindAction("ActivateItem", false).ReadValue<float>();
 
            if (honk > 0 && honkTimer >= honkInterval)
            {
                HonkOnEveryClient();
            }
            
            Vector3 velocity = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();

            
            if (velocity.x != 0 || velocity.y != 0)
            {
                if(!drivingAudioSource.isPlaying)
                {
                    drivingAudioSource.clip = drivingLoop;
                    drivingAudioSource.Play();
                }
                
                if(velocity.y > 0) navMeshAgent.Move(transform.forward * speed * Time.deltaTime);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + velocity.x * rotationSpeed, 0);
            }
            else
            {
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
            itemHeld.gameObject.transform.position = itemHeldPosition.position + Vector3.up * itemHeld.itemProperties.floorYOffset;
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
            RCCarNetwork.StopUseCarServerRpc(NetworkObjectId, transform.position);
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

    public void SetNewHealth(int health)
    {
        Health = health;
        if (Health < MaxHealth)
        {
            if(!smokeParticules.isPlaying) smokeParticules.Play();
        }

        if (Health <= 0)
        {
            explosion.Play();
            SfxAudioSource.PlayOneShot(explosionAudio);
            RCCarNetwork.StopUseCarServerRpc(NetworkObjectId, transform.position);
            if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position,
                    transform.position) <= explosionRange)
            {
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(RCCarsPlugin.instance.explosionDamage.Value);
            }

            List<EnemyAI> enemiesClose = FindObjectsOfType<EnemyAI>().ToList();
            enemiesClose.ForEach(enemy =>
            {
                if (Vector3.Distance(enemy.transform.position,
                        transform.position) <= explosionRange)
                {
                    enemy.HitEnemy(3);
                }
            });
            
            
            StartCoroutine(DestroyObject());
        }
    }

    public IEnumerator DestroyObject()
    {
        carBody.SetActive(false);
        yield return new WaitForSeconds(1f);
        RCCarsPlugin.instance.RegistredCars.Remove(NetworkObjectId);
        if(IsServer) Destroy(gameObject);
    }

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false,
        int hitID = -1)
    {
        if (hitTimer >= 0.2f)
        {
            hitTimer = 0;
            RCCarNetwork.SetCarHealthServerRpc(NetworkObjectId, Health - force);
        }
        return true;
    }
}