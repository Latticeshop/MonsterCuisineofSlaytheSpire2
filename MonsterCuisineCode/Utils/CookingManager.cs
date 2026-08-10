using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Cards;
using MonsterCuisineCode.Relics;

namespace MonsterCuisineCode.Utils;

/// <summary>
/// 料理系统：根据消耗卡牌的四维度料理参数（粘稠度/素度/肉度/怪物度）决定获得的遗物。
/// 配方以可扩展的规则表形式维护，后续新增配方只需调用 <see cref="RegisterRecipe"/>。
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
        public int SpecialCount { get; }

        public FoodStatAggregate(
            decimal viscosity,
            decimal plant,
            decimal meat,
            decimal monster,
            int specialCount)
        {
            Viscosity = viscosity;
            Plant = plant;
            Meat = meat;
            Monster = monster;
            SpecialCount = specialCount;
        }

        public static FoodStatAggregate From(IEnumerable<CardModel> cards)
        {
            List<FoodCardModel> foods = cards.OfType<FoodCardModel>().ToList();
            decimal viscosity = 0m, plant = 0m, meat = 0m, monster = 0m;
            int special = 0;
            foreach (FoodCardModel food in foods)
            {
                if (food.FoodStats.IsSpecial)
                {
                    special++;
                    continue;
                }
                viscosity += food.FoodStats.Viscosity;
                plant += food.FoodStats.Plant;
                meat += food.FoodStats.Meat;
                monster += food.FoodStats.Monster;
            }
            return new FoodStatAggregate(viscosity, plant, meat, monster, special);
        }
    }

    /// <summary>料理配方：满足条件时奖励对应遗物。</summary>
    public sealed record CookingRecipe(
        string Id,
        Func<FoodStatAggregate, bool> Condition,
        Type RelicType,
        string Description);

    private static readonly List<CookingRecipe> RecipesList = new(BuildDefaultRecipes());

    /// <summary>当前全部配方（只读视图）。</summary>
    public static IReadOnlyList<CookingRecipe> Recipes => RecipesList;

    /// <summary>扩展 API：注册新配方。</summary>
    public static void RegisterRecipe(CookingRecipe recipe)
    {
        RecipesList.Add(recipe);
    }

    /// <summary>根据消耗的料理卡牌，返回应获得的遗物类型列表（按配方顺序）。</summary>
    public static IEnumerable<Type> GetRelicTypes(IEnumerable<CardModel> cards)
    {
        FoodStatAggregate aggregate = FoodStatAggregate.From(cards);
        return RecipesList
            .Where(recipe => recipe.Condition(aggregate))
            .Select(recipe => recipe.RelicType);
    }

    private static List<CookingRecipe> BuildDefaultRecipes()
    {
        return new List<CookingRecipe>
        {
            new CookingRecipe(
                "VEGAN_FEAST",
                aggregate => aggregate.Plant >= 2m,
                typeof(VeganSalad),
                "素度≥2 → 素食沙拉"),
            new CookingRecipe(
                "MEAT_FEAST",
                aggregate => aggregate.Meat >= 2m,
                typeof(MeatFeast),
                "肉度≥2 → 肉食大餐"),
            new CookingRecipe(
                "VISCOSITY_DESSERT",
                aggregate => aggregate.Viscosity >= 2m,
                typeof(ViscousDessert),
                "粘稠度≥2 → 粘稠甜点"),
            new CookingRecipe(
                "DARK_DISH",
                aggregate => aggregate.Monster >= 1m,
                typeof(DarkDish),
                "怪物度≥1 → 黑暗料理"),
            new CookingRecipe(
                "SECRET_DISH",
                aggregate => aggregate.SpecialCount >= 1,
                typeof(SecretDish),
                "特殊料理≥1 → 秘制料理")
        };
    }
}
