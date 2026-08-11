using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>火头（由石头煮成，特殊料理）：自己失去99点生命，消耗。</summary>
public sealed class FireHead : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.FireHead;

    public override FoodStats FoodStats => FoodStats.Special;

    public FireHead() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("SelfDamage", (int)Values.SelfDamage)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, Values.SelfDamage, ValueProp.Unblockable, this, cardPlay);
    }
}
