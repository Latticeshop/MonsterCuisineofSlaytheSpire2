using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MonsterCuisineCode.Cards;

namespace MonsterCuisineCode.Patches;

/// <summary>
/// 击杀对应怪物时，掉落对应的怪物料理卡牌（战斗结束奖励）。
/// </summary>
[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.OnCombatEnded))]
public static class MonsterDropPatch
{
    private static readonly Dictionary<Type, Type> Drops = new()
    {
        { typeof(LeafSlimeS), typeof(VanillaJelly) },
        { typeof(LeafSlimeM), typeof(VanillaJelly) },
        { typeof(ShrinkerBeetle), typeof(BeadCookie) },
        { typeof(Mawler), typeof(BruteTail) },
        { typeof(SlitheringStrangler), typeof(SpikySnakeMeat) },
        { typeof(SnappingJaxfruit), typeof(SnakeFruitMeat) },
        { typeof(KinFollower), typeof(KinDumpling) },
        { typeof(KinPriest), typeof(KinDumpling) },
        { typeof(FuzzyWurmCrawler), typeof(ActiveAcid) },
        { typeof(TwigSlimeS), typeof(SpikyJelly) },
        { typeof(TwigSlimeM), typeof(SpikyJelly) },
        { typeof(VineShambler), typeof(VineNoodleBundle) },
        { typeof(PhrogParasite), typeof(GreenDumpling) },
        { typeof(Fogmog), typeof(SporeMushroom) },
        { typeof(Flyconid), typeof(GlowingMushroom) },
        { typeof(CeremonialBeast), typeof(DeerAntler) },
        { typeof(BygoneEffigy), typeof(Stone) },
        { typeof(Byrdonis), typeof(BirdMeat) },
        { typeof(Inklet), typeof(Petroleum) },
        { typeof(Vantom), typeof(ActivePetroleum) },
        { typeof(Nibbit), typeof(NibbitMeat) }
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
            if (!Drops.TryGetValue(monsterType, out Type? cardType) ||
                cardType == null ||
                !awarded.Add(cardType))
            {
                continue;
            }

            foreach (var player in __instance.CombatState.Players)
            {
                CardModel card = player.RunState.CreateCard(
                    (CardModel)Activator.CreateInstance(cardType)!, player);
                __instance.AddExtraReward(player, new SpecialCardReward(card, player));
            }
        }
    }
}
