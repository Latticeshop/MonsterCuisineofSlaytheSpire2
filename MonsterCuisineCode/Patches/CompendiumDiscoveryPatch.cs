using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using MegaCrit.Sts2.Core.Saves;
using MonsterCuisineCode.Cards;
using MonsterCuisineCode.Relics;

namespace MonsterCuisineCode.Patches;

/// <summary>
/// 百科"已发现"标记补丁：打开卡牌/遗物图鉴时，把本 Mod 内容标记为已见，
/// 让玩家无需先获得即可在百科中直接查阅（不参与默认奖励/商店）。
/// ID 惰性获取，避免在 ModelDb 初始化完成前访问。
/// </summary>
public static class CompendiumDiscoveryPatch
{
    private static HashSet<ModelId>? _cardIds;
    private static HashSet<ModelId>? _relicIds;

    private static HashSet<ModelId> CardIds => _cardIds ??= new HashSet<ModelId>
    {
        ModelDb.Card<VanillaJelly>().Id,
        ModelDb.Card<BeadCookie>().Id,
        ModelDb.Card<BruteTail>().Id,
        ModelDb.Card<SpikySnakeMeat>().Id,
        ModelDb.Card<SnakeFruitMeat>().Id,
        ModelDb.Card<KinDumpling>().Id,
        ModelDb.Card<ActiveAcid>().Id,
        ModelDb.Card<SpikyJelly>().Id,
        ModelDb.Card<VineNoodleBundle>().Id,
        ModelDb.Card<GreenDumpling>().Id,
        ModelDb.Card<SporeMushroom>().Id,
        ModelDb.Card<GlowingMushroom>().Id,
        ModelDb.Card<DeerAntler>().Id,
        ModelDb.Card<Stone>().Id,
        ModelDb.Card<BirdMeat>().Id,
        ModelDb.Card<Petroleum>().Id,
        ModelDb.Card<ActivePetroleum>().Id,
        ModelDb.Card<NibbitMeat>().Id
    };

    private static HashSet<ModelId> RelicIds => _relicIds ??= new HashSet<ModelId>
    {
        ModelDb.Relic<Meatball>().Id,
        ModelDb.Relic<BigPotMeat>().Id,
        ModelDb.Relic<Hamburger>().Id,
        ModelDb.Relic<RoastedStone>().Id,
        ModelDb.Relic<FailedDish>().Id
    };

    [HarmonyPatch(typeof(NCardLibraryGrid), nameof(NCardLibraryGrid.RefreshVisibility))]
    [HarmonyPrefix]
    private static void MarkCardsSeen()
    {
        ProgressState progress = SaveManager.Instance.Progress;
        foreach (ModelId id in CardIds)
        {
            progress.MarkCardAsSeen(id);
        }
    }

    [HarmonyPatch(typeof(NRelicCollection), "LoadRelics")]
    [HarmonyPrefix]
    private static void MarkRelicsSeen()
    {
        ProgressState progress = SaveManager.Instance.Progress;
        foreach (ModelId id in RelicIds)
        {
            progress.MarkRelicAsSeen(id);
        }
    }
}
