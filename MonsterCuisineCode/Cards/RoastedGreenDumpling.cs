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

/// <summary>烤青团（由青团煮成）：给予所有敌人5层中毒，回复7点生命，消耗。</summary>
public sealed class RoastedGreenDumpling : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.RoastedGreenDumpling;

    public override FoodStats FoodStats => FoodStats.Of(viscosity: 2m);

    public RoastedGreenDumpling() : base(CardType.Skill, CardRarity.Token, TargetType.AllEnemies) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Poison", (int)Values.Poison),
        new IntVar("Heal", (int)Values.Heal)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = Owner.Creature.CombatState?.HittableEnemies ?? Array.Empty<Creature>();
        await PowerCmd.Apply<PoisonPower>(
            choiceContext, enemies, Values.Poison, Owner.Creature, this);
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
    }
}
