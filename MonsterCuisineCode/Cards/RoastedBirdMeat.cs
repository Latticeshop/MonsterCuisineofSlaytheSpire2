using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>烧鸟肉（由鸟肉煮成）：回复7点生命，抽1张牌，消耗。</summary>
public sealed class RoastedBirdMeat : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.RoastedBirdMeat;

    public override FoodStats FoodStats => FoodStats.Of(meat: 2m);

    public RoastedBirdMeat() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Heal", (int)Values.Heal),
        new IntVar("Draw", Values.Draw)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
    }
}
