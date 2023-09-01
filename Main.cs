using System;
using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using CompleteChaos;
using HarmonyLib;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Map;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppAssets.Scripts.Utils;
using MelonLoader;
using UnityEngine;
using Main = CompleteChaos.Main;
using Random = UnityEngine.Random;

[assembly: MelonInfo(typeof(Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace CompleteChaos;

[HarmonyPatch]
public class Main : BloonsTD6Mod
{
    public static RandomExt MainRandom = new();
    internal static HashSet<TowerModel> TowerModels = new();
    private static HashSet<BloonModel> BloonModels = new();
    private static HashSet<string> Maps = new();
    private static HashSet<SpriteReference> UpgradeIcons = new();
    private static HashSet<string> UpgradeNames = new();
    private static HashSet<SpriteReference> TowerPortraits = new();
    private static HashSet<string> TowerNames = new();
    internal static HashSet<ProjectileModel> Projectiles = new();

    private static string[] borken =
    {
        
    };

    public override void OnNewGameModel(GameModel result)
    {
        TowerModels = new();
        BloonModels = new();
        UpgradeIcons = new();
        UpgradeNames = new();
        TowerPortraits = new();
        TowerNames = new();
        Projectiles = new();

        foreach (var projectileModel in result.GetDescendants<ProjectileModel>().ToList())
        {
            Projectiles.Add(projectileModel);
        }

        foreach (var tower in result.towers.Where(x => !x.IsHero() && !x.isSubTower))
        {
            if (!borken.Any(tower.name.Contains))
            {
                TowerModels.Add(tower);
            }
            
            TowerNames.Add(tower.baseId);

            foreach (var upgrade in tower.upgrades.Select(x => x.GetUpgrade()))
            {
                UpgradeIcons.Add(upgrade.icon);
                UpgradeNames.Add(upgrade.name);
            }

            TowerPortraits.Add(tower.portrait);
        }

        var validbloons = result.bloons.Where(x =>
            !x.isBoss && !x.isInvulnerable && !x.name.Contains("Lych") && !x.name.Contains("Dread") &&
            !x.name.Contains("Golden") && !x.name.Contains("Test")).ToArray();
        foreach (var bloon in validbloons)
        {
            BloonModels.Add(bloon);
        }
    }

    [HarmonyPatch(typeof(MainHudRightAlign), nameof(MainHudRightAlign.Initialise))]
    [HarmonyPostfix]
    public static void AddRoundInfoButton(ref MainHudRightAlign __instance)
    {
        SpeedUI.Create(__instance.panel);
        SpeedUI.Update(_currentSpeed);
        
        TimeUI.Create(__instance.panel);
        TimeUI.Update(_nextActionTime);
    }
    
    private static float _nextActionTime = Random.Range(5, 45);
    private static float period;
    private static float _currentSpeed = Random.Range(.01f, 7);

    public override void OnUpdate()
    {
        if (InGame.instance?.bridge is null) return;

        if (period > _nextActionTime)
        {
            _currentSpeed = Random.Range(.01f, 7);
            SpeedUI.Update(_currentSpeed);

            var gameModel = InGame.instance.GetGameModel();

            foreach (var upgradeModel in gameModel.upgrades)
            {
                var baseUpgrade = Game.instance.model.GetUpgrade(upgradeModel.name);
                upgradeModel.cost = Random.Range(baseUpgrade.cost / 2, baseUpgrade.cost * 2);
                upgradeModel.icon = UpgradeIcons.GetRandomElement();
                upgradeModel.name = UpgradeNames.GetRandomElement();
            }


            foreach (var tts in InGame.instance.GetAllTowerToSim().Where(tts => !tts.Def.isSubTower))
            {
                if (tts?.tower is null) continue;
                if (tts.tower.towerModel.IsHero() || tts.tower.towerModel.isSubTower ||
                    tts.tower.towerModel.isGeraldoItem || tts.tower.towerModel.isPowerTower) continue;

                var tower = tts.tower;
                if (Random.Range(0, 100) > 75) continue;
                if (Random.Range(0, 100) > 98)
                {
                    tower.SellTower();
                    continue;
                }

                if (TowerSelectionMenu.instance.selectedTower is not null &&
                    TowerSelectionMenu.instance.selectedTower.Equals(tower.GetTowerToSim()))
                {
                    tower.PlaceRandom();
                }
                else
                {
                    tower.PlaceRandom(false);
                }
            }

            _nextActionTime = Random.Range(5, 35);
            period = 0;
        }
    }

    public override void OnLateUpdate()
    {
        if (InGame.instance?.bridge is null) return;
        TimeManager.timeScaleWithoutNetwork = _currentSpeed;
        TimeManager.networkScale = _currentSpeed;
        TimeManager.maxSimulationStepsPerUpdate = _currentSpeed;
        period += Time.unscaledDeltaTime;
        TimeUI.Update(-(period - _nextActionTime));
    }

    public override void OnMainMenu()
    {
        Maps = GameData._instance.mapSet.maps.Where(x => Game.instance.GetBtd6Player().IsMapUnlocked(x.id))
            .Select(x => x.id).ToHashSet();
    }

    public override void OnTowerUpgraded(Tower tower, string upgradeName, TowerModel newBaseTowerModel)
    {
        if (TowerSelectionMenu.instance.selectedTower is not null &&
            TowerSelectionMenu.instance.selectedTower.Equals(tower.GetTowerToSim()))
        {
            tower.PlaceRandom();
        }
        else
        {
            tower.PlaceRandom(false);
        }
    }

    [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.LoadScene))]
    [HarmonyPrefix]
    private static void MapLoader_LoadScene(ref MapLoader __instance)
    {
        __instance.currentMapName = Maps.GetRandomElement();
    }

    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.Show))]
    [HarmonyPatch(typeof(TowerSelectionMenu), nameof(TowerSelectionMenu.SelectionChanged))]
    [HarmonyPostfix]
    private static void TowerSelectionMenu_SelectionChanged_Postfix(TowerSelectionMenu __instance)
    {
        __instance.paragonDegree.text =
            Random.Range(1, InGame.instance.GetGameModel().paragonDegreeDataModel.degreeCount).ToString();
        if (__instance.selectedTower?.tower is null)
            return;

        __instance.upgradeButtons.ForEach(upgradeObject =>
        {
            if (!upgradeObject.upgradeButton.lockedText.enabled)
            {
                upgradeObject.upgradeButton.icon.SetSprite(UpgradeIcons.GetRandomElement());
                upgradeObject.upgradeButton.label.SetText(UpgradeNames.GetRandomElement());
            }

            if (!upgradeObject.currentUpgrade.notUpgraded.active)
            {
                upgradeObject.currentUpgrade.icon.SetSprite(UpgradeIcons.GetRandomElement());
                upgradeObject.currentUpgrade.label.SetText(UpgradeNames.GetRandomElement());
            }
        });
        
        
        var newtower = __instance.selectedTower.tower.towerModel.Duplicate();
        newtower.portrait = TowerPortraits.GetRandomElement();
        __instance.selectedTower.tower.UpdateRootModel(newtower);
    }
    
    [HarmonyPatch(typeof(InputManager), nameof(InputManager.CreateTowerGraphicsAsync))]
    [HarmonyPrefix]
    private static void CreateTowerGraphicsAsync(ref TowerModel towerModel)
    {
        towerModel = Extensions.RandomTowerModel(TowerModels.GetRandomElement());
    }

    [HarmonyPatch(typeof(InputManager), nameof(InputManager.PrimeTower))]
    [HarmonyPrefix]
    private static void InputManager_PrimeTower(ref TowerModel tm)
    {
        tm = Extensions.RandomTowerModel(TowerModels.GetRandomElement());
    }

    [HarmonyPatch(typeof(Bloon), nameof(Bloon.Initialise))]
    [HarmonyPrefix]
    private static void Bloon_Initialize(ref Model modelToUse)
    {
        modelToUse = BloonModels.GetRandomElement().Duplicate();
    }

    [HarmonyPatch(typeof(Bloon), nameof(Bloon.Degrade))]
    [HarmonyPrefix]
    private static void Bloon_Degrade(ref Bloon __instance, ref bool blockSpawnChildren)
    {
        if (__instance.lineage.Count <= 1) return;
        blockSpawnChildren = true;
    }
}