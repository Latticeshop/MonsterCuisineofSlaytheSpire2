using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>活性石油（特殊料理）：抽2张牌。掉落自墨影幻灵。</summary>
public sealed class ActivePetroleum : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.ActivePetroleum;

    public override FoodStats FoodStats => FoodStats.Special;

    public ActivePetroleum() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Draw", Values.Draw)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
    }
}
