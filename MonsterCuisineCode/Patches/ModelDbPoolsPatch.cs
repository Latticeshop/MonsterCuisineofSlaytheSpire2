using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Pools;

namespace MonsterCuisineCode.Patches;

/// <summary>把怪物料理的卡池/遗物池注册进 ModelDb，使料理卡与料理遗物可被游戏识别。</summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
public static class AllCardPoolsPatch
{
    static void Postfix(ref IEnumerable<CardPoolModel> __result)
    {
        __result = __result.Append(ModelDb.CardPool<MonsterCuisineCardPool>()).Distinct();
    }
}

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllRelicPools), MethodType.Getter)]
public static class AllRelicPoolsPatch
{
    static void Postfix(ref IEnumerable<RelicPoolModel> __result)
    {
        __result = __result.Append(ModelDb.RelicPool<MonsterCuisineRelicPool>()).Distinct();
    }
}
