using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>活性酸液：抽2张牌，并随机所有手牌耗能。掉落自毛绒伏地虫。</summary>
public sealed class ActiveAcid : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.ActiveAcid;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 1m, meat: 0.5m);

    public ActiveAcid() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Draw", Values.Draw)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);

        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards
                     .Where(card => card.EnergyCost.Canonical >= 0)
                     .ToList())
        {
            int cost = Owner.RunState.Rng.CombatEnergyCosts.NextInt(4);
            card.EnergyCost.SetThisCombat(cost);
            NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
        }
    }
}
