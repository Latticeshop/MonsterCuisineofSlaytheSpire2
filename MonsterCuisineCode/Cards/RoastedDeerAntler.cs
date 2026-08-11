using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>烤鹿茸（由鹿茸煮成，特殊料理）：回复10点生命，消耗。</summary>
public sealed class RoastedDeerAntler : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.RoastedDeerAntler;

    public override FoodStats FoodStats => FoodStats.Special;

    public RoastedDeerAntler() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Heal", (int)Values.Heal)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
    }
}
