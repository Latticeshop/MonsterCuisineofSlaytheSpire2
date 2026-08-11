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
/// 熟蛇果（由蛇果肉煮成，能力牌）：每回合开始时获得1点力量；效果触发5次后从牌组移除。
/// </summary>
public sealed class CookedSnakeFruit : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.CookedSnakeFruit;

    public override FoodStats FoodStats => FoodStats.Of(plant: 1m, monster: 0.5m);

    public CookedSnakeFruit() : base(CardType.Power, CardRarity.Token, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Strength", (int)Values.Strength)
    ]);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CookedSnakeFruitPower? power = await PowerCmd.Apply<CookedSnakeFruitPower>(
            choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (power != null)
        {
            power.SourceDeckCard = DeckVersion as FoodCardModel;
        }
    }

    protected override Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;
}
