using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MonsterCuisineCode.Cards;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Patches;

/// <summary>
/// 击杀对应怪物时，掉落对应的怪物料理卡牌（战斗结束奖励）。
/// </summary>
[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.OnCombatEnded))]
public static class MonsterDropPatch
{
    private static readonly Dictionary<Type, Func<CardModel>> Drops = new()
    {
        { typeof(LeafSlimeS), () => ModelDb.Card<VanillaJelly>() },
        { typeof(LeafSlimeM), () => ModelDb.Card<VanillaJelly>() },
        { typeof(ShrinkerBeetle), () => ModelDb.Card<BeadCookie>() },
        { typeof(Mawler), () => ModelDb.Card<BruteTail>() },
        { typeof(SlitheringStrangler), () => ModelDb.Card<SpikySnakeMeat>() },
        { typeof(SnappingJaxfruit), () => ModelDb.Card<SnakeFruitMeat>() },
        { typeof(KinFollower), () => ModelDb.Card<KinDumpling>() },
        { typeof(KinPriest), () => ModelDb.Card<KinDumpling>() },
        { typeof(FuzzyWurmCrawler), () => ModelDb.Card<ActiveAcid>() },
        { typeof(TwigSlimeS), () => ModelDb.Card<SpikyJelly>() },
        { typeof(TwigSlimeM), () => ModelDb.Card<SpikyJelly>() },
        { typeof(VineShambler), () => ModelDb.Card<VineNoodleBundle>() },
        { typeof(PhrogParasite), () => ModelDb.Card<GreenDumpling>() },
        { typeof(Fogmog), () => ModelDb.Card<SporeMushroom>() },
        { typeof(Flyconid), () => ModelDb.Card<GlowingMushroom>() },
        { typeof(CeremonialBeast), () => ModelDb.Card<DeerAntler>() },
        { typeof(BygoneEffigy), () => ModelDb.Card<Stone>() },
        { typeof(Byrdonis), () => ModelDb.Card<BirdMeat>() },
        { typeof(Inklet), () => ModelDb.Card<Petroleum>() },
        { typeof(Vantom), () => ModelDb.Card<ActivePetroleum>() },
        { typeof(Nibbit), () => ModelDb.Card<NibbitMeat>() }
    };

    [HarmonyPostfix]
    private static void AddFoodRewards(CombatRoom __instance)
    {
        if (__instance.Encounter == null)
        {
            return;
        }

        var awarded = new HashSet<Type>();
        foreach ((var monster, _) in __instance.Encounter.MonstersWithSlots)
        {
            Type monsterType = monster.GetType();
            if (!Drops.TryGetValue(monsterType, out Func<CardModel>? factory) ||
                factory == null)
            {
                continue;
            }

            Type cardType = factory().GetType();
            if (!awarded.Add(cardType))
            {
                continue;
            }

            foreach (var player in __instance.CombatState.Players)
            {
                CardModel card = player.RunState.CreateCard(
                    factory(), player);
                __instance.AddExtraReward(player, new SpecialCardReward(card, player));
            }
            ModLogger.Instance.Info(
                $"尖塔乐事掉落触发：{monsterType.Name} → {cardType.Name}");
        }
    }
}
