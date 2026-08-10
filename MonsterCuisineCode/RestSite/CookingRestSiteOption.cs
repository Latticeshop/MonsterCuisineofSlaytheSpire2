using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MonsterCuisineCode.Cards;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.RestSite;

/// <summary>
/// 篝火"料理"选项：选择 0-任意张本 Mod 的怪物料理卡牌，
/// 根据料理参数（粘稠度/素度/肉度/怪物度/特殊料理）获得对应遗物，并移除被选中的卡牌。
/// 选择 2-4 张；不消耗休息次数（与休息/锻造等原版选项独立）；每次休息站仅可料理一次。
/// </summary>
public sealed class CookingRestSiteOption : RestSiteOption
{
    /// <summary>独立于原版选项的 ID：标题取"料理"，图标由补丁复用游戏的烹饪图标。</summary>
    public override string OptionId => "MC_COOK";

    public override LocString Description => new("rest_site_ui", "OPTION_MC_COOK.description");

    public override bool IsEnabled =>
        PileType.Deck.GetPile(Owner).Cards.Count(card => card is FoodCardModel) >= 2;

    public override IEnumerable<string> AssetPaths =>
        ["res://MonsterCuisineResources/image/RestSite/料理.png"];

    public CookingRestSiteOption(Player owner)
        : base(owner)
    {
    }

    public override async Task<bool> OnSelect()
    {
        CardSelectorPrefs prefs = new(
            new LocString("card_keywords", "mc.cook.prompt"), 2, 4);

        List<CardModel> selected = (await CardSelectCmd.FromDeckGeneric(
            Owner, prefs, card => card is FoodCardModel)).ToList();

        if (selected.Count < 2)
        {
            // 取消/未选够 2 张：不消耗本次料理机会，选项保留
            return false;
        }

        RelicModel canonicalRelic = CookingManager.GetRelicFactory(selected)();
        Type relicType = canonicalRelic.GetType();
        if (!Owner.Relics.Any(relic => relic.GetType() == relicType))
        {
            await RelicCmd.Obtain(canonicalRelic.ToMutable(), Owner);
        }

        await CardPileCmd.RemoveFromDeck(selected);
        ModLogger.Instance.Info(
            $"尖塔乐事料理成功：{selected.Count} 张卡 → 遗物 {relicType.Name}");
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) =>
        Task.CompletedTask;

    public override Task DoRemotePostSelectVfx() =>
        Task.CompletedTask;
}
