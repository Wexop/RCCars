using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalLevelLoader;
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
        private const string VERSION = "1.0.0";

        public static RCCarsPlugin instance;

        public Dictionary<ulong, RegistredCar> RegistredCars = new Dictionary<ulong, RegistredCar>();

        public ConfigEntry<float> honkVolume;
        public ConfigEntry<float> engineVolume;
        public ConfigEntry<float> rotationSpeed;
        
        public ConfigEntry<int> carPrice;
        public ConfigEntry<int> policeCarPrice;

        private void Awake()
        {
            instance = this;

            Logger.LogInfo("RCCars starting....");
            
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "rccars");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            LoadConfigs();
            RegisterCar(bundle);


            Logger.LogInfo("RCCars Patched !!");
        }

        void RegisterCar(AssetBundle bundle)
        {
            //NormalCar
            Item carAsset =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCar.asset");
            Logger.LogInfo($"{carAsset.name} FOUND");
            Logger.LogInfo($"{carAsset.spawnPrefab} prefab");
            NetworkPrefabs.RegisterNetworkPrefab(carAsset.spawnPrefab);
            Utilities.FixMixerGroups(carAsset.spawnPrefab);
            Items.RegisterItem(carAsset);
            Items.RegisterShopItem(carAsset, price: instance.carPrice.Value);
            
            //NormalCar
            Item policeCarAsset =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCarPolice.asset");
            Logger.LogInfo($"{policeCarAsset.name} FOUND");
            Logger.LogInfo($"{policeCarAsset.spawnPrefab} prefab");
            NetworkPrefabs.RegisterNetworkPrefab(policeCarAsset.spawnPrefab);
            Utilities.FixMixerGroups(policeCarAsset.spawnPrefab);
            Items.RegisterItem(policeCarAsset);
            Items.RegisterShopItem(policeCarAsset, price: instance.policeCarPrice.Value);
        }

        public void LoadConfigs()
        {
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
            
            //PRICE
            
            carPrice = Config.Bind(
                "Price", "carPrice", 
                100, 
                "Car price. No need to restart the game :)"
                );
            CreateIntConfig(carPrice,0, 1000);
            
            policeCarPrice = Config.Bind(
                "Price", "policeCarPrice", 
                150, 
                "Police car price. No need to restart the game :)"
                );
            CreateIntConfig(policeCarPrice,0, 1000);
            
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

        private void CreateIntConfig(ConfigEntry<int> configEntry, int min = 0, int max = 100)
        {
            var exampleSlider = new IntSliderConfigItem(configEntry, new IntSliderOptions()
            {
                Min = min,
                Max = max,
                RequiresRestart = false
            });
            LethalConfigManager.AddConfigItem(exampleSlider);
        }

        private void CreateStringConfig(ConfigEntry<string> configEntry)
        {
            var exampleSlider = new TextInputFieldConfigItem(configEntry, new TextInputFieldOptions
            {
                RequiresRestart = false
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