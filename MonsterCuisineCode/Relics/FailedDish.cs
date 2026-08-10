using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace MonsterCuisineCode.Relics;

/// <summary>失败料理（食材不满足任何要求，或一次放入两个特殊料理）：没有效果。</summary>
public sealed class FailedDish : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override string PackedIconPath => ModelDb.Relic<Vajra>().PackedIconPath;
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
}
