namespace MonsterCuisineCode.Cards;

/// <summary>
/// 怪物料理卡牌数值存储类：统一管理所有料理卡的效果数值，避免在卡牌类中硬编码。
/// </summary>
public static class FoodCardValues
{
    public sealed class CardValues
    {
        public int Cost { get; set; } = 0;

        /// <summary>回复生命</summary>
        public decimal Heal { get; set; }

        /// <summary>获得能量</summary>
        public decimal Energy { get; set; }

        /// <summary>抽牌数量</summary>
        public int Draw { get; set; }

        /// <summary>获得力量</summary>
        public decimal Strength { get; set; }

        /// <summary>获得中毒（自身）</summary>
        public decimal Poison { get; set; }

        /// <summary>获得虚弱（自身）</summary>
        public decimal Weak { get; set; }

        /// <summary>获得脆弱（自身）</summary>
        public decimal Frail { get; set; }

        /// <summary>获得易伤（自身）</summary>
        public decimal Vulnerable { get; set; }

        /// <summary>获得再生</summary>
        public decimal Regen { get; set; }

        /// <summary>获得缩小（回合）</summary>
        public decimal Shrink { get; set; }

        /// <summary>自身失去生命</summary>
        public decimal SelfDamage { get; set; }

        /// <summary>往弃牌堆加入的状态牌数量</summary>
        public int StatusCount { get; set; }

        /// <summary>从手牌中选择的卡牌数量</summary>
        public int HandSelectCount { get; set; }
    }

    public static CardValues VanillaJelly => new() { Heal = 1m, Draw = 1 };
    public static CardValues BeadCookie => new() { Energy = 1m, Draw = 1, Shrink = 1m };
    public static CardValues BruteTail => new() { Energy = 1m, Draw = 2, Heal = 3m, Vulnerable = 1m };
    public static CardValues SpikySnakeMeat => new() { Energy = 2m, Draw = 1, Poison = 3m };
    public static CardValues SnakeFruitMeat => new() { Strength = 1m, Poison = 2m };
    public static CardValues KinDumpling => new() { Draw = 1, Energy = 1m, Weak = 1m, Frail = 1m };
    public static CardValues ActiveAcid => new() { Draw = 2 };
    public static CardValues SpikyJelly => new() { Regen = 2m, SelfDamage = 1m, Draw = 1 };
    public static CardValues VineNoodleBundle => new() { Regen = 3m };
    public static CardValues GreenDumpling => new() { StatusCount = 3, Heal = 7m };
    public static CardValues SporeMushroom => new() { StatusCount = 3, Heal = 3m, Draw = 1 };
    public static CardValues GlowingMushroom => new() { Draw = 1, Heal = 5m, Vulnerable = 1m };
    public static CardValues DeerAntler => new() { Heal = 10m };
    public static CardValues Stone => new() { SelfDamage = 10m };
    public static CardValues BirdMeat => new() { Heal = 5m };
    public static CardValues Petroleum => new() { Draw = 1 };
    public static CardValues ActivePetroleum => new() { Draw = 2 };
    public static CardValues NibbitMeat => new() { Strength = 2m, Weak = 1m };

    // ============ 熟食材卡牌（烹饪产物） ============

    public static CardValues GreenLeafStickyBall => new() { Draw = 1, Energy = 1m };
    public static CardValues StickyPearlSoftCake => new() { Energy = 1m, Draw = 1, Shrink = 1m };
    public static CardValues CookedBruteTail => new() { Energy = 1m, Draw = 2, Heal = 3m, Vulnerable = 1m };
    public static CardValues VanillaPudding => new() { Heal = 5m, Draw = 2 };
    public static CardValues RoastedSnakeMeat => new() { Energy = 2m, Draw = 1, Poison = 3m };
    public static CardValues RoastedSourBug => new() { Draw = 4 };
    public static CardValues SoftPudding => new() { Regen = 3m, Draw = 2 };
    public static CardValues VineNoodleSalad => new() { Regen = 3m };
    public static CardValues RoastedGreenDumpling => new() { Poison = 5m, Heal = 7m };
    public static CardValues CookedSporeMushroom => new() { Heal = 3m, Draw = 1, HandSelectCount = 3 };
    public static CardValues CookedSnakeFruit => new() { Strength = 1m };
    public static CardValues RoastedGlowingMushroom => new() { Draw = 1, Heal = 7m, Vulnerable = 1m };
    public static CardValues RoastedDeerAntler => new() { Heal = 10m };
    public static CardValues FireHead => new() { SelfDamage = 99m };
    public static CardValues RoastedBirdMeat => new() { Heal = 7m, Draw = 1 };
    public static CardValues TransformedPetroleum => new() { Draw = 2 };
    public static CardValues AngryPetroleum => new() { Draw = 0 };
}
