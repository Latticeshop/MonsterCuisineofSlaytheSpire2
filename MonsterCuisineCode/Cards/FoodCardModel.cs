using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MonsterCuisineCode.RestSite;
using MonsterCuisineCode.Utils;

namespace MonsterCuisineCode.Cards;

/// <summary>
/// 怪物料理卡牌基类。
/// 统一处理：
/// 1. 料理参数（粘稠度/素度/肉度/怪物度/特殊料理）展示；
/// 2. 打出 5 次后从牌组移除（次数随存档持久化）；
/// 3. 休息站"料理"选项注册（牌组中存在料理卡时出现）。
/// </summary>
public abstract class FoodCardModel : CardModel
{
    public const int PlaysToRemoveCount = 5;

    protected FoodCardModel(CardType type, CardRarity rarity, TargetType target)
        : base(0, type, rarity, target)
    {
    }

    /// <summary>本卡牌的料理参数。</summary>
    public abstract FoodStats FoodStats { get; }

    /// <summary>已打出次数（随存档持久化，达到 5 次后从牌组移除）。</summary>
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int PlaysUsed { get; set; }

    /// <summary>距离移除还剩多少次（用于卡牌描述动态展示）。</summary>
    public int PlaysRemaining => System.Math.Max(0, PlaysToRemoveCount - PlaysUsed);

    /// <summary>暂不配置卡图，回退到游戏默认卡图。</summary>
    public override string PortraitPath => ModelDb.Card<Infection>().PortraitPath;

    /// <summary>基础动态变量：料理参数展示 + 移除次数。</summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new StringVar("Stats", FoodStats.ToDisplayString()),
        new IntVar("PlaysRemaining", PlaysRemaining)
    };

    public override void AfterCreated()
    {
        base.AfterCreated();
        RefreshPlaysRemainingDisplay();
    }

    protected override void AfterDeserialized()
    {
        base.AfterDeserialized();
        RefreshPlaysRemainingDisplay();
    }

    /// <summary>把"剩余次数"同步到卡牌描述的动态变量，使描述实时显示（如 5→4→3…）。</summary>
    public void RefreshPlaysRemainingDisplay()
    {
        if (DynamicVars != null &&
            DynamicVars.TryGetValue("PlaysRemaining", out DynamicVar? variable) &&
            variable is IntVar intVar)
        {
            intVar.BaseValue = PlaysRemaining;
        }
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }

        if (!options.Any(option => option is CookingRestSiteOption))
        {
            options.Add(new CookingRestSiteOption(player));
        }
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayEffect(choiceContext, cardPlay);
        await CountPlayAndRemoveIfNeeded(DeckVersion as FoodCardModel, this);
    }

    /// <summary>子类实现卡牌的具体效果。</summary>
    protected abstract Task PlayEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay);

    /// <summary>
    /// 计数一次"打出/触发"，达到 <see cref="PlaysToRemoveCount"/> 次后从牌组移除。
    /// 战斗卡是牌组卡的克隆，通过 <see cref="CardModel.DeckVersion"/> 定位牌组原卡。
    /// </summary>
    public static async Task CountPlayAndRemoveIfNeeded(
        FoodCardModel? deckCard,
        FoodCardModel? combatCard = null)
    {
        if (deckCard == null)
        {
            return;
        }

        deckCard.PlaysUsed++;
        deckCard.RefreshPlaysRemainingDisplay();
        combatCard?.RefreshPlaysRemainingDisplay();
        if (deckCard.PlaysUsed >= PlaysToRemoveCount)
        {
            await CardPileCmd.RemoveFromDeck(deckCard);
        }
    }
}
