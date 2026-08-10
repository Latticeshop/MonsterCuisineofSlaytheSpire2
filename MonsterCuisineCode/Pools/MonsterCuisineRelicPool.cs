using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Relics;

namespace MonsterCuisineCode.Pools;

/// <summary>
/// 怪物料理遗物池：料理遗物为 Event 稀有度，不进入普通遗物奖励，仅通过篝火料理获得。
/// </summary>
public sealed class MonsterCuisineRelicPool : RelicPoolModel
{
    public override string EnergyColorName => "colorless";

    protected override IEnumerable<RelicModel> GenerateAllRelics() => new RelicModel[]
    {
        ModelDb.Relic<VeganSalad>(),
        ModelDb.Relic<MeatFeast>(),
        ModelDb.Relic<ViscousDessert>(),
        ModelDb.Relic<DarkDish>(),
        ModelDb.Relic<SecretDish>()
    };
}
