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
/// 篝火"烹饪"选项：选择 1 张可烹饪的生食材卡牌，
/// 将其转换为对应的熟食材卡牌（被选中的卡牌被移除，熟食材卡加入牌组）。
/// 每次休息站仅可烹饪一次；选择不足 1 张/取消时不消耗本次机会。
/// </summary>
public sealed class CookIngredientOption : RestSiteOption
{
    /// <summary>独立于"料理"（MC_COOK）的选项 ID。</summary>
    public override string OptionId => "MC_COOK_FOOD";

    public override LocString Description => new("rest_site_ui", "OPTION_MC_COOK_FOOD.description");

    public override bool IsEnabled =>
        PileType.Deck.GetPile(Owner).Cards.Any(card => card is FoodCardModel && CookedFood.CanCook(card));

    public override IEnumerable<string> AssetPaths =>
        ["res://MonsterCuisineResources/image/RestSite/料理.png"];

    public CookIngredientOption(Player owner)
        : base(owner)
    {
    }

    public override async Task<bool> OnSelect()
    {
        CardSelectorPrefs prefs = new(
            new LocString("card_keywords", "mc.cookfood.prompt"), 1, 1);

        List<CardModel> selected = (await CardSelectCmd.FromDeckGeneric(
            Owner, prefs, card => card is FoodCardModel && CookedFood.CanCook(card))).ToList();

        if (selected.Count < 1)
        {
            // 取消/未选择：不消耗本次烹饪机会，选项保留
            return false;
        }

        CardModel rawCard = selected[0];
        CardModel? canonicalCooked = CookedFood.GetCookedCanonical(rawCard);
        if (canonicalCooked == null)
        {
            return false;
        }

        CardModel cookedCard = Owner.RunState.CreateCard(canonicalCooked, Owner);
        await CardPileCmd.Add(cookedCard, PileType.Deck);
        await CardPileCmd.RemoveFromDeck(rawCard);

        ModLogger.Instance.Info(
            $"尖塔乐事烹饪成功：{rawCard.Id.Entry} → {cookedCard.Id.Entry}");
        return true;
    }

    public override Task DoLocalPostSelectVfx(CancellationToken ct = default) =>
        Task.CompletedTask;

    public override Task DoRemotePostSelectVfx() =>
        Task.CompletedTask;
}
