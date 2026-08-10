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

/// <summary>孢子蘑菇：往弃牌堆加入3张眩晕牌，回复3点生命，抽1张牌。掉落自雾菇。</summary>
public sealed class SporeMushroom : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.SporeMushroom;

    public override FoodStats FoodStats => FoodStats.Of(plant: 2m);

    public SporeMushroom() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("StatusCount", Values.StatusCount),
        new IntVar("Heal", (int)Values.Heal),
        new IntVar("Draw", Values.Draw)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < Values.StatusCount; i++)
        {
            CardModel dazed = Owner.RunState.CreateCard<Dazed>(Owner);
            await CardPileCmd.Add(dazed, PileType.Discard, CardPilePosition.Random, this);
        }
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
    }
}
