using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>青叶糯丸（由同族团子煮成）：抽1张牌，获得1点能量，把这张牌放到抽牌堆顶部。无消耗。</summary>
public sealed class GreenLeafStickyBall : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.GreenLeafStickyBall;

    public override FoodStats FoodStats => FoodStats.Of(meat: 1m, plant: 1m);

    public GreenLeafStickyBall() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Draw", Values.Draw),
        new IntVar("Energy", (int)Values.Energy)
    ]);

    protected override CardLocation GetResultLocationForCardPlay()
    {
        return new CardLocation(Owner, PileType.Draw, CardPilePosition.Top);
    }

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
        await PlayerCmd.GainEnergy(Values.Energy, Owner);
    }
}
