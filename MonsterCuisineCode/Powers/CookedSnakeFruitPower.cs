using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MonsterCuisineCode.Cards;

namespace MonsterCuisineCode.Powers;

/// <summary>
/// 熟蛇果能力：每回合开始时获得1点力量；
/// 效果触发累计5次后，从牌组移除对应料理卡并移除本能力。
/// </summary>
public sealed class CookedSnakeFruitPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>牌组中的原卡（战斗卡通过 DeckVersion 关联）。</summary>
    public FoodCardModel? SourceDeckCard { get; set; }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Side || !participants.Contains(Owner))
        {
            return;
        }

        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(ctx, Owner, 1m, Owner, null);

        if (SourceDeckCard == null)
        {
            return;
        }

        SourceDeckCard.PlaysUsed++;
        SourceDeckCard.RefreshPlaysRemainingDisplay();
        if (SourceDeckCard.PlaysUsed >= FoodCardModel.PlaysToRemoveCount)
        {
            await CardPileCmd.RemoveFromDeck(SourceDeckCard);
            await PowerCmd.Remove(this);
        }
    }
}
