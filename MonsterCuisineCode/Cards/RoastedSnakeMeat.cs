using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>烤蛇肉（由带刺蛇肉煮成）：获得2点能量，抽1张牌，给予一名敌人3层中毒，消耗。</summary>
public sealed class RoastedSnakeMeat : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.RoastedSnakeMeat;

    public override FoodStats FoodStats => FoodStats.Of(meat: 1m, monster: 0.5m);

    public RoastedSnakeMeat() : base(CardType.Skill, CardRarity.Token, TargetType.AnyEnemy) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Energy", (int)Values.Energy),
        new IntVar("Draw", Values.Draw),
        new IntVar("Poison", (int)Values.Poison)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(Values.Energy, Owner);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
        if (cardPlay.Target != null)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, Values.Poison, Owner.Creature, this);
        }
    }
}
