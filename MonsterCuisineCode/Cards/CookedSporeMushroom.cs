using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>熟孢子蘑菇（由孢子蘑菇煮成）：回复3点生命，抽1张牌，选择手牌中3张牌，为其添加虚无，消耗。</summary>
public sealed class CookedSporeMushroom : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.CookedSporeMushroom;

    public override FoodStats FoodStats => FoodStats.Of(plant: 1.5m);

    public CookedSporeMushroom() : base(CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Heal", (int)Values.Heal),
        new IntVar("Draw", Values.Draw),
        new IntVar("HandSelectCount", Values.HandSelectCount)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);

        var prefs = new CardSelectorPrefs(
            new LocString("card_keywords", "mc.cookfood.hand_ethereal.prompt"),
            Values.HandSelectCount, Values.HandSelectCount);

        List<CardModel> selected = (await CardSelectCmd.FromHand(
            choiceContext, Owner, prefs,
            card => !card.Keywords.Contains(CardKeyword.Ethereal),
            this)).ToList();

        foreach (CardModel card in selected)
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
        }
    }
}
