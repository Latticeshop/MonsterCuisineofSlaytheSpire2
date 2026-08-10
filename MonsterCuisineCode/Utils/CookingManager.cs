using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Cards;
using MonsterCuisineCode.Relics;

namespace MonsterCuisineCode.Utils;

/// <summary>
/// 料理系统：根据消耗卡牌的料理参数决定合成哪一项遗物（每次料理只合成一项）。
/// 判定优先级：特殊料理 ＞ 混合料理 ＞ 粘稠度 ＞ 怪物度 ＞ 蜜度 ＞ 鱼度 ＞ 贝度 ＞ 肉度 ＞ 素度。
/// 配方以可扩展的规则表维护，后续新增配方调用 <see cref="RegisterRecipe"/> 即可。
/// </summary>
public static class CookingManager
{
    /// <summary>一次料理消耗的卡牌聚合后的料理参数。</summary>
    public sealed class FoodStatAggregate
    {
        public decimal Viscosity { get; }
        public decimal Plant { get; }
        public decimal Meat { get; }
        public decimal Monster { get; }
        public decimal Honey { get; }
        public decimal Fish { get; }
        public decimal Shell { get; }
        public int SpecialCount { get; }
        public bool HasStone { get; }

        public FoodStatAggregate(
            decimal viscosity,
            decimal plant,
            decimal meat,
            decimal monster,
            decimal honey,
            decimal fish,
            decimal shell,
            int specialCount,
            bool hasStone)
        {
            Viscosity = viscosity;
            Plant = plant;
            Meat = meat;
            Monster = monster;
            Honey = honey;
            Fish = fish;
            Shell = shell;
            SpecialCount = specialCount;
            HasStone = hasStone;
        }

        public static FoodStatAggregate From(IEnumerable<CardModel> cards)
        {
            List<FoodCardModel> foods = cards.OfType<FoodCardModel>().ToList();
            decimal viscosity = 0m, plant = 0m, meat = 0m, monster = 0m;
            decimal honey = 0m, fish = 0m, shell = 0m;
            int special = 0;
            bool hasStone = false;
            foreach (FoodCardModel food in foods)
            {
                if (food is Stone)
                {
                    hasStone = true;
                }

                if (food.FoodStats.IsSpecial)
                {
                    special++;
                    continue;
                }

                viscosity += food.FoodStats.Viscosity;
                plant += food.FoodStats.Plant;
                meat += food.FoodStats.Meat;
                monster += food.FoodStats.Monster;
                honey += food.FoodStats.Honey;
                fish += food.FoodStats.Fish;
                shell += food.FoodStats.Shell;
            }
            return new FoodStatAggregate(
                viscosity, plant, meat, monster, honey, fish, shell, special, hasStone);
        }
    }

    /// <summary>料理配方：满足条件时合成对应遗物。Priority 越小优先级越高。</summary>
    public sealed record CookingRecipe(
        string Id,
        int Priority,
        Func<FoodStatAggregate, bool> Condition,
        Func<RelicModel> RelicFactory,
        string Description);

    /// <summary>配方优先级（数字越小越先判定）。</summary>
    public static class Priority
    {
        public const int Special = 0;   // 特殊料理（烤石子/失败料理）
        public const int Mixed = 1;     // 混合料理（汉堡包）
        public const int Viscosity = 2;
        public const int Monster = 3;
        public const int Honey = 4;
        public const int Fish = 5;
        public const int Shell = 6;
        public const int Meat = 7;
        public const int Plant = 8;
        public const int Fallback = 99; // 失败料理兜底
    }

    private static readonly List<CookingRecipe> RecipesList = new(BuildDefaultRecipes());

    /// <summary>当前全部配方（按优先级排序的只读视图）。</summary>
    public static IReadOnlyList<CookingRecipe> Recipes =>
        RecipesList.OrderBy(recipe => recipe.Priority).ToList();

    /// <summary>扩展 API：注册新配方（会自动按优先级参与判定）。</summary>
    public static void RegisterRecipe(CookingRecipe recipe)
    {
        RecipesList.Add(recipe);
    }

    /// <summary>
    /// 根据消耗的料理卡牌返回合成结果的遗物工厂（每次只合成一项）。
    /// 无任何配方命中时返回失败料理。
    /// </summary>
    public static Func<RelicModel> GetRelicFactory(IEnumerable<CardModel> cards)
    {
        FoodStatAggregate aggregate = FoodStatAggregate.From(cards);
        CookingRecipe? match = RecipesList
            .Where(recipe => recipe.Priority != Priority.Fallback)
            .OrderBy(recipe => recipe.Priority)
            .FirstOrDefault(recipe => recipe.Condition(aggregate));

        if (match != null)
        {
            return match.RelicFactory;
        }

        // 不满足任何要求 → 失败料理
        return () => ModelDb.Relic<FailedDish>();
    }

    private static List<CookingRecipe> BuildDefaultRecipes()
    {
        return new List<CookingRecipe>
        {
            // 特殊料理：石头 + 任意其他食材 → 烤石子
            new CookingRecipe(
                "ROASTED_STONE",
                Priority.Special,
                aggregate => aggregate.HasStone,
                () => ModelDb.Relic<RoastedStone>(),
                "石头+任意食材 → 烤石子"),

            // 特殊料理：一次放入两个（或以上）特殊料理（不含石头）→ 失败料理
            new CookingRecipe(
                "FAILED_TWO_SPECIALS",
                Priority.Special,
                aggregate => aggregate.SpecialCount >= 2,
                () => ModelDb.Relic<FailedDish>(),
                "两个特殊料理 → 失败料理"),

            // 混合料理：素度 2~3.5 且 肉度 2~3.5 → 汉堡包
            new CookingRecipe(
                "HAMBURGER",
                Priority.Mixed,
                aggregate => aggregate.Plant is >= 2m and <= 3.5m &&
                             aggregate.Meat is >= 2m and <= 3.5m,
                () => ModelDb.Relic<Hamburger>(),
                "素度2~3.5且肉度2~3.5 → 汉堡包"),

            // 肉度：仅有肉度且 2~3.5 → 肉丸
            new CookingRecipe(
                "MEATBALL",
                Priority.Meat,
                aggregate => IsOnlyDimension(aggregate, aggregate.Meat) &&
                             aggregate.Meat is >= 2m and <= 3.5m,
                () => ModelDb.Relic<Meatball>(),
                "仅有肉度且2~3.5 → 肉丸"),

            // 肉度：肉度 ≥ 4 → 大锅肉
            new CookingRecipe(
                "BIG_POT_MEAT",
                Priority.Meat,
                aggregate => aggregate.Meat >= 4m,
                () => ModelDb.Relic<BigPotMeat>(),
                "肉度≥4 → 大锅肉"),

            // 兜底：失败料理（不满足任何要求）
            new CookingRecipe(
                "FAILED_FALLBACK",
                Priority.Fallback,
                _ => true,
                () => ModelDb.Relic<FailedDish>(),
                "不满足任何要求 → 失败料理")
        };
    }

    /// <summary>是否只有该维度有数值（其余维度与特殊料理均为 0/无）。</summary>
    private static bool IsOnlyDimension(FoodStatAggregate aggregate, decimal dimension)
    {
        return aggregate.Viscosity == 0m &&
               aggregate.Plant == 0m &&
               aggregate.Monster == 0m &&
               aggregate.Honey == 0m &&
               aggregate.Fish == 0m &&
               aggregate.Shell == 0m &&
               aggregate.SpecialCount == 0 &&
               dimension > 0m;
    }
}
