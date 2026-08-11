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

/// <summary>糯珠软饼（由爆珠饼干煮成）：获得1点能量，抽1张牌，选择一名敌人缩小1回合，消耗。</summary>
public sealed class StickyPearlSoftCake : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.StickyPearlSoftCake;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 1m, meat: 0.5m);

    public StickyPearlSoftCake() : base(CardType.Skill, CardRarity.Token, TargetType.AnyEnemy) { }

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
        if (cardPlay.Target != null)
        {
            await PowerCmd.Apply<ShrinkPower>(choiceContext, cardPlay.Target, Values.Shrink, Owner.Creature, this);
        }
    }
}
