using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Powers;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>
/// 蛇果肉（能力牌）：每回合开始时获得1点力量、2点中毒；效果触发5次后从牌组移除。
/// 掉落自闪光贾克斯果。
/// </summary>
public sealed class SnakeFruitMeat : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.SnakeFruitMeat;

    public override FoodStats FoodStats => FoodStats.Of(plant: 1m, monster: 0.5m);

    public SnakeFruitMeat() : base(CardType.Power, CardRarity.Token, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Strength", (int)Values.Strength),
        new IntVar("Poison", (int)Values.Poison)
    ]);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SnakeFruitMeatPower? power = await PowerCmd.Apply<SnakeFruitMeatPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (power != null)
        {
            power.SourceDeckCard = DeckVersion as FoodCardModel;
        }
    }

    protected override Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;
}
