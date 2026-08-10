using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>青团：往自己的弃牌堆加入3张感染牌，回复7点生命。掉落自异蛙寄生虫。</summary>
public sealed class GreenDumpling : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.GreenDumpling;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 2m);

    public GreenDumpling() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("StatusCount", Values.StatusCount),
        new IntVar("Heal", (int)Values.Heal)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < Values.StatusCount; i++)
        {
            CardModel infection = Owner.RunState.CreateCard<Infection>(Owner);
            await CardPileCmd.Add(infection, PileType.Discard, CardPilePosition.Random, this);
        }
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
    }
}
