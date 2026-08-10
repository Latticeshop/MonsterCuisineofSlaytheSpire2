using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MonsterCuisineCode.Relics;

/// <summary>大锅肉（肉度 ≥4 合成）：前三个回合开始时多抽1张牌、多获得1点能量。</summary>
public sealed class BigPotMeat : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => ModelDb.Relic<MeatCleaver>().PackedIconPath;
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || Owner.PlayerCombatState?.TurnNumber > 3)
        {
            return count;
        }
        return count + 1m;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) &&
            Owner.PlayerCombatState?.TurnNumber is 1 or 2 or 3)
        {
            Flash();
            await PlayerCmd.GainEnergy(1m, Owner);
        }
    }
}
