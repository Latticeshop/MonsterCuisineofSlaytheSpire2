using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MonsterCuisineCode.RestSite;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Patches;

/// <summary>
/// 料理选项图标补丁。
/// 注意：`RestSiteOption.Icon` 是非虚属性，且原始 getter 会加载"不存在的 option_mc_cook.png"直接抛错。
/// 属性 getter 用 `[HarmonyPatch]` 注解经 PatchAll 发现不可靠（与基石符文等 Mod 遇到的问题一致），
/// 因此这里改为在 <see cref="Install"/> 中手动 Patch（Prefix 返回 false 跳过原逻辑）。
/// </summary>
public static class RestSiteOptionPatches
{
    private const string ModIconPath =
        "res://MonsterCuisineResources/image/RestSite/料理.png";

    private const string FallbackIconPath =
        "res://images/ui/rest_site/option_cook.png";

    private static bool _loggedOnce;

    public static void Install(Harmony harmony)
    {
        MethodInfo? getter = AccessTools.PropertyGetter(
            typeof(RestSiteOption), nameof(RestSiteOption.Icon));
        if (getter == null)
        {
            ModLogger.Instance.Warn(
                "RestSiteOption.Icon getter 未找到，料理选项图标将使用默认路径。");
            return;
        }

        harmony.Patch(getter, prefix: new HarmonyMethod(
            typeof(RestSiteOptionPatches), nameof(IconPrefix)));
        ModLogger.Instance.Info("RestSiteOption.Icon 图标补丁已手动安装。");
    }

    private static bool IconPrefix(RestSiteOption __instance, ref Texture2D __result)
    {
        if (__instance is not CookingRestSiteOption)
        {
            return true; // 其他选项走原逻辑
        }

        if (!_loggedOnce)
        {
            _loggedOnce = true;
            ModLogger.Instance.Info("料理选项图标补丁已生效，正在替换图标。");
        }

        string path = ResourceLoader.Exists(ModIconPath) ? ModIconPath : FallbackIconPath;
        __result = ResourceLoader.Load<Texture2D>(path);
        return false;
    }
}
