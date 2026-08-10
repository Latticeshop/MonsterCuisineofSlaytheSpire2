using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MonsterCuisineCode.Cards;
using MonsterCuisineCode.Patches;
using MonsterCuisineCode.Relics;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode;

[ModInitializer(nameof(Initialize))]
public static class ModInitializer
{
    public const string ModId = "SpireDelight";

    public static void Initialize()
    {
        // 卡牌注册进原版无色卡池：Token 稀有度不参与默认卡牌奖励/商店，仅百科可查。
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(VanillaJelly));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(BeadCookie));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(BruteTail));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(SpikySnakeMeat));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(SnakeFruitMeat));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(KinDumpling));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(ActiveAcid));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(SpikyJelly));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(VineNoodleBundle));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(GreenDumpling));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(SporeMushroom));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(GlowingMushroom));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(DeerAntler));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(Stone));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(BirdMeat));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(Petroleum));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(ActivePetroleum));
        ModHelper.AddModelToPool(typeof(ColorlessCardPool), typeof(NibbitMeat));

        // 遗物注册进原版事件遗物池：Event 稀有度不参与默认遗物奖励，仅百科可查。
        ModHelper.AddModelToPool(typeof(EventRelicPool), typeof(Meatball));
        ModHelper.AddModelToPool(typeof(EventRelicPool), typeof(BigPotMeat));
        ModHelper.AddModelToPool(typeof(EventRelicPool), typeof(Hamburger));
        ModHelper.AddModelToPool(typeof(EventRelicPool), typeof(RoastedStone));
        ModHelper.AddModelToPool(typeof(EventRelicPool), typeof(FailedDish));

        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        RestSiteOptionPatches.Install(harmony);
        ModLogger.Instance.Info("尖塔乐事（SpireDelight）Mod 初始化完成，Harmony 补丁已应用。");
    }
}
