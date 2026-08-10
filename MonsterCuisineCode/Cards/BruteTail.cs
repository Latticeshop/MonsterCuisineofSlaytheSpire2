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

/// <summary>蛮兽尾巴：获得1点能量，抽2张牌，回复3点生命，获得1层易伤。掉落自蛮兽。</summary>
public sealed class BruteTail : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.BruteTail;

    public override FoodStats FoodStats => FoodStats.Of(meat: 1.5m);

    public BruteTail() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Energy", (int)Values.Energy),
        new IntVar("Draw", Values.Draw),
        new IntVar("Heal", (int)Values.Heal),
        new IntVar("Vulnerable", (int)Values.Vulnerable)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(Values.Energy, Owner);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, Values.Vulnerable, Owner.Creature, this);
    }
}
