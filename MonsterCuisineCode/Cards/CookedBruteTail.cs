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

/// <summary>熟蛮兽尾巴（由蛮兽尾巴煮成）：给予所有敌人1层易伤，抽2张牌，获得1点能量，回复3点生命，消耗。</summary>
public sealed class CookedBruteTail : FoodCardModel
{
    private static readonly FoodCardValues.CardValues Values = FoodCardValues.CookedBruteTail;

    public override FoodStats FoodStats => FoodStats.Of(meat: 1.5m);

    public CookedBruteTail() : base(CardType.Skill, CardRarity.Token, TargetType.AllEnemies) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => base.CanonicalVars.Concat(
    [
        new IntVar("Vulnerable", (int)Values.Vulnerable),
        new IntVar("Draw", Values.Draw),
        new IntVar("Energy", (int)Values.Energy),
        new IntVar("Heal", (int)Values.Heal)
    ]);

    protected override async Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = Owner.Creature.CombatState?.HittableEnemies ?? Array.Empty<Creature>();
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, enemies, Values.Vulnerable, Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, Values.Draw, Owner);
        await PlayerCmd.GainEnergy(Values.Energy, Owner);
        await CreatureCmd.Heal(Owner.Creature, Values.Heal);
    }
}
