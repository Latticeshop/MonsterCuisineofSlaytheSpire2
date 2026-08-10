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

/// <summary>爆珠饼干：获得1点能量，抽1张牌，获得1回合缩小。掉落自缩小甲虫。</summary>
public sealed class BeadCookie : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.BeadCookie;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 1m, meat: 0.5m);

    public BeadCookie() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Energy", (int)Values.Energy),
        new IntVar("Draw", Values.Draw),
        new IntVar("Shrink", (int)Values.Shrink)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(Values.Energy, Owner);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
        await PowerCmd.Apply<ShrinkPower>(choiceContext, Owner.Creature, Values.Shrink, Owner.Creature, this);
    }
}
