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

/// <summary>同族团子：抽1张牌，获得1点能量，把这张牌放到抽牌堆顶部；获得1层虚弱与1层脆弱。掉落自同族信徒/同族神官。</summary>
public sealed class KinDumpling : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.KinDumpling;

    public override FoodStats FoodStats => FoodStats.Of(meat: 1m, plant: 1m);

    public KinDumpling() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Draw", Values.Draw),
        new IntVar("Energy", (int)Values.Energy),
        new IntVar("Weak", (int)Values.Weak),
        new IntVar("Frail", (int)Values.Frail)
    ]);

    protected override CardLocation GetResultLocationForCardPlay()
    {
        return new CardLocation(Owner, PileType.Draw, CardPilePosition.Top);
    }

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
        await PlayerCmd.GainEnergy(Values.Energy, Owner);
        await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, Values.Weak, Owner.Creature, this);
        await PowerCmd.Apply<FrailPower>(choiceContext, Owner.Creature, Values.Frail, Owner.Creature, this);
    }
}
