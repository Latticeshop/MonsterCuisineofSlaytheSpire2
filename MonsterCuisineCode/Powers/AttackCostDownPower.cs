using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace MonsterCuisineCode.Powers;

/// <summary>
/// "手牌中所有攻击牌在本回合耗能-1"能力（藤蔓拌面）。
/// 通过战斗内费用修正钩子实现，玩家方回合结束时移除。
/// </summary>
public sealed class AttackCostDownPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        if (card.Owner == Owner.Player && card.Type == CardType.Attack)
        {
            modifiedCost = originalCost - 1m;
            return true;
        }

        modifiedCost = originalCost;
        return false;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side || !participants.Contains(Owner))
        {
            return;
        }

        await PowerCmd.Remove(this);
    }
}
