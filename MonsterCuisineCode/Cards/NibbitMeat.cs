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

/// <summary>小啃兽肉：获得2点力量，自己获得1层虚弱。掉落自小啃兽。</summary>
public sealed class NibbitMeat : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.NibbitMeat;

    public override FoodStats FoodStats => FoodStats.Of(meat: 1m);

    public NibbitMeat() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Strength", (int)Values.Strength),
        new IntVar("Weak", (int)Values.Weak)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, Values.Strength, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, Values.Weak, Owner.Creature, this);
    }
}
