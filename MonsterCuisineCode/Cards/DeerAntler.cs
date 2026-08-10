using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Powers;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>鹿茸（特殊料理）：回复10点生命，下回合不能再出任何牌。掉落自仪式兽。</summary>
public sealed class DeerAntler : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.DeerAntler;

    public override FoodStats FoodStats => FoodStats.Special;

    public DeerAntler() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Heal", (int)Values.Heal)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
        await PowerCmd.Apply<CantPlayCardsPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
    }
}
