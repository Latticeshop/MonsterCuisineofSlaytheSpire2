using Godot;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Cards;

namespace MonsterCuisineCode.Pools;

/// <summary>
/// 怪物料理共享卡池：料理卡不进入普通奖励池（Token 稀有度），仅通过击杀掉落获得。
/// </summary>
public sealed class MonsterCuisineCardPool : CardPoolModel
{
    public override string Title => "monster_cuisine";
    public override string EnergyColorName => "colorless";
    public override string CardFrameMaterialPath => "card_frame_colorless";
    public override Color DeckEntryCardColor => new Color("A3A3A3FF");
    public override bool IsColorless => true;

    protected override CardModel[] GenerateAllCards() =>
    [
        ModelDb.Card<VanillaJelly>(),
        ModelDb.Card<BeadCookie>(),
        ModelDb.Card<BruteTail>(),
        ModelDb.Card<SpikySnakeMeat>(),
        ModelDb.Card<SnakeFruitMeat>(),
        ModelDb.Card<KinDumpling>(),
        ModelDb.Card<ActiveAcid>(),
        ModelDb.Card<SpikyJelly>(),
        ModelDb.Card<VineNoodleBundle>(),
        ModelDb.Card<GreenDumpling>(),
        ModelDb.Card<SporeMushroom>(),
        ModelDb.Card<GlowingMushroom>(),
        ModelDb.Card<DeerAntler>(),
        ModelDb.Card<Stone>(),
        ModelDb.Card<BirdMeat>(),
        ModelDb.Card<Petroleum>(),
        ModelDb.Card<ActivePetroleum>(),
        ModelDb.Card<NibbitMeat>()
    ];
}
