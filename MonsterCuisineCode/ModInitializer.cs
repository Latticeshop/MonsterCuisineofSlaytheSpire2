using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace MonsterCuisineCode;

[ModInitializer(nameof(Initialize))]
public static class ModInitializer
{
    public const string ModId = "MonsterCuisine";

    public static void Initialize()
    {
        // 卡牌/遗物/能力模型由 ModelDb 自动扫描本程序集注册，无需手动注册。
        var harmony = new Harmony(ModId);
        harmony.PatchAll();
    }
}
