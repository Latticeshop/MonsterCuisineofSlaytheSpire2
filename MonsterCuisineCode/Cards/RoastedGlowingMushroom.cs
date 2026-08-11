using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>烤发光蘑菇（由发光蘑菇煮成）：抽1张牌，回复7点生命，给予所有敌人1层易伤，消耗。</summary>
public sealed class RoastedGlowingMushroom : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.RoastedGlowingMushroom;

    public override FoodStats FoodStats => FoodStats.Of(plant: 1.5m, monster: 0.5m);

    public RoastedGlowingMushroom() : base(CardType.Skill, CardRarity.Token, TargetType.AllEnemies) { }

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
        var enemies = Owner.Creature.CombatState?.HittableEnemies ?? Array.Empty<Creature>();
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, enemies, Values.Vulnerable, Owner.Creature, this);
    }
}
