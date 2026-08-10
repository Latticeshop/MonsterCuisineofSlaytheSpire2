using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>香草果冻：回复1点生命，抽1张牌。掉落自树叶史莱姆（小/中）。</summary>
public sealed class VanillaJelly : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.VanillaJelly;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 1m, plant: 0.5m);

    public VanillaJelly() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

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
