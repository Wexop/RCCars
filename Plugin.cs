using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalLib.Modules;
using RCCars.Scripts;
using UnityEngine;

namespace RCCars
{
    [BepInDependency(StaticNetcodeLib.StaticNetcodeLib.Guid)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class RCCarsPlugin : BaseUnityPlugin
    {
        private const string GUID = "wexop.rc_cars";
        private const string NAME = "RCCars";
        private const string VERSION = "1.0.4";

        public static RCCarsPlugin instance;

        public Dictionary<ulong, RegistredCar> RegistredCars = new Dictionary<ulong, RegistredCar>();

        public ConfigEntry<float> honkVolume;
        public ConfigEntry<float> engineVolume;
        public ConfigEntry<float> rotationSpeed;
        public ConfigEntry<int> explosionDamage;
        public ConfigEntry<float> syncInterval;
        public ConfigEntry<string> blacklistCar;
        
        public ConfigEntry<int> carPrice;
        public ConfigEntry<int> policeCarPrice;
        public ConfigEntry<int> ambulanceCarPrice;
        public ConfigEntry<int> sportCarPrice;
        public ConfigEntry<int> bombCarPrice;
        public ConfigEntry<int> cruiserCarPrice;
        public ConfigEntry<int> wexopCarPrice;

        private void Awake()
        {
            instance = this;

            Logger.LogInfo("RCCars starting....");
            
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "rccars");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            LoadConfigs();
            RegisterCar(bundle);


            Logger.LogInfo("RCCars ready !!");
        }

        void RegisterCar(AssetBundle bundle)
        {
            //NormalCar
            Item carAsset =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCar.asset");
            Logger.LogInfo($"{carAsset.name} FOUND");
            Logger.LogInfo($"{carAsset.spawnPrefab} prefab");
            if (!CarIsInBlacklist(carAsset.itemName))
            {
                NetworkPrefabs.RegisterNetworkPrefab(carAsset.spawnPrefab);
                Utilities.FixMixerGroups(carAsset.spawnPrefab);
                Items.RegisterItem(carAsset);
                Items.RegisterShopItem(carAsset, price: instance.carPrice.Value);
            }
            
            //PoliceCar
            Item policeCarAsset =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCarPolice.asset");
            Logger.LogInfo($"{policeCarAsset.name} FOUND");
            Logger.LogInfo($"{policeCarAsset.spawnPrefab} prefab");
            if (!CarIsInBlacklist(policeCarAsset.itemName))
            {
                NetworkPrefabs.RegisterNetworkPrefab(policeCarAsset.spawnPrefab);
                Utilities.FixMixerGroups(policeCarAsset.spawnPrefab);
                Items.RegisterItem(policeCarAsset);
                Items.RegisterShopItem(policeCarAsset, price: instance.policeCarPrice.Value);
            }

            //AmbulanceCar
            Item ambulanceCarAsset =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCarAmbulance.asset");
            Logger.LogInfo($"{ambulanceCarAsset.name} FOUND");
            Logger.LogInfo($"{ambulanceCarAsset.spawnPrefab} prefab");
            if (!CarIsInBlacklist(ambulanceCarAsset.itemName))
            {
                NetworkPrefabs.RegisterNetworkPrefab(ambulanceCarAsset.spawnPrefab);
                Utilities.FixMixerGroups(ambulanceCarAsset.spawnPrefab);
                Items.RegisterItem(ambulanceCarAsset);
                Items.RegisterShopItem(ambulanceCarAsset, price: instance.ambulanceCarPrice.Value);
            }
            
            //SportCar
            Item sportCar =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCSportCar.asset");
            Logger.LogInfo($"{sportCar.name} FOUND");
            Logger.LogInfo($"{sportCar.spawnPrefab} prefab");
            if (!CarIsInBlacklist(sportCar.itemName))
            {
                NetworkPrefabs.RegisterNetworkPrefab(sportCar.spawnPrefab);
                Utilities.FixMixerGroups(sportCar.spawnPrefab);
                Items.RegisterItem(sportCar);
                Items.RegisterShopItem(sportCar, price: instance.sportCarPrice.Value);
            }
            
            //BombCar
            Item bombCar =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCarBomb.asset");
            Logger.LogInfo($"{bombCar.name} FOUND");
            Logger.LogInfo($"{bombCar.spawnPrefab} prefab");
            if (!CarIsInBlacklist(bombCar.itemName))
            {
                NetworkPrefabs.RegisterNetworkPrefab(bombCar.spawnPrefab);
                Utilities.FixMixerGroups(bombCar.spawnPrefab);
                Items.RegisterItem(bombCar);
                Items.RegisterShopItem(bombCar, price: instance.bombCarPrice.Value);
            }
            
            //CruiserCar
            Item cruiserCar =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCruiserCar.asset");
            Logger.LogInfo($"{cruiserCar.name} FOUND");
            Logger.LogInfo($"{cruiserCar.spawnPrefab} prefab");
            if (!CarIsInBlacklist(cruiserCar.itemName))
            {
                NetworkPrefabs.RegisterNetworkPrefab(cruiserCar.spawnPrefab);
                Utilities.FixMixerGroups(cruiserCar.spawnPrefab);
                Items.RegisterItem(cruiserCar);
                Items.RegisterShopItem(cruiserCar, price: instance.cruiserCarPrice.Value);
            }
            
            //WexopCar
            Item wexopCar =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCarWexop.asset");
            Logger.LogInfo($"{wexopCar.name} FOUND");
            Logger.LogInfo($"{wexopCar.spawnPrefab} prefab");
            if (!CarIsInBlacklist(wexopCar.itemName))
            {
                NetworkPrefabs.RegisterNetworkPrefab(wexopCar.spawnPrefab);
                Utilities.FixMixerGroups(wexopCar.spawnPrefab);
                Items.RegisterItem(wexopCar);
                Items.RegisterShopItem(wexopCar, price: instance.wexopCarPrice.Value);
            }
        }

        public void LoadConfigs()
        {
            
            //GENERAL
            honkVolume = Config.Bind(
                "General", "honkVolume", 
                1f, 
                "Honk volume. No need to restart the game :)"
                );
            CreateFloatConfig(honkVolume,0f, 2f);
            
            engineVolume = Config.Bind(
                "General", "engineVolume", 
                0.2f, 
                "Engine volume. No need to restart the game :)"
                );
            CreateFloatConfig(engineVolume,0f, 2f);
            
            rotationSpeed = Config.Bind(
                "General", "rotationSpeed", 
                7f, 
                "Cars rotation speed. No need to restart the game :)"
                );
            CreateFloatConfig(rotationSpeed,0f, 200f);
            
            explosionDamage = Config.Bind(
                "General", "explosionDamage", 
                50, 
                "Cars explosion damage on destroy. No need to restart the game :)"
                );
            CreateIntConfig(explosionDamage,0, 200);
            
            blacklistCar = Config.Bind(
                "General", "blacklistCar", 
                "RCWexopCar", 
                "Blacklist car, they will not be added into the game. You can find cars name into the terminal store. Use this config like this : RCCar,RCPoliceCar"
                );
            CreateStringConfig(blacklistCar,true);
            
            //Network
            
            syncInterval = Config.Bind(
                "General", "syncInterval", 
                0.35f, 
                "Cars sync interval between players. No need to restart the game :)"
            );
            CreateFloatConfig(syncInterval,0f, 2);
            
            //PRICE
            
            carPrice = Config.Bind(
                "Price", "carPrice", 
                100, 
                "Normal car price. You need to restart the game."
                );
            CreateIntConfig(carPrice,0, 1000, true);
            
            policeCarPrice = Config.Bind(
                "Price", "policeCarPrice", 
                150, 
                "Police car price. You need to restart the game."
                );
            CreateIntConfig(policeCarPrice,0, 1000, true);
            
            ambulanceCarPrice = Config.Bind(
                "Price", "ambulanceCarPrice", 
                175, 
                "Ambulance car price. You need to restart the game."
                );
            CreateIntConfig(ambulanceCarPrice,0, 1000, true);
            
            sportCarPrice = Config.Bind(
                "Price", "sportCarPrice", 
                200, 
                "Sport car price. You need to restart the game."
                );
            CreateIntConfig(sportCarPrice,0, 1000, true);
            
            bombCarPrice = Config.Bind(
                "Price", "bombCarPrice", 
                75, 
                "Bomb car price. You need to restart the game."
                );
            CreateIntConfig(bombCarPrice,0, 1000, true);
            
            cruiserCarPrice = Config.Bind(
                "Price", "cruiserCarPrice", 
                125, 
                "Cruiser car price. You need to restart the game."
                );
            CreateIntConfig(cruiserCarPrice,0, 1000, true);
            
            wexopCarPrice = Config.Bind(
                "Price", "wexopCarPrice", 
                200, 
                "Wexop car price. You need to restart the game."
                );
            CreateIntConfig(wexopCarPrice,0, 1000, true);
            
        }
        
        public static string ConditionalString(string value)
        {
            var finalString = value.ToLower();
            while (finalString.Contains(" "))
            {
                finalString = finalString.Replace(" ", "");
            }

            return finalString;
        }

        private bool CarIsInBlacklist(string name)
        {
            
            if (instance.blacklistCar.Value == "") return false;
            
            var nameSearch = ConditionalString(name);
            var disable = false;
            
            instance.blacklistCar.Value.Split(",").ToList().ForEach(hazard =>
            {
                if (hazard != "")
                {
                    var hazardName = ConditionalString(hazard);
                    if (nameSearch.Contains(hazardName)) disable = true;
                }
            });
            
            return disable;
            
        }

        private void CreateFloatConfig(ConfigEntry<float> configEntry, float min = 0f, float max = 100f)
        {
            var exampleSlider = new FloatSliderConfigItem(configEntry, new FloatSliderOptions
            {
                Min = min,
                Max = max,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }

        private void CreateIntConfig(ConfigEntry<int> configEntry, int min = 0, int max = 100, bool restart = false)
        {
            var exampleSlider = new IntSliderConfigItem(configEntry, new IntSliderOptions()
            {
                Min = min,
                Max = max,
                RequiresRestart = restart
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }

        private void CreateStringConfig(ConfigEntry<string> configEntry, bool requireRestart = false)
        {
            var exampleSlider = new TextInputFieldConfigItem(configEntry, new TextInputFieldOptions
            {
                RequiresRestart = requireRestart
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }

        private void CreateBoolConfig(ConfigEntry<bool> configEntry)
        {
            var exampleSlider = new BoolCheckBoxConfigItem(configEntry, new BoolCheckBoxOptions
            {
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }
    }
}