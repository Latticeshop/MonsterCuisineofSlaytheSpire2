using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MonsterCuisineCode.Powers;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>藤蔓捆面：获得3层再生，手牌中所有攻击牌在本回合耗能+1。掉落自藤蔓蹒跚者。</summary>
public sealed class VineNoodleBundle : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.VineNoodleBundle;

    public override FoodStats FoodStats => FoodStats.Of(plant: 1m);

    public VineNoodleBundle() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Regen", (int)Values.Regen)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, Values.Regen, Owner.Creature, this);
        await PowerCmd.Apply<AttackCostUpPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
