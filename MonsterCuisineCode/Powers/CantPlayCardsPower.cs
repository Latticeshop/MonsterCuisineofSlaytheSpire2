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
/// "下回合不能再出任何牌"能力（鹿茸）。
/// 层数语义：2 = 刚打出（本回合不限制）；1 = 下一回合禁止出牌；回合结束时递减/移除。
/// </summary>
public sealed class CantPlayCardsPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        return card.Owner.Creature != Owner || Amount != 1m;
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

        if (Amount > 1m)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, Owner, null);
        }
        else
        {
            await PowerCmd.Remove(this);
        }
    }
}
