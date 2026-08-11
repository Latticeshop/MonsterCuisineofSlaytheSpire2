using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>生气石油（由活性石油煮成，特殊料理）：抽0张牌（不抽牌），消耗。</summary>
public sealed class AngryPetroleum : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.AngryPetroleum;

    public override FoodStats FoodStats => FoodStats.Special;

    public AngryPetroleum() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Draw", Values.Draw)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
    }
}
