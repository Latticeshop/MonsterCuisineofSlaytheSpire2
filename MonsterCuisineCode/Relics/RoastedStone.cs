using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace MonsterCuisineCode.Relics;

/// <summary>烤石子（石头+任意食材合成）：每场战斗前三个回合开始时获得7点格挡。</summary>
public sealed class RoastedStone : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => ModelDb.Relic<Pear>().PackedIconPath;
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) &&
            Owner.PlayerCombatState?.TurnNumber is 1 or 2 or 3)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner.Creature, 7m, ValueProp.Unpowered, null, fast: true);
        }
    }
}
