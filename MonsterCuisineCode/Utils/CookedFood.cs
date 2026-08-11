using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Cards;

namespace MonsterCuisineCode.Utils;

/// <summary>
/// 熟食材映射表：生食材卡牌 → 对应熟食材卡牌（烹饪产物）。
/// 篝火"烹饪"选项依据此表把 1 张生食材卡转换为对应的熟食材卡。
/// </summary>
public static class CookedFood
{
    private static readonly IReadOnlyDictionary<Type, Func<CardModel>> Recipes =
        new Dictionary<Type, Func<CardModel>>
        {
            [typeof(KinDumpling)] = () => ModelDb.Card<GreenLeafStickyBall>(),
            [typeof(BeadCookie)] = () => ModelDb.Card<StickyPearlSoftCake>(),
            [typeof(BruteTail)] = () => ModelDb.Card<CookedBruteTail>(),
            [typeof(VanillaJelly)] = () => ModelDb.Card<VanillaPudding>(),
            [typeof(SpikySnakeMeat)] = () => ModelDb.Card<RoastedSnakeMeat>(),
            [typeof(ActiveAcid)] = () => ModelDb.Card<RoastedSourBug>(),
            [typeof(SpikyJelly)] = () => ModelDb.Card<SoftPudding>(),
            [typeof(VineNoodleBundle)] = () => ModelDb.Card<VineNoodleSalad>(),
            [typeof(GreenDumpling)] = () => ModelDb.Card<RoastedGreenDumpling>(),
            [typeof(SporeMushroom)] = () => ModelDb.Card<CookedSporeMushroom>(),
            [typeof(SnakeFruitMeat)] = () => ModelDb.Card<CookedSnakeFruit>(),
            [typeof(GlowingMushroom)] = () => ModelDb.Card<RoastedGlowingMushroom>(),
            [typeof(DeerAntler)] = () => ModelDb.Card<RoastedDeerAntler>(),
            [typeof(Stone)] = () => ModelDb.Card<FireHead>(),
            [typeof(BirdMeat)] = () => ModelDb.Card<RoastedBirdMeat>(),
            [typeof(Petroleum)] = () => ModelDb.Card<TransformedPetroleum>(),
            [typeof(ActivePetroleum)] = () => ModelDb.Card<AngryPetroleum>()
        };

    /// <summary>该生食材卡是否可以烹饪成熟食材卡。</summary>
    public static bool CanCook(CardModel card) => Recipes.ContainsKey(card.GetType());

    /// <summary>返回生食材卡对应的熟食材卡（规范实例）；无可烹饪时返回 null。</summary>
    public static CardModel? GetCookedCanonical(CardModel rawCard) =>
        Recipes.TryGetValue(rawCard.GetType(), out Func<CardModel>? factory) ? factory() : null;
}
