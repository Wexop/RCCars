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

        private void Awake()
        {
            instance = this;

            Logger.LogInfo("RCCars starting....");
            
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "rccars");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            RegisterCar(bundle);


            Logger.LogInfo("RCCars Patched !!");
        }

        void RegisterCar(AssetBundle bundle)
        {
            //colorfulJar
            Item carAsset =
                bundle.LoadAsset<Item>("Assets/LethalCompany/Mods/RCCars/RCCar.asset");
            Logger.LogInfo($"{carAsset.name} FOUND");
            Logger.LogInfo($"{carAsset.spawnPrefab} prefab");
            NetworkPrefabs.RegisterNetworkPrefab(carAsset.spawnPrefab);
            Utilities.FixMixerGroups(carAsset.spawnPrefab);
            Items.RegisterItem(carAsset);
            Items.RegisterShopItem(carAsset, price: 50);
        }



        private void CreateFloatConfig(ConfigEntry<float> configEntry, float min = 0f, float max = 30f)
        {
            var exampleSlider = new FloatSliderConfigItem(configEntry, new FloatSliderOptions
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