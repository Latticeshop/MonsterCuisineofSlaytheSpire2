using System.Collections.Generic;
using System.Globalization;

namespace MonsterCuisineCode.Utils;

/// <summary>
/// 料理参数管理类：统一管理 Mod 卡牌的四个料理维度。
/// 粘稠度（Viscosity）、素度（Plant）、肉度（Meat）、怪物度（Monster），以及特殊料理标记。
/// </summary>
public sealed class FoodStats
{
    /// <summary>粘稠度</summary>
    public decimal Viscosity { get; init; }

    /// <summary>素度</summary>
    public decimal Plant { get; init; }

    /// <summary>肉度</summary>
    public decimal Meat { get; init; }

    /// <summary>怪物度</summary>
    public decimal Monster { get; init; }

    /// <summary>特殊料理（不参与四维度计算）</summary>
    public bool IsSpecial { get; init; }

    public static FoodStats Special { get; } = new() { IsSpecial = true };

    public static FoodStats Of(
        decimal viscosity = 0m,
        decimal plant = 0m,
        decimal meat = 0m,
        decimal monster = 0m)
    {
        return new FoodStats
        {
            Viscosity = viscosity,
            Plant = plant,
            Meat = meat,
            Monster = monster
        };
    }

    /// <summary>卡牌描述中展示的料理参数行。</summary>
    public string ToDisplayString()
    {
        if (IsSpecial)
        {
            return "特殊料理";
        }

        var parts = new List<string>();
        if (Viscosity != 0m) parts.Add($"粘稠度：{Format(Viscosity)}");
        if (Plant != 0m) parts.Add($"素度：{Format(Plant)}");
        if (Meat != 0m) parts.Add($"肉度：{Format(Meat)}");
        if (Monster != 0m) parts.Add($"怪物度：{Format(Monster)}");
        return string.Join(" ", parts);
    }

    private static string Format(decimal value)
    {
        return value.ToString("0.#", CultureInfo.InvariantCulture);
    }
}
