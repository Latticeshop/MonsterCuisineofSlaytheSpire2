using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Patches;

/// <summary>
/// 诊断补丁：ModelDb 初始化完成后统计本 Mod 注册的模型数量，
/// 用于确认卡牌/遗物/能力/池子是否被游戏正确识别。
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Init))]
public static class ModelDbDiagnosticsPatch
{
    static void Postfix()
    {
        int cards = ModelDb.AllAbstractModelSubtypes.Count(
            type => type.Namespace == "MonsterCuisineCode.Cards");
        int relics = ModelDb.AllAbstractModelSubtypes.Count(
            type => type.Namespace == "MonsterCuisineCode.Relics");
        int powers = ModelDb.AllAbstractModelSubtypes.Count(
            type => type.Namespace == "MonsterCuisineCode.Powers");
        ModLogger.Instance.Info(
            $"尖塔乐事模型注册：卡牌 {cards}，遗物 {relics}，能力 {powers}");
    }
}
