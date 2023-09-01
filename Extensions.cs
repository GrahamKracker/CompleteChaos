using System;
using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.AbilitiesMenu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.TowerSelectionMenu;
using Il2CppSystem.IO;
using MelonLoader;
using Random = UnityEngine.Random;

namespace CompleteChaos;

using static Main;

public static class Extensions
{
    public static Tower PlaceRandom(this Tower baseTower, bool updateselection = true)
    {
        var newtower = InGame.instance.GetTowerManager()
            .CreateTower(TowerModels.GetRandomElement().Duplicate(), baseTower.Position,
                InGame.Bridge.MyPlayerNumber, baseTower.areaPlacedOn, baseTower.parentTowerId, null, false, false);
        var tts = newtower.GetTowerToSim();

        newtower.AddPoppedCash(baseTower.cashEarned);
        newtower.appliedCash = baseTower.GetAppliedCash();
        newtower.damageDealt = baseTower.damageDealt;
        newtower.worth = baseTower.worth;
        newtower.shouldShowCashIconInstead = baseTower.shouldShowCashIconInstead;

        //no more base tower from here down
        InGame.instance.GetTowerManager().DestroyTower(baseTower, InGame.Bridge.MyPlayerNumber);
        if (updateselection)
        {
            TowerSelectionMenu.instance.DeselectTower();      
            InGame.instance.inputManager.UpdateRangeMeshes();
            InGame.instance.inputManager.SetSelected(tts);
            InGame.instance.GetSimulation().SelectionChanged(tts.tower);
            TowerSelectionMenu.instance.SelectTower(tts);
        }

        AbilityMenu.instance.RebuildAbilities();
        AbilityMenu.instance.AbilitiesChanged();
        newtower.UpdateThrowCache();
        newtower.UpdateBuffs();
        newtower.UpdateThrowLocation();
        newtower.UpdateTargetType();
        newtower.UpdateRoundMutators();


        newtower.RandomizeStats();
        return newtower;
    }

    public static TowerModel RandomTowerModel(TowerModel baseTower)
    {
        var towerModel = baseTower.Duplicate();
        towerModel.range = Random.Range(.1f, 100f);
        towerModel.ignoreBlockers = MainRandom.NextBoolean();

        foreach (var attackModel in towerModel.GetDescendants<AttackModel>().ToArray())
        {
            attackModel.range = Random.Range(.1f, 100f);
            attackModel.attackThroughWalls = MainRandom.NextBoolean();
            attackModel.fireWithoutTarget = MainRandom.NextBoolean();
        }

        foreach (var weaponModel in towerModel.GetDescendants<WeaponModel>().ToArray())
        {
            weaponModel.Rate = Random.Range(.01f, 5f);
            weaponModel.fireBetweenRounds = MainRandom.NextBoolean();
            weaponModel.fireWithoutTarget = MainRandom.NextBoolean();
            weaponModel.startInCooldown = MainRandom.NextBoolean();
        }

        var projectileModels = towerModel.GetDescendants<ProjectileModel>().ToArray();
        for (var index = 0; index < projectileModels.Count; index++)
        {
            var projectileModel = projectileModels[index] = Projectiles.GetRandomElement().Duplicate();
            projectileModel.pierce = Random.Range(0, 100);
            projectileModel.maxPierce = projectileModel.pierce;
            projectileModel.ignoreBlockers = MainRandom.NextBoolean();
            projectileModel.ignorePierceExhaustion = MainRandom.NextBoolean();
            projectileModel.canCollisionBeBlockedByMapLos = MainRandom.NextBoolean();
        }

        foreach (var damageModel in towerModel.GetDescendants<DamageModel>().ToArray())
        {
            damageModel.damage = Random.Range(0, 100);
            damageModel.createPopEffect = MainRandom.NextBoolean();
            damageModel.distributeToChildren = MainRandom.NextBoolean();
            damageModel.createPopEffect = MainRandom.NextBoolean();
            damageModel.maxDamage = damageModel.damage;
            damageModel.immuneBloonProperties = MainRandom.NextEnum<BloonProperties>();
            damageModel.immuneBloonPropertiesOriginal = damageModel.immuneBloonProperties;
        }

        return towerModel;
    }

    public static void RandomizeStats(this Tower baseTower)
    {
        try
        {
            baseTower.UpdateRootModel(RandomTowerModel(baseTower.towerModel));
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
        }
    }
    
    public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
    {
        var list = enumerable.ToList();
        return list.ElementAt(Random.Range(0, list.Count));
    }
}