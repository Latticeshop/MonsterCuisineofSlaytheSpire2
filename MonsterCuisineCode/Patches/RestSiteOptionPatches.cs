using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;
using MonsterCuisineCode.RestSite;

namespace MonsterCuisineCode.Patches;

/// <summary>
/// 料理选项与原版休息事件独立的补丁：
/// 1. 图标复用游戏的"烹饪"图标（OptionId 独立后自动图标不存在）；
/// 2. 料理成功后保留剩余休息选项（不消耗休息次数）。
/// </summary>
public static class RestSiteOptionPatches
{
    [HarmonyPatch(typeof(RestSiteOption), nameof(RestSiteOption.Icon), MethodType.Getter)]
    [HarmonyPostfix]
    private static void IconPostfix(RestSiteOption __instance, ref Texture2D __result)
    {
        if (__instance is CookingRestSiteOption)
        {
            __result = ResourceLoader.Load<Texture2D>(
                "res://images/ui/rest_site/option_cook.png");
        }
    }

    [HarmonyPatch(
        typeof(Hook),
        nameof(Hook.ShouldDisableRemainingRestSiteOptions))]
    [HarmonyPostfix]
    private static void ShouldDisableRemainingPostfix(
        IRunState runState,
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        ref bool __result)
    {
        if (CookingRestSiteOption.KeepRestSiteOptionsOpen)
        {
            CookingRestSiteOption.KeepRestSiteOptionsOpen = false;
            __result = false;
        }
    }
}
