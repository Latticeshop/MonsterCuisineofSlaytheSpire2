using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MonsterCuisineCode.Relics;

/// <summary>汉堡包（素度 2~3.5 且 肉度 2~3.5 合成）：每场战斗开始时获得3层再生；第一回合多获得1点能量、多抽1张牌。</summary>
public sealed class Hamburger : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => ModelDb.Relic<Bread>().PackedIconPath;
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || Owner.PlayerCombatState?.TurnNumber > 1)
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
        if (!participants.Contains(Owner.Creature) ||
            Owner.PlayerCombatState?.TurnNumber != 1)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<RegenPower>(
            new ThrowingPlayerChoiceContext(), Owner.Creature, 3m, Owner.Creature, null);
        await PlayerCmd.GainEnergy(1m, Owner);
    }
}
