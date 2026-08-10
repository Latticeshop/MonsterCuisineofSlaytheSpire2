using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>带刺果冻：获得2层再生，失去1点生命，抽1张牌。掉落自树枝史莱姆（中/小）。</summary>
public sealed class SpikyJelly : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.SpikyJelly;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 1m);

    public SpikyJelly() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Regen", (int)Values.Regen),
        new IntVar("SelfDamage", (int)Values.SelfDamage),
        new IntVar("Draw", Values.Draw)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, Values.Regen, Owner.Creature, this);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, Values.SelfDamage, ValueProp.Unblockable, this, cardPlay);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
    }
}
