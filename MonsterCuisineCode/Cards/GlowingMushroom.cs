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

/// <summary>发光蘑菇：抽1张牌，回复5点生命，给予自己1层易伤。掉落自飞蝇菌子。</summary>
public sealed class GlowingMushroom : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.GlowingMushroom;

    public override FoodStats FoodStats => FoodStats.Of(plant: 1m, monster: 0.5m);

    public GlowingMushroom() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Draw", Values.Draw),
        new IntVar("Heal", (int)Values.Heal),
        new IntVar("Vulnerable", (int)Values.Vulnerable)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, Values.Vulnerable, Owner.Creature, this);
    }
}
