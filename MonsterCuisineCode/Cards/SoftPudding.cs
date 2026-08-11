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

/// <summary>软糯布丁（由带刺果冻煮成）：获得3层再生，抽2张牌，消耗。</summary>
public sealed class SoftPudding : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.SoftPudding;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 1m);

    public SoftPudding() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Regen", (int)Values.Regen),
        new IntVar("Draw", Values.Draw)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, Values.Regen, Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
    }
}
