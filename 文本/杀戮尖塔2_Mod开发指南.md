# 杀戮尖塔2 Mod 开发指南

>
> **适用版本**：Beta 版｜Godot 4.5.1（Megadot 分支）｜.NET 9.0｜C# 13
>
> **模式范围**：当前仅面向单机模式，本文档已移除联机（多人）相关内容。

---

## 📚 目录

1. [版本差异速览（Beta vs 正式版）](#1-版本差异速览beta-vs-正式版)
2. [环境搭建](#2-环境搭建)
3. [自定义卡牌（CardModel）](#3-自定义卡牌cardmodel)
4. [自定义词条（Custom Keywords）](#4-自定义词条custom-keywords)
5. [卡牌悬浮提示（HoverTip）](#5-卡牌悬浮提示hovertip)
6. [自定义遗物（RelicModel）](#6-自定义遗物relicmodel)
7. [自定义药水（PotionModel）](#7-自定义药水potionmodel)
8. [卡牌附魔（EnchantmentModel）](#8-卡牌附魔enchantmentmodel)
9. [自定义能力（PowerModel）](#9-自定义能力powermodel)
10. [自定义事件（EventModel）](#10-自定义事件eventmodel)
11. [自定义角色（CharacterModel）](#11-自定义角色charactermodel)
12. [自定义敌怪（MonsterModel + EncounterModel）](#12-自定义敌怪monstermodel--encountermodel)
13. [攻击特效（VFX）](#13-攻击特效vfx)
14. [DamageVar 与增伤机制](#14-damagevar-与增伤机制)
15. [本地化键名规则](#15-本地化键名规则)
16. [UI 选择页面本地化配置](#16-ui-选择页面本地化配置)
17. [控制台命令](#17-控制台命令)
18. [关键 API 速查](#18-关键-api-速查)
19. [开发最佳实践](#19-开发最佳实践)
20. [快速开始检查清单](#20-快速开始检查清单)
21. [附录：游戏解包资源结构](#21-附录游戏解包资源结构)

---

## 1. 版本差异速览（Beta vs 正式版）

| 模块 | 正式版 | Beta版 | 注意事项 |
|------|--------|--------|---------|
| 卡牌去向方法 | `GetResultPileTypeForCardPlay()` | `GetResultLocationForCardPlay()` | 返回值从 `PileType` 改为 `CardLocation` |
| 攻击卡 FromCard | `FromCard(CardModel)` | `FromCard(CardModel, CardPlay?)` | 新增 `cardPlay` 参数 |
| CardPlay 构造 | 可选 Player | **必填** `Player` | `CardPlay.Player` 为必填成员 |
| Targeting 参数 | 接受 `List<Creature>` | 单个 `Creature` 或 `TargetingAllOpponents()` | 群体攻击需改用新API |

> 以下 API 详解以 Beta 版为准；移植旧代码时需重点关注 [3.10 Beta 版 API 变化详解](#310-beta-版-api-变化详解)。

---

## 2. 环境搭建

### 2.1 Mod 的基本构成

一个完整的《杀戮尖塔2》Mod 由三个同名文件组成，必须放在游戏 `mods` 目录下的同一级文件夹中：

| 文件类型 | 扩展名 | 必需性 | 说明 |
|---------|--------|--------|------|
| 模组清单文件 | `.json` | ✅ 必需 | Mod 的"身份证" |
| 资源包文件 | `.pck` | ⚠️ 可选 | 包含图片、场景等资源 |
| 代码程序集 | `.dll` | ⚠️ 可选 | 包含 C# 逻辑代码 |

**示例结构：**
```
mods/MyCustomMod/
├── MyCustomMod.json
├── MyCustomMod.pck
└── MyCustomMod.dll
```

### 2.2 模组清单文件（.json）

```json
{
  "id": "MyCustomMod",
  "name": "我的自定义模组",
  "author": "作者名",
  "description": "模组描述",
  "version": "v1.0.0",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [],
  "affects_gameplay": true
}
```

**关键字段说明：**
- `has_pck` / `has_dll`: 如实声明是否包含资源包或代码
- `affects_gameplay`: 若影响游戏玩法（添加卡牌、角色等），必须设为 `true`
- `dependencies`: 依赖的其他 Mod ID 列表

### 2.3 环境与工具要求

| 项目 | 要求 |
|------|------|
| .NET 版本 | **.NET 9.0**（必需） |
| C# 语言版本 | C# 13 |
| Godot 编辑器 | **Megadot** 分支（https://megadot.megacrit.com） |
| IDE 推荐 | Visual Studio 或 JetBrains Rider |

⚠️ **重要**：必须使用 Megadot 而非标准 Godot 版本，以确保兼容性。

### 2.4 创建 Mod 项目步骤

1. **新建 Godot 项目**
   - 使用 Megadot 编辑器创建新项目
   - 项目名建议与 Mod ID 一致（英文）

2. **推荐目录结构模板**（以 `<ModID>` 为项目名占位）
   ```
   <ModID>/
   ├── .idea/                      # IDE配置（自动生成）
   ├── libs/                       # 依赖库
   │   ├── sts2.dll
   │   └── 0Harmony.dll
   ├── <ModID>Code/                # C#源代码
   │   ├── Cards/
   │   ├── Relics/
   │   ├── Potions/
   │   ├── Powers/
   │   ├── Characters/
   │   ├── Monsters/
   │   ├── Events/
   │   ├── Enchantments/
   │   ├── Utils/                  # 工具类（词条、数值存储等）
   │   └── ModInitializer.cs       # Mod入口
   ├── <ModID>Resources/           # Godot资源
   │   ├── images/
   │   │   ├── character/
   │   │   ├── charui/
   │   │   ├── packed/
   │   │   │   ├── card_portraits/
   │   │   │   └── character_select/
   │   │   ├── powers/
   │   │   ├── relics/
   │   │   └── potions/
   │   ├── scenes/
   │   │   ├── creature_visuals/
   │   │   ├── encounters/
   │   │   └── ui/
   │   └── localization/zhs/
   │       ├── cards.json
   │       ├── relics.json
   │       └── ...
   ├── localization/zhs/           # 本地化文件（游戏读取路径）
   ├── build/                      # 构建输出
   │   ├── <ModID>.json
   │   ├── <ModID>.pck
   │   └── <ModID>.dll
   ├── <ModID>.csproj              # C#项目文件
   ├── <ModID>.sln                 # 解决方案文件
   └── project.godot               # Godot项目配置
   ```

3. **目录职责说明**

   | 目录 | 职责 |
   |------|------|
   | `<ModID>Code/` | C# 源代码，包含角色、卡牌、遗物等逻辑 |
   | `<ModID>Resources/` | Godot 资源（图片、场景、本地化） |
   | `localization/zhs/` | 游戏读取的本地化文件路径 |
   | `build/` | 构建输出目录 |
   | `libs/` | 游戏 DLL 依赖 |

4. **添加依赖库**
   - 从游戏安装目录的 `data_sts2_<platform>` 文件夹复制：
     - `sts2.dll`
     - `0Harmony.dll`
   - 放入项目的 `libs/` 文件夹
   - 在 IDE 中引用这两个 DLL

5. **配置 .csproj 文件**
   ```xml
   <Project Sdk="Godot.NET.Sdk/4.5.1">
     <PropertyGroup>
       <TargetFramework>net9.0</TargetFramework>
       <EnableDynamicLoading>true</EnableDynamicLoading>
     </PropertyGroup>
     <ItemGroup>
       <Reference Include="0Harmony">
         <HintPath>libs/0Harmony.dll</HintPath>
       </Reference>
       <Reference Include="sts2">
         <HintPath>libs/sts2.dll</HintPath>
       </Reference>
     </ItemGroup>
   </Project>
   ```

6. **创建 Mod 入口类**
   ```csharp
   using MegaCrit.Sts2.Core.Modding;

   [ModInitializer(nameof(Initialize))]
   public static class MyModInitializer
   {
       public static void Initialize()
       {
           // 初始化代码
           Log.Info("Mod加载成功");

           // 注册模型到池
           ModHelper.AddModelToPool(typeof(SharedRelicPool), typeof(MyCustomRelic));

           // Harmony 补丁
           var harmony = new Harmony("Author.ModID");
           harmony.PatchAll();
       }
   }
   ```

### 2.5 资源包（.pck）制作

1. **放置资源**
   - 按与游戏本体相同的相对路径放置资源
   - 例如：`res://images/ui/...`

2. **导出 PCK**
   - 菜单：`项目 > 导出`
   - 添加 Windows 导出方案
   - 点击 **导出 PCK/ZIP**
   - 命名为 `<Mod ID>.pck`
   - ❌ 取消"使用调试导出"
   - ❌ 取消"导出为补丁"

### 2.6 游戏日志路径

遇到问题时，第一个要查看的就是游戏日志文件：

**日志位置**：
```
C:\Users\<你的用户名>\AppData\Roaming\SlayTheSpire2\logs\
```

**重要日志文件**：
- `godot.log` - 游戏启动和运行时的详细日志，包含所有错误信息
- 当游戏无法启动或 Mod 加载失败时，这里会告诉你具体哪里出了问题

### 2.7 常见问题

**问题1：Mod 完全无法加载 - 致命错误**

**症状**：游戏启动时报错，Mod 的 DLL 或 PCK 文件找不到

**原因**：这是最常见的致命错误！JSON 配置文件中的 `"id"` 字段必须与文件名完全一致！

```json
// MyMod.json
{
  "id": "MyMod",  // ← 这个ID
  "name": "我的Mod",
  ...
}
```

**必须确保三个文件同名**：
- `MyMod.json`（ID必须匹配）
- `MyMod.dll`
- `MyMod.pck`

如果你把 ID 改成 `"OtherName"`，那么文件名也必须是 `OtherName.json`、`OtherName.dll`、`OtherName.pck`，否则游戏根本无法找到你的 Mod 文件！

**解决步骤**：
1. 打开 `godot.log` 查看具体报错信息
2. 检查 JSON 中的 `"id"` 字段
3. 确保三个文件名完全一致（包括大小写）
4. 重新复制到游戏 mods 文件夹

---

**问题2：角色资源显示为空白**

**症状**：角色已经能在选择页面看到，但图标、立绘、背景图显示为空白

**原因**：Godot 场景文件中的 `Sprite2D` 节点没有设置 `texture` 属性

**解决步骤**：
1. 在 Godot 编辑器中打开你的场景文件（如 `my_character.tscn`、`my_character_icon.tscn`、`my_character_bg.tscn`）
2. 在场景树中选中 `Sprite2D` 节点（通常叫 `Visuals`、`Bg`、`CharacterIconCharName` 等）
3. 在右侧的"检查器"面板找到 **Texture** 属性
4. 点击 Texture 旁边的下拉箭头，选择 **快速加载**（Quick Load）
5. 在弹出的文件浏览器中选择对应的 PNG 图片
6. 保存场景（Ctrl+S），然后重新导出 PCK

**正确的场景文件应该长这样**：
```gdscript
[node name="Visuals" type="Sprite2D" parent="."]
position = Vector2(0, -150)
texture = ExtResource("1_char")  # ← 必须有这一行！

[ext_resource type="Texture2D" path="res://images/character/my_character.png" id="1_char"]
```

如果缺少 `texture = ExtResource(...)` 这行，场景就无法显示图片！

---

**问题3：PlatformNotSupportedException**
- **原因**：.NET 版本不匹配
- **解决**：修改 Megadot 编辑器目录下的 `GodotPlugins.runtimeconfig.json`，将 `version` 强制改为 `9.0.0`

**问题4：脚本无法找到**
- 在 `Initialize` 中调用：
  ```csharp
  Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
  ```

**问题5：Mod 未被加载**
- 检查三个文件是否同名且在同一目录
- 检查 JSON 中的 `has_pck` / `has_dll` 是否与实际文件相符

**问题6：资源不显示（其他原因）**
- 确认 PCK 内资源路径与游戏原版完全一致（包括大小写）
- 验证裁切纹理（`.tres`）是否存在

---

## 3. 自定义卡牌（CardModel）

### 3.1 卡牌构造函数

```csharp
public MyCustomCard() : base(energyCost, cardType, cardRarity, targetType, shouldShowInCardLibrary)
```

**参数说明：**

| 参数 | 类型 | 说明 |
|------|------|------|
| energyCost | int | 基础能量消耗 |
| cardType | CardType | Attack, Skill, Power, Status, Curse, Quest |
| cardRarity | CardRarity | 决定卡牌稀有度和出现逻辑 |
| targetType | TargetType | Self, AnyEnemy, AllEnemies, RandomEnemy |
| shouldShowInCardLibrary | bool | 是否在图鉴显示（默认 true） |

**基本结构示例：**
```csharp
public class MyCard : CardModel
{
    public MyCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(6m)
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)   // Beta版：第二个参数 cardPlay 必填
            .Targeting(play.Target)
            .Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
```

### 3.2 卡牌稀有度（CardRarity）

`CardRarity` 属性不仅决定卡牌的边框样式，还直接影响卡牌的获取逻辑和商店售价：

```csharp
public enum CardRarity
{
    None,     // 无
    Basic,    // 基础
    Common,   // 普通
    Uncommon, // 罕见
    Rare,     // 稀有
    Ancient,  // 先古之民
    Event,    // 事件
    Token,    // 代币
    Status,   // 状态
    Curse,    // 诅咒
    Quest     // 任务
}
```

**稀有度分类说明**：

| 类型 | 是否出现在随机卡池 | 说明 | 示例 |
|------|------------------|------|------|
| Basic/Common/Uncommon/Rare | **是** | 可在战斗奖励、商店、事件中随机获取 | 打击、防御、各类攻击牌 |
| Ancient | 否 | 先古之民专属卡牌 | 先古遗物相关卡牌 |
| Event | 否 | 事件专属卡牌 | 特定事件奖励 |
| **Token** | **否** | 衍生卡牌，需通过特定条件获取 | 小刀、巨石、灵魂 |
| Status | 否 | 状态卡牌 | 虚弱、易伤 |
| Curse | 否 | 诅咒卡牌 | 痛苦、悔恨 |
| Quest | 否 | 任务卡牌 | 藏宝图、多尼斯异鸟蛋 |

**Token 类型卡牌的使用场景**：

Token 卡牌类似于游戏内置的"衍生卡"机制，不会出现在奖励卡池中，只能通过特定条件（如怪物掉落、卡牌效果）获取。这对于实现"怪物料理只能通过击杀怪物掉落获得"、"单位卡只能通过生产获得"等设计非常有用。

**示例：将掉落卡设置为 Token 类型**：
```csharp
public sealed class MyDroppedFood : CardModel
{
    // 使用 Token 类型，该卡不会出现在随机奖励池中
    public MyDroppedFood() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self) { }
}
```

### 3.3 动态变量（CanonicalVars）

```csharp
protected override List<DynamicVar> CanonicalVars => new List<DynamicVar>
{
    new DamageVar(2m, ValueProp.Move),   // 基础伤害2点
    new BlockVar(5m)                     // 基础格挡5点
};
```

### 3.4 核心回调方法

#### OnPlay - 打出时触发
```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this, cardPlay)
        .Targeting(cardPlay.Target)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);
}
```

**UI 刷新注意事项**：当卡牌打出后需要向手牌添加新卡牌时，可能会出现新卡牌卡在画面中央的情况。此时需要在添加卡牌后调用 `CardPileCmd.Draw(ctx, 0, Owner)` 触发 UI 刷新（虽然抽 0 张牌，但会强制更新手牌区域的 UI 显示）：

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    // ... 选择/生成卡牌逻辑 ...

    // 将生成的卡牌加入手牌
    await CardPileCmd.AddGeneratedCardToCombat(selectedCard, PileType.Hand, Owner);

    // 触发UI刷新：抽0张牌（仅触发刷新机制）
    await CardPileCmd.Draw(ctx, 0, Owner);
}
```

#### OnUpgrade - 升级时触发
```csharp
protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(2m);
    DynamicVars.Block.UpgradeValueBy(3m);
}
```

#### OnTurnEndInHand - 回合结束手牌中触发
```csharp
public override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
{
    await CreatureCmd.Damage(choiceContext, Owner.Creature, 3m, ValueProp.Unblockable, null);
}
```

### 3.5 卡牌标记与关键词

```csharp
// 卡牌标记
protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

// 卡牌关键词
protected override HashSet<CardKeyword> CanonicalKeywords => new HashSet<CardKeyword>
{
    CardKeyword.Exhaust    // 消耗
    // CardKeyword.Ethereal  // 虚无
    // CardKeyword.Innate    // 固有
};
```

### 3.6 注册到卡池

**方法一：使用 ModHelper（推荐，适用于加入原版卡池）**
```csharp
ModHelper.AddModelToPool(typeof(IroncladCardPool), typeof(MyCustomCard));
```

**方法二：在自定义卡池类中注册（适用于自定义角色）**

创建自定义卡池类并在 `GenerateAllCards()` 方法中注册所有卡牌：
```csharp
public sealed class MyCardPool : CardPoolModel
{
    public override string Title => "my_character";

    protected override CardModel[] GenerateAllCards()
    {
        return new CardModel[]
        {
            ModelDb.Card<MyCustomCard>(),
            ModelDb.Card<MyTokenCard>()
            // ... 其他卡牌
        };
    }
}
```

> **重要**：新卡牌必须注册到对应角色的卡池才能被游戏识别和使用。

**常见卡池：**
- `IroncladCardPool`, `SilentCardPool` - 角色专属
- `ColorlessCardPool` - 无色卡池
- `TokenCardPool` - 衍生卡牌
- `StatusCardPool`, `CurseCardPool` - 状态和诅咒

### 3.7 卡牌数值存储规范

**规则1：数值集中存储**
- 任何卡牌的数值信息（费用、伤害、护盾、次数等）都应在数值文件中统一存储
- 推荐使用 `<角色/分类>CardValues.cs` 这样的静态类管理所有卡牌数值
- 卡牌类中通过引用数值存储类获取数值，避免硬编码

**数值存储示例**：
```csharp
// MyCardValues.cs - 统一数值存储
public static class MyCardValues
{
    public static CardValueStore.CardValues MyCard => new()
    {
        Cost = 1,
        Damage = 6,
        DamageUpgraded = 3
    };
}
```

**规则2：动态数值显示（`diff()` 格式化器，容易遗漏）**
- 卡牌描述中的伤害/格挡变量**必须**写成 `{Damage:diff()}`、`{Block:diff()}` 等带 `:diff()` 的格式
- 若写成裸 `{Damage}`，卡牌只会显示 `BaseValue`（基础/升级值），战斗中力量、易伤、虚弱、敏捷等 buff 对数值的修正**不会显示**在卡牌上（这就是"易伤增伤没显示"的根因）
- 机制：`DamageVar`/`BlockVar.UpdateCardPreview` 通过 `Hook.ModifyDamage/ModifyBlock` 计算修正后的 `PreviewValue`；`NCard.UpdateVisuals` 在手牌/打出堆中运行全局 hooks（`runGlobalHooks=true`）；悬停/锁定敌人时 `NCardPlay.SetPreviewTarget` 提供目标，目标身上的易伤/虚弱等 debuff 才会参与计算；修正后数值与基础值不同时自动绿色（变高）/红色（变低）高亮，与原版 Strike 行为一致
- 适用于：`Damage`、`Block`、`DefendDamage`、`DeployDamage`、`DeployVigor` 等攻击/防御类变量
- 不要乱加：`Repeat`、`Poison`、`Count`、`{0}`/`{1}` 等非 hook 修正值保持 `{Var}` 原样

**规则3：ID 映射一致性**
- 卡牌 ID 由类名自动生成：`ClassName` → `CLASS_NAME`（全大写 + 驼峰处加下划线）
- 类名带 `Card` 后缀时，映射键/本地化 key 必须包含 `_CARD`，否则 UI 显示价格或文本会取不到值

### 3.8 本地化 ID 命名规则（`_CARD` 后缀）

游戏的本地化 key 由**卡牌类名**自动生成，规则如下：

- 类名 → 全大写 + 驼峰处加下划线
- **类名以 `Card` 结尾** → 本地化 key **保留** `_CARD` 后缀
- **类名不以 `Card` 结尾** → 本地化 key **没有** `_CARD` 后缀

| 类名 | 本地化 key（title/description） | 是否以 Card 结尾 |
|------|--------------------------------|-------------------|
| `HealFood` | `HEAL_FOOD.title` | ❌ 否 |
| `RockCard` | `ROCK_CARD.title` | ✅ 是 |
| `MyMonsterMeat` | `MY_MONSTER_MEAT.title` | ❌ 否 |
| `BombCard` | `BOMB_CARD.title` | ✅ 是 |

> **排查技巧**：如果卡牌标题/描述在游戏中显示为原始 key（如 `cards.MY_CARD.title`），说明本地化 key 不匹配。检查类名是否以 `Card` 结尾，并同步修改所有 4 个语言的 `cards.json`。

### 3.9 百科卡框颜色（`Pool` / `VisualCardPool`）

卡牌在百科（图鉴）中的边框颜色由 `Pool` / `VisualCardPool` 属性决定：

| 效果 | 实现方式 |
|------|---------|
| 显示所属角色卡池的颜色 | **不 override** `Pool` 和 `VisualCardPool`，使用基类默认值（继承 `Owner.Character.CardPool`） |
| 白色无色边框（公共/中立/衍生卡） | override 两者，将 `VisualCardPool` 设为 `TokenCardPool` |

**无色公共卡标准写法**：
```csharp
using MegaCrit.Sts2.Core.Models.CardPools;

public override CardPoolModel Pool => IsMutable && Owner != null
    ? Owner.Character.CardPool      // 战斗中：角色实际卡池（正常打牌）
    : ModelDb.CardPool<TokenCardPool>();

public override CardPoolModel VisualCardPool => ModelDb.CardPool<TokenCardPool>();  // 百科显示：白色无边框
```

**核心属性说明**：

| 属性 | 说明 |
|------|------|
| `Pool` | 卡牌所属卡池，决定卡框颜色 |
| `VisualCardPool` | UI 显示时使用的卡池，通常与 `Pool` 相同 |
| `IsMutable` | 是否为战斗实例（战斗实例才有 `Owner`） |
| `Owner.Character.CardPool` | 当前持有者的角色卡池 |
| `TokenCardPool` | 无主/衍生卡牌使用的卡池（白色/无色） |

> **常见坑**：如果 override `VisualCardPool` 为 `TokenCardPool`，百科中该卡就是白色边框；如果不 override，会继承注册角色的颜色。根据卡牌定位选择即可。不要忘了添加 `using MegaCrit.Sts2.Core.Models.CardPools;`。

### 3.10 Beta 版 API 变化详解

> 以下 API 仅适用于 Beta 版，正式版使用不同签名。移植代码时需重点关注。

#### 1. 卡牌去向：GetResultLocationForCardPlay

**Beta 版变更**：方法名和返回值都变了。

```csharp
// 正式版（已废弃）
protected virtual PileType GetResultPileTypeForCardPlay()

// Beta版（新API）
protected virtual CardLocation GetResultLocationForCardPlay()
```

**CardLocation 结构**（Beta 版新增）：
```csharp
public record struct CardLocation
{
    public Player player;              // 目标玩家
    public PileType pileType;          // 牌堆类型
    public CardPilePosition position;  // 牌堆位置
}
```

**迁移示例（打出后回到手牌的卡）**：
```csharp
// 正式版
protected override PileType GetResultPileTypeForCardPlay()
{
    PileType result = base.GetResultPileTypeForCardPlay();
    if (result != PileType.Discard) return result;
    return PileType.Hand;
}

// Beta版
protected override CardLocation GetResultLocationForCardPlay()
{
    CardLocation result = base.GetResultLocationForCardPlay();
    if (result.pileType != PileType.Discard) return result;
    result.pileType = PileType.Hand;
    return result;
}
```

#### 2. 攻击卡：FromCard 新增 cardPlay 参数

**影响范围：所有攻击卡**

```csharp
// 正式版
public AttackCommand FromCard(CardModel card)

// Beta版（新增 cardPlay 参数）
public AttackCommand FromCard(CardModel card, CardPlay? cardPlay)
```

**迁移示例**：
```csharp
// 正式版
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .Execute(choiceContext);

// Beta版
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this, cardPlay)  // 新增第二个参数
    .Targeting(cardPlay.Target)
    .Execute(choiceContext);
```

#### 3. 群体攻击：Targeting 参数变化

**Beta 版变更**：`Targeting` 不再接受 `List<Creature>`，需改用新 API。

```csharp
// 正式版
DamageCmd.Attack(amount).FromCard(this).Targeting(List<Creature>)

// Beta版（二选一）
DamageCmd.Attack(amount).FromCard(this, cardPlay).Targeting(Creature)           // 单个目标
DamageCmd.Attack(amount).FromCard(this, cardPlay).TargetingAllOpponents(CombatState) // 所有敌人
```

#### 4. CardPlay 构造：Player 为必填成员

```csharp
// 正式版
new CardPlay
{
    Card = this,
    Target = target,
    // ...
}

// Beta版（CardPlay.Player 为必填）
new CardPlay
{
    Player = Owner,  // 新增必填项
    Card = this,
    Target = target,
    // ...
}
```

### 3.11 卡牌进阶注意事项

**坑1：动态关键词不会刷新缓存**

`CardModel.LocalKeywords` 会缓存 `CanonicalKeywords` 的结果。首次访问后，即使 `CanonicalKeywords` 返回值变化，UI 读取的仍是旧缓存。

```csharp
// ❌ 错误：CanonicalKeywords 动态返回值不会刷新缓存
public override IEnumerable<CardKeyword> CanonicalKeywords
{
    get
    {
        var keywords = new List<CardKeyword>();
        if (_condition) keywords.Add(CardKeyword.Exhaust);
        return keywords;
    }
}

// ✅ 正确：用 AddKeyword 直接修改缓存的 _keywords 集合
public void SetState(bool condition)
{
    if (condition)
        AddKeyword(CardKeyword.Exhaust);  // 直接修改缓存 + 触发UI刷新
}
```

**坑2：DeepCloneFields 必须复制/重置动态状态字段**

卡牌克隆（如复制到手牌/转移）时，`DeepCloneFields` 必须复制所有自定义字段；需要重置的状态字段也要显式重置（`_keywords` 的克隆由 `base.DeepCloneFields()` 负责）：

```csharp
protected override void DeepCloneFields()
{
    base.DeepCloneFields();
    _storedCards = new List<CardModel>(_storedCards);
    _hasStored = false;        // 重置：克隆体不应继承存储状态
    _condition = false;        // 重置：由 SetState 重新设置
}
```

**坑3：打出后加入手牌需要手动刷新 UI**

见 [3.4 OnPlay](#onplay---打出时触发) 的 UI 刷新注意事项，`CardPileCmd.Draw(ctx, 0, Owner)` 是通用解法。

### 3.12 资源路径

```
res://images/atlases/card_atlas.sprites/<卡池名称>/<卡牌ID小写>.tres
res://images/packed/card_portraits/<卡池名称>/<卡牌ID小写>.png
```

**卡池名称对应：**
- `ironclad`, `silent`, `defect`, `necrobinder`, `regent`
- `colorless`, `curse`, `event`, `quest`, `status`, `token`

### 3.13 本地化文本

`res://<ModID>/localization/zhs/cards.json`:
```json
{
  "MY_CUSTOM_CARD.title": "飞刀",
  "MY_CUSTOM_CARD.description": "对指定敌人造成 {Damage} 点伤害。"
}
```

- `{Damage}` 会被动态变量的当前值替换
- `{Damage:diff()}` 显示升级后的差值（如"造成 2→4 点伤害"），且会随战斗中 buff 实时修正

### 3.14 编译与部署

每次代码更新后，需要重新编译生成新的 DLL：

```bash
dotnet build <ModID>.csproj -c Release -o build
```

编译成功后，将以下文件复制到游戏的 `mods/<ModID>/` 目录：
- `<ModID>.dll` - 主程序集（必须）
- `<ModID>.json` - Mod 配置文件（必须）
- `<ModID>.pck` - 资源包（如果有资源）

---

## 4. 自定义词条（Custom Keywords）

Mod 可以添加自定义词条来增强卡牌的视觉效果和交互体验。词条会在卡牌描述下方显示金色文本，鼠标悬停时显示详细描述。

### 设计理念

自定义词条适用于需要特殊条件或限制的卡牌，例如：
- 需要特定条件才能打出的卡牌
- 具有特殊使用规则的卡牌
- 增强卡牌的视觉效果和提示

### 实现步骤

#### 第一步：创建词条定义类

在 `Utils/` 目录下创建 `CustomKeyword.cs`：

```csharp
using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace MyModCode.Utils;

/// <summary>
/// 自定义词条定义
/// </summary>
public class CustomKeyword
{
    public string Id { get; }
    public LocString Title { get; }
    public LocString Description { get; }

    public CustomKeyword(string id, LocString title, LocString description)
    {
        Id = id;
        Title = title;
        Description = description;
    }

    /// <summary>
    /// 创建悬停提示
    /// </summary>
    public IHoverTip CreateHoverTip()
    {
        return new HoverTip(Title, Description);
    }
}

/// <summary>
/// 预定义的自定义词条
/// </summary>
public static class ModCardKeywords
{
    /// <summary>
    /// 示例词条
    /// </summary>
    public static readonly CustomKeyword Example = new(
        "EXAMPLE",
        new LocString("card_keywords", "example.title"),
        new LocString("card_keywords", "example.description")
    );
}
```

#### 第二步：在卡牌中使用 ExtraHoverTips

在需要添加词条的卡牌类中重写 `ExtraHoverTips` 属性：

```csharp
public sealed class MyKeywordCard : CardModel
{
    public MyKeywordCard() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    /// <summary>
    /// 额外的悬停提示（包含自定义词条）
    /// </summary>
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Example.CreateHoverTip()
    ];
}
```

#### 第三步：添加本地化文本

创建或更新 `localization/zhs/card_keywords.json`：

```json
{
    "example.title": "示例词条",
    "example.description": "这是示例词条的详细说明。"
}
```

#### 第四步：在卡牌描述中显示词条文本

在 `cards.json` 的卡牌描述中添加金色格式化的词条文本：

```json
{
    "MY_KEYWORD_CARD.title": "示例卡牌",
    "MY_KEYWORD_CARD.description": "[gold]示例词条. [/gold]\n卡牌效果描述。"
}
```

### 效果说明

- **卡牌显示**：在描述下方显示金色的"示例词条."文本
- **悬停提示**：鼠标悬停在词条上时显示详细描述

### 扩展更多词条

在 `ModCardKeywords` 类中添加更多词条：

```csharp
public static class ModCardKeywords
{
    public static readonly CustomKeyword Example = new(...);

    // 添加新词条
    public static readonly CustomKeyword MyNewKeyword = new(
        "MY_NEW_KEYWORD",
        new LocString("card_keywords", "my_new_keyword.title"),
        new LocString("card_keywords", "my_new_keyword.description")
    );
}
```

然后在需要使用该词条的卡牌中添加到 `ExtraHoverTips`：

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    ModCardKeywords.Example.CreateHoverTip(),
    ModCardKeywords.MyNewKeyword.CreateHoverTip()
];
```

### 进阶：带行为逻辑的自定义词条（"超时空"案例）

前面介绍的自定义词条只包含视觉效果（金色文本 + 悬停提示）。当词条需要绑定游戏行为时，需要创建基类来封装词条逻辑。

#### 应用场景

"超时空"词条的核心逻辑：
1. 打出卡牌时，卡牌进入摸牌堆而非弃牌堆
2. 当卡牌同时拥有"消耗（Exhaust）"词条时，首次打出进入摸牌堆并移除超时空词条，下次打出正常消耗

#### 第一步：创建词条定义

在 `CustomKeyword.cs` 的 `ModCardKeywords` 类中添加超时空词条：

```csharp
public static readonly CustomKeyword Chrono = new(
    "CHRONO",
    new LocString("card_keywords", "chrono.title"),
    new LocString("card_keywords", "chrono.description")
);
```

#### 第二步：创建词条行为基类

在 `Common/Cards/` 目录下创建 `ChronoCardModel.cs`：

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MyModCode.Common.Utils;

namespace MyModCode.Common.Cards;

/// <summary>
/// 超时空卡牌基类
/// 自动处理超时空词条效果：
/// 1. 打出时卡牌进入摸牌堆而非弃牌堆
/// 2. 当卡牌同时拥有消耗(Exhaust)词条时，本次打出进入摸牌堆并移除超时空词条，下次打出正常消耗
/// 3. 自动添加超时空描述文本和悬停提示
/// </summary>
public abstract class ChronoCardModel : CardModel
{
    private bool _chronoConsumed;

    protected ChronoCardModel(int cost, CardType cardType, CardRarity cardRarity, TargetType targetType)
        : base(cost, cardType, cardRarity, targetType) { }

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var tips = GetExtraHoverTips();

            if (!_chronoConsumed)
            {
                tips.Add(ModCardKeywords.Chrono.CreateHoverTip());
            }

            return tips;
        }
    }

    /// <summary>
    /// 子类重写此方法提供额外的悬停提示
    /// </summary>
    protected abstract List<IHoverTip> GetExtraHoverTips();

    protected override CardLocation GetResultLocationForCardPlay()
    {
        // 如果超时空效果已消耗，走正常流程
        if (_chronoConsumed)
        {
            return base.GetResultLocationForCardPlay();
        }

        bool hasExhaustKeyword = Keywords.Contains(CardKeyword.Exhaust);

        if (hasExhaustKeyword)
        {
            // 有消耗词条：执行最后一次超时空，移除超时空效果
            _chronoConsumed = true;
            if (DynamicVars["ChronoTitle"] is StringVar chronoTitleVar)
            {
                chronoTitleVar.StringValue = string.Empty;
            }
            return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
        }

        // 无消耗词条：正常超时空效果，进入摸牌堆
        return new CardLocation(Owner, PileType.Draw, CardPilePosition.Bottom);
    }
}
```

#### 第三步：卡牌继承基类

改造原有卡牌，继承 `ChronoCardModel` 而非 `CardModel`：

```csharp
public sealed class MyChronoCard : ChronoCardModel
{
    private static readonly CardValueStore.CardValues Values = MyCardValues.MyChronoCard;

    public MyChronoCard() : base((int)Values.Cost, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override string PortraitPath => "res://MyModResources/images/packed/card_portraits/my_chrono_card.png";

    protected override List<IHoverTip> GetExtraHoverTips()
    {
        return new List<IHoverTip>
        {
            ModCardKeywords.Example.CreateHoverTip()
        };
    }

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new IntVar("Value", Values.Value),
        new StringVar("ChronoTitle", "[gold]超时空.[/gold]\n")
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        // 卡牌特有逻辑...
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Value"].BaseValue = Values.Value + Values.ValueUpgraded;
    }
}
```

#### 第四步：添加本地化文本

**card_keywords.json**：
```json
{
    "chrono.title": "超时空",
    "chrono.description": "打出时进入摸牌堆。与消耗词条共存时，首次打出进入摸牌堆并移除超时空，下次打出正常消耗。"
}
```

**cards.json**（在描述开头添加 `{ChronoTitle}` 动态变量）：
```json
{
    "MY_CHRONO_CARD.description": "{ChronoTitle}获得 {Value} 点效果。"
}
```

#### 核心机制详解

| 机制 | 说明 |
|------|------|
| `GetResultLocationForCardPlay()` | Beta版新增方法，控制卡牌打出后的去向 |
| `_chronoConsumed` | 状态标记，控制超时空效果是否已消耗 |
| `StringVar("ChronoTitle")` | 动态变量，控制描述开头的"超时空."文本显示/隐藏 |
| `GetExtraHoverTips()` | 抽象方法，子类返回额外的悬浮提示 |
| `ExtraHoverTips` | 基类重写，根据 `_chronoConsumed` 状态动态添加超时空词条悬浮提示 |

#### 效果流程

```
打出超时空卡牌（无消耗词条）
    ↓
GetResultLocationForCardPlay() 返回 CardLocation(Draw, Bottom)
    ↓
卡牌进入摸牌堆底部，超时空效果保留
    ↓
下次打出重复此流程

打出超时空卡牌（有消耗词条）
    ↓
检测到 CardKeyword.Exhaust
    ↓
_chronoConsumed = true
    ↓
ChronoTitle.StringValue = ""（移除描述中的"超时空."文本）
    ↓
返回 CardLocation(Draw, Bottom)，卡牌进入摸牌堆
    ↓
下次打出时 _chronoConsumed = true
    ↓
走 base.GetResultLocationForCardPlay()，正常消耗
```

#### 优势

1. **代码解耦**：超时空逻辑集中在基类，卡牌只需关注自身特有逻辑
2. **易于维护**：修改超时空规则只需修改基类，影响所有超时空卡牌
3. **一致性**：所有超时空卡牌行为一致，避免遗漏或错误
4. **可扩展性**：新增超时空卡牌只需继承基类，无需重复编写超时空逻辑

---

## 5. 卡牌悬浮提示（HoverTip）

### 5.1 核心原理

卡牌上展示悬浮的其他卡牌和能力，是通过重写 `CardModel` 类的 **`ExtraHoverTips`** 属性实现的。游戏引擎会自动将这些提示显示在卡牌描述下方，当玩家将鼠标悬浮在卡牌上时，会显示对应的卡牌或能力的详细信息。

### 5.2 HoverTipFactory 工具类

游戏提供了 `MegaCrit.Sts2.Core.HoverTips.HoverTipFactory` 静态类来生成各种悬浮提示：

| 方法 | 作用 | 示例 |
|------|------|------|
| `FromCard<T>(bool upgrade = false)` | 生成卡牌预览 | `HoverTipFactory.FromCard<Shiv>()` |
| `FromCardWithCardHoverTips<T>()` | 生成卡牌预览 + 卡牌附带的所有悬浮提示 | `HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>()` |
| `FromPower<T>(int? amount = null)` | 生成能力预览 | `HoverTipFactory.FromPower<PoisonPower>()` |
| `FromPowerWithPowerHoverTips<T>()` | 生成能力预览 + 能力附带的所有悬浮提示 | - |
| `FromOrb<T>()` | 生成球体预览 | `HoverTipFactory.FromOrb<LightningOrb>()` |
| `FromRelic<T>()` | 生成遗物预览 | - |
| `Static(StaticHoverTip tip, params DynamicVar[] vars)` | 生成静态文本提示 | - |

### 5.3 游戏原版示例

**Accuracy 卡牌**（展示 Shiv 卡牌预览）：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class Accuracy : CardModel
{
    // ... 其他代码 ...

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<Shiv>()];
}
```

**Abrasive 卡牌**（展示多个能力预览）：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class Abrasive : CardModel
{
    // ... 其他代码 ...

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<ThornsPower>()
    ];
}
```

**SovereignBlade 卡牌**（展示卡牌及其附带的所有提示）：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class SovereignBlade : CardModel
{
    // ... 其他代码 ...

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>();
}
```

### 5.4 自定义实现示例

**示例1：展示升级后的卡牌预览**

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class MyCard : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        // 展示基础版卡牌
        HoverTipFactory.FromCard<MyTokenCard>(),
        // 展示升级后的卡牌（upgrade: true）
        HoverTipFactory.FromCard<MyTokenCard>(upgrade: true)
    ];
}
```

**示例2：展示能力预览**

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class PoisonStab : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        // 展示毒药能力，指定层数
        HoverTipFactory.FromPower<PoisonPower>(3)
    ];
}
```

**示例3：混合展示卡牌和能力**

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class BladeOfInk : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        // 展示卡牌预览
        HoverTipFactory.FromCard<InkyShiv>(),
        // 展示能力预览
        HoverTipFactory.FromPower<WeakPower>()
    ];
}
```

### 5.5 能力中的悬浮提示

能力类也可以通过重写 `ExtraHoverTips` 属性来展示其他能力或卡牌的预览：

```csharp
using MegaCrit.Sts2.Core.HoverTips;

public sealed class MyBuff : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<ThornsPower>()
    ];
}
```

### 5.6 动态悬浮 Tip 升级机制（HoverTipHelper）

当卡牌的衍生卡效果会随升级而变化时，使用 `HoverTipHelper` 可以根据源卡牌的升级状态动态显示对应版本的衍生卡牌。

#### 核心原理

传统的 `HoverTipFactory.FromCard<T>()` 只能显示固定版本的卡牌预览，无法根据源卡牌的升级状态动态调整。`HoverTipHelper` 通过传入一个 `Func<bool>` 委托来判断当前卡牌是否已升级，从而生成对应版本的悬浮提示。

#### 使用示例

```csharp
using MyModCode.Common.Utils;

public sealed class MyBuildingCard : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ModCardKeywords.Example.CreateHoverTip(),
        HoverTipHelper.FromCardWithUpgrade<MyTokenCard>(() => IsUpgraded)
    ];
}
```

#### HoverTipHelper 工具类实现

```csharp
// MyModCode/Common/Utils/HoverTipHelper.cs
public static class HoverTipHelper
{
    public static IHoverTip FromCardWithUpgrade<T>(Func<bool> isUpgradedFunc) where T : CardModel
    {
        var model = ModelDb.Card<T>();
        var mutable = model.ToMutable();

        if (isUpgradedFunc())
        {
            mutable.UpgradeInternal();
        }

        return HoverTipFactory.FromCard(mutable);
    }
}
```

#### 使用场景

| 场景 | 说明 |
|------|------|
| 卡牌生成衍生卡 | 源卡升级后，生产的衍生卡也会升级 |
| 超级武器类卡牌 | 升级后冷却回合减少，产生的效果卡牌数值增强 |
| 能力卡衍生效果 | 能力卡升级后，衍生卡牌的数值或效果发生变化 |

#### 注意事项

- 使用前需要添加引用：`using MyModCode.Common.Utils;`
- 泛型参数必须是已注册的卡牌类型
- 委托 `() => IsUpgraded` 使用了卡牌的 `IsUpgraded` 属性，可以替换为自定义的升级判断逻辑

### 5.7 注意事项

1. **using 引用**：使用 `HoverTipFactory` 前需要添加 `using MegaCrit.Sts2.Core.HoverTips;`
2. **泛型类型**：`FromCard<T>`、`FromPower<T>` 中的泛型参数必须是已注册的卡牌或能力类型
3. **升级参数**：`FromCard<T>(upgrade: true)` 会生成升级后的卡牌预览
4. **层数参数**：`FromPower<T>(amount)` 可以指定能力的层数显示
5. **动态升级机制**：使用 `HoverTipHelper.FromCardWithUpgrade<T>()` 实现随升级状态变化的悬浮提示

---

## 6. 自定义遗物（RelicModel）

### 6.1 遗物基类

所有遗物继承自 `RelicModel` 抽象类。

```csharp
public class MyCustomRelic : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new EnergyVar(2) };

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Creature.Side && combatState.RoundNumber == 1)
        {
            Flash();  // 遗物图标闪烁
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }
}
```

### 6.2 遗物稀有度

| 稀有度 | 获取方式 |
|--------|----------|
| `Starter` | 初始遗物，不在宝箱/精英中出现 |
| `Common` | 普通遗物，可通过宝箱、精英获取 |
| `Uncommon` | 罕见遗物 |
| `Rare` | 稀有遗物 |
| `Shop` | 商店遗物 |
| `Event` | 事件遗物 |
| `Ancient` | 先古之民给予的遗物 |

### 6.3 常用事件钩子

```csharp
// 回合开始时
public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)

// 卡牌打出后
public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)

// 受到伤害前
public override async Task BeforeTakeDamage(...)

// 敌人死亡时
public override async Task AfterMonsterKilled(...)

// 战斗结束时
public override async Task AfterCombatEnd(...)
```

> **提示**：`AfterSideTurnStart` 在每个阵营回合开始时各触发一次（玩家方回合、敌方回合各一次），同一回合内不会重复触发。

### 6.4 注册到遗物池

```csharp
// 在ModInitializer中
ModHelper.AddModelToPool(typeof(IroncladRelicPool), typeof(MyCustomRelic));
```

**常见遗物池：**
- `IroncladRelicPool`, `SilentRelicPool` - 角色专属
- `SharedRelicPool` - 公共遗物池（精英、商店、宝箱）
- `EventRelicPool` - 事件遗物
- `FallbackRelicPool` - 兜底池

### 6.5 修改初始遗物（HarmonyPatch）

```csharp
[HarmonyPatch(typeof(Ironclad), nameof(Ironclad.StartingRelics), MethodType.Getter)]
public static class IroncladStartingRelicsPatch
{
    static void Postfix(ref IReadOnlyList<RelicModel> __result)
    {
        var customRelic = ModelDb.Relic<MyCustomRelic>();
        if (__result.Any(r => r.Id == customRelic.Id)) return;
        var list = __result.ToList();
        list.Add(customRelic);
        __result = list;
    }
}
```

### 6.6 修改原版遗物行为（NewLeaf / LeafyPoultice 模式）

原版遗物「新叶」（NewLeaf）和「树叶膏药」（LeafyPoultice）会转换牌组中的卡牌。Mod 可以通过 **Harmony Prefix 拦截 `AfterObtained`**，为自定义角色提供专属的转换逻辑/转换卡池（例如把原版卡转换成 Mod 卡）。

**核心模式：**
1. **角色判断**：非本 Mod 角色 `return true` 走原版逻辑；本 Mod 角色用 `__result = MyTransformAsync(__instance); return false;` 替换逻辑
2. **选择面板**：用 `CardSelectCmd.FromDeckGeneric(player, prefs, filter)` 让玩家从牌组选卡，`CardSelectorPrefs` 控制提示与选择数量
3. **转换**：`CardCmd.Transform(selectedCard, replacement)` 精确转换；`CardCmd.TransformToRandom(selectedCard, rng)` 随机转换
4. **转换卡池**：通过注册类方法动态获取（如 `GetAllMyUnitCards()`），避免硬编码卡牌列表

```csharp
[HarmonyPrefix]
[HarmonyPatch(typeof(NewLeaf), "AfterObtained")]
public static bool NewLeafAfterObtainedPrefix(NewLeaf __instance, ref Task __result)
{
    if (!IsMyCharacter(__instance.Owner.Character))
        return true; // 非Mod角色走原版逻辑

    __result = NewLeafTransformAsync(__instance);
    return false;
}

private static async Task NewLeafTransformAsync(NewLeaf __instance)
{
    var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1, 1);

    // 选择面板 filter：排除诅咒卡等
    var selectedCards = (await CardSelectCmd.FromDeckGeneric(
        player: __instance.Owner,
        prefs: prefs,
        filter: card => card.Type != CardType.Curse
    )).ToList();

    if (selectedCards.Any())
    {
        var selectedCard = selectedCards.First();

        if (IsMyConvertibleCard(selectedCard))
        {
            // Mod 卡 → 从 Mod 转换卡池随机转换（排除自身）
            var pool = GetAllMyConvertibleCards();
            var rng = __instance.Owner.PlayerRng.Transformations;
            var targets = pool.Where(t => t.Id.Entry != selectedCard.Id.Entry).ToList();
            if (targets.Any())
            {
                var replacement = __instance.Owner.RunState.CreateCard(rng.NextItem(targets), __instance.Owner);
                await CardCmd.Transform(selectedCard, replacement);
            }
        }
        else
        {
            // 非 Mod 卡 → 走原版随机转换
            await CardCmd.TransformToRandom(selectedCard, __instance.Owner.RunState.Rng.Niche);
        }
    }
}
```

> **注意**：如果拦截的是 `AfterObtained`，还需要手动处理原版逻辑中的副作用（如 LeafyPoultice 扣除 12 点最大生命值），确保替换后的逻辑与原文保持一致。

### 6.7 资源路径

```
res://images/relics/my_custom_relic.png              # 大图 (256x256)
res://images/relics/my_custom_relic_outline.png      # 描边图
res://images/atlases/relic_atlas.sprites/my_custom_relic.tres          # 裁切纹理
res://images/atlases/relic_outline_atlas.sprites/my_custom_relic.tres  # 描边裁切
```

### 6.8 本地化文本

`res://<ModID>/localization/zhs/relics.json`:
```json
{
  "MY_CUSTOM_RELIC.title": "瓶装能量",
  "MY_CUSTOM_RELIC.description": "每场战斗开始时，获得 {Energy} 点能量。",
  "MY_CUSTOM_RELIC.flavor": "这个瓶子中蕴含着无尽的力量"
}
```

---

## 7. 自定义药水（PotionModel）

### 7.1 药水核心属性

```csharp
public sealed class MyAoEPotion : PotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AllEnemies;

    protected override List<DynamicVar> CanonicalVars => new() { new DamageVar(30m, ValueProp.Unpowered) };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState.HittableEnemies,
            DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props, Owner.Creature, null);
    }
}
```

**使用时机（PotionUsage）：**
- `CombatOnly` - 只能在战斗中使用
- `AnyTime` - 可在战斗外任意时机使用
- `Automatic` - 不能主动使用，由游戏自动触发（如复活药水）

### 7.2 自动触发药水示例

```csharp
public sealed class MyRevivePotion : PotionModel
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.Automatic;
    public override TargetType TargetType => TargetType.Self;
    public override bool CanBeGeneratedInCombat => false;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await CreatureCmd.Heal(target, 10m);
    }

    public override bool ShouldDie(Creature creature) => creature != Owner.Creature;

    public override async Task AfterPreventingDeath(Creature creature)
    {
        await OnUseWrapper(new ThrowingPlayerChoiceContext(), creature);
    }
}
```

### 7.3 注册到药水池

```csharp
ModHelper.AddModelToPool(typeof(SharedPotionPool), typeof(MyAoEPotion));
```

**常见药水池：**
- `SharedPotionPool` - 所有角色共享
- `IroncladPotionPool`, `SilentPotionPool` - 角色专属
- `EventPotionPool` - 事件药水
- `TokenPotionPool` - 衍生药水

### 7.4 资源路径

```
res://images/potions/<药水ID小写>.png
res://images/atlases/potion_atlas.sprites/<药水ID小写>.tres
res://images/atlases/potion_outline_atlas.sprites/<药水ID小写>.tres
```

### 7.5 本地化文本

`res://<ModID>/localization/zhs/potions.json`:
```json
{
  "MY_AOE_POTION.title": "群体伤害药水",
  "MY_AOE_POTION.description": "对所有敌人造成 {Damage} 点伤害。"
}
```

---

## 8. 卡牌附魔（EnchantmentModel）

### 8.1 附魔基类

```csharp
public sealed class MyCustomEnchantment : EnchantmentModel
{
    public override bool ShowAmount => true;
    public override bool HasExtraCardText => true;

    protected override List<DynamicVar> CanonicalVars => new()
    {
        new DamageVar(0m, ValueProp.Move),
        new BlockVar(0m, ValueProp.Move)
    };

    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;  // 只能附魔攻击牌
    }

    public override void RecalculateValues()
    {
        DynamicVars.Damage.BaseValue = Amount;
        DynamicVars.Block.BaseValue = Amount;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await CreatureCmd.GainBlock(Card.Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        if (Status == EnchantmentStatus.Disabled) return 0m;
        bool isPoweredAttack = props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
        return isPoweredAttack ? DynamicVars.Damage.BaseValue : 0m;
    }
}
```

### 8.2 关键方法

| 方法 | 说明 |
|------|------|
| `CanEnchantCardType` | 限制可附魔的卡牌类型 |
| `OnEnchant` | 附魔被添加时触发 |
| `RecalculateValues` | 层数变化时重新计算数值 |
| `OnPlay` | 被附魔卡牌打出时触发 |
| `EnchantDamageAdditive` | 提供额外伤害 |
| `EnchantBlockAdditive` | 提供额外格挡 |

### 8.3 给卡牌添加附魔

```csharp
CardCmd.Enchant<MyCustomEnchantment>(card, 1m);  // 层数为1
```

### 8.4 资源路径

```
res://images/enchantments/<附魔ID小写>.png
```

### 8.5 本地化文本

`res://<ModID>/localization/zhs/enchantments.json`:
```json
{
  "MY_CUSTOM_ENCHANTMENT.title": "谨慎",
  "MY_CUSTOM_ENCHANTMENT.description": "这张牌额外造成{Damage}点[gold]伤害[/gold]。\n打出这张牌时，提供{Block}点[gold]格挡[/gold]。",
  "MY_CUSTOM_ENCHANTMENT.extraCardText": "增加 {Amount} 伤害"
}
```

---

## 9. 自定义能力（PowerModel）

### 9.1 能力基类

```csharp
public sealed class MyBlockOnPlayBuff : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => false;
    public override bool AllowNegative => false;

    protected override List<IHoverTip> ExtraHoverTips => new()
    {
        HoverTipFactory.Static(StaticHoverTip.Block)
    };

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner) return;
        if (Amount <= 0) return;

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
```

### 9.2 能力属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Type` | `PowerType` | Buff（增益）/ Debuff（减益） |
| `StackType` | `PowerStackType` | Counter（右下角显示层数数值，如力量）/ Single（无层数只显示图标，如虚弱） |
| `InstanceType` | `PowerInstanceType`（枚举，推荐使用） | 重复施加时的实例策略，见下表详解 |
| `AllowNegative` | `bool` | 是否允许 Amount 为负数 |
| `IsInstanced`（旧） | `bool` | 过时 API，等效于 `InstanceType = Instanced` / `None`，建议改用枚举版 |

#### PowerInstanceType 枚举详解（解包 `sts2.dll` 源码确认）

控制 `PowerCmd.Apply` 时是"叠加 Amount 到已有实例"还是"新建独立实例"，直接决定能力"会不会叠层"。

| 枚举值 | PowerCmd.Apply 内部行为（PowerCmd.cs:167-173） | 含义与适用场景 | 官方示例 |
|--------|-----------------------------------------------|---------------|------------|
| `None`（默认） | 用 `target.GetPower(Id)` 查找同 ID 实例 → 找到就 ModifyAmount 叠加 Amount，找不到才新建。Creature.AddPower 还会校验非 Instanced 类型不许重复添加。 | 再打一张 = 在同一份效果上"加数值"，**不需要每个实例独立状态**。 | Strength、Dexterity、Vulnerable 等纯数值 Buff/Debuff |
| `Instanced` | **查找 existing 直接返回 null → 每次 Apply 都新建独立实例**，每个实例 Amount 独立。 | 再打一张 = "又多了一个独立单元"，每个实例需要**独立的自定义字段/状态/倒计时**。 | TheBombPower（炸弹倒计时）等 |

> **⚠️ 最常见的坑**：`InstanceType` 已经设为 `Instanced`（要独立实例），但在卡牌 OnPlay 里又手写 `owner.Powers.OfType<YourPower>().FirstOrDefault() → ModifyAmount`，这段手动查找和叠加会完全绕过框架的 InstanceType 机制，导致永远叠不上独立实例。正确做法：**`Instanced` + 直接 Apply，让框架自己每次新建。**

#### PowerStackType（枚举：层数显示方式）

控制 UI 中层数的可视化样式，与 InstanceType 正交可自由组合：

| 值 | 说明 | 典型搭配 |
|---|---|---|
| `Counter` | 右下角显示 Amount 数值（例："力量 2"） | 纯数值叠层 + None，或 Instanced 但每个实例有 Amount |
| `Single` | 不显示层数，只显示图标 | 状态类能力：Weak、Frail、Spirit |

#### 场景速选："我这能力到底要叠层（Amount）还是不叠层（独立实例）？"

| 你的需求 | InstanceType | StackType | 卡牌侧 OnPlay 写法 |
|---------|-------------|-----------|----------------|
| 再打一张力量卡，是"力量+2"，还是又多了一个"力量图标"？ | `None` | `Counter` | 直接 `PowerCmd.Apply<T>(amount, ...)`，框架自动叠加 |
| 再打一张炸弹卡，是"倒计时更久"，还是又多了一颗独立倒计时的炸弹？ | `Instanced` | `Counter` / `Single` | 直接 `PowerCmd.Apply<T>(amount: 1, ...)`，每次新建实例 |

> **通用选择速查**
> - 需要独立状态的实体（每座建筑/每个炮塔/每颗炸弹/每个倒计时）→ **`Instanced`**
> - 纯数值合并（中毒、易伤、虚弱、力量、计数类）→ **`None`**

### 9.3 常用事件钩子

```csharp
OnApplied          // 能力被施加时
OnRemoved          // 能力被移除时
BeforeTakeDamage   // 受到伤害前
AfterTakeDamage    // 受到伤害后
OnTurnStart        // 回合开始时
OnCardDrawn        // 抽牌时
ModifyDamage       // 修改伤害数值
ModifyBlock        // 修改格挡数值
```

### 9.4 施加与移除能力

```csharp
// 施加
await PowerCmd.Apply<MyBlockOnPlayBuff>(target, amount, source, sourceCard);

// 移除
await PowerCmd.Remove(powerInstance);
```

### 9.5 Power 高级模式（推荐）：自监听「未格挡伤害」+ 动态状态描述注入

> **⚠️ 先用二分法判断你属于哪种场景！不要误用模式！**
>
> | 你监听伤害之后要触发什么行为？ | 推荐模式 |
> |-----------------------------|---------|
> | **改 Power 自己的状态（扣血/蓄能/护盾计数）+ 触发 Power 自己的效果（爆炸/放技能/护盾破）** | ⭐ 本节新模式：Harmony Postfix + Power 自监听 |
> | **卡牌级操作**（从牌堆/弃牌堆/抽牌堆搜一张卡、自动打出某卡、生成 Token 卡入手牌） | **遗物模式**（旧模式正确）。遗物天然有玩家 Owner、战斗 combatState、牌堆 DrawPile/DiscardPile/Hand、CardPlay 上下文 |
> | **跨多战斗的玩家级永久加成**（全局+力量、+血量上限、开局获得某卡） | **遗物模式**。遗物是战斗间持久化容器；Power 战斗结束会清空 |
>
> 一句话口诀：**改 Power 自己 → Power 自监听；动卡牌 → 遗物；跨战斗 → 遗物。**

#### 旧模式（遗物）vs 新模式（Power 自监听）适用场景对比

这是以后"带独立状态 + 受击触发"类能力（地雷、炮塔、自爆单位、护盾装置、可破坏建筑等）**一定会反复遇到**的设计选择：

| 维度 | 旧模式：遗物中写监听 | **新模式：Harmony 广播补丁 + Power 自监听 ⭐ 推荐** |
|------|----------------------------------|-------------------------------------------------------------|
| **耦合结构** | 能力逻辑 + 遗物逻辑两个类，必须先有遗物才能触发监听 | **只有 Power 一个类**，Harmony 补丁充当"事件总线"，不产生任何游戏内实体 |
| **遗物实例必须存在？** | ✅ 是。玩家没有该遗物 → 能力根本无法响应伤害事件 | ❌ **不需要**。Postfix 挂的是 `RelicModel.AfterDamageReceived` 的「方法签名」→ 所有 HookListener 经过的广播出口，不要求任何遗物存在 |
| **UnblockedDamage 精度** | ✅ 同精度（都是同一个 Hook 点） | ✅ 同精度（都是同一个 Hook 点） |
| **多个同类能力（多个独立实例）** | 需要遗物内部遍历 `OfType<YourPower>()` 逐个处理 | **天然解耦**：补丁统一做一次外层去重，然后遍历 powers.ForEach(p.Receive())，每个 Power 独立去重 |
| **触发者身份** | 怪物/中立生物受击时遗物不监听（遗物只在玩家身上）→ 怪物带能力无法触发 | **任何 Creature 受击都能触发**（Postfix 参数 target 是受伤者，不区分玩家/怪物），对 Boss 战设计非常友好 |
| **代码复用性** | 每个需要监听伤害的能力要配一个遗物类 + 遗物注册 + 遗物获取条件 | **一个补丁支持 N 种 Power**：只需在 Postfix 的 OfType 行里调用多个 `DispatchToPower<T>()` 即可 |
| **反模式风险** | 玩家身上会有大量"看不见但实际存在的监听遗物"，战斗结束时如果没正确清理，可能泄漏状态 | 零额外游戏实体。补丁只做转发，不持有任何玩家状态 |

#### 整体链路（不创建任何遗物）

```
CreatureCmd.Damage（解包内部结算完格挡）
    ↓
框架对所有 HookListener 广播 RelicModel.AfterDamageReceived
（N 个 Relic/Power/Card 监听器各自触发一次）
    ↓
★ Harmony Postfix：挂「方法」不挂「遗物实例」→ 偷听广播
    ├─ 过滤：UnblockedDamage > 0 且 target 身上有目标 Power
    ├─ 外层去重：_processedGlobalEvents（N 次广播只放行 1 次）
    └─ 遍历：target.Powers.OfType<YourPower>() → 逐个调用 ReceiveUnblockedDamage()
    ↓
★ Power 内部处理（每个实例独立）
    ├─ 防重入：_isTriggering 连锁触发标志
    ├─ 内层去重：_processedDamageEventIds（再保险，确保同一事件不重复扣血）
    ├─ 更新自定义状态：CurrentHealth -= UnblockedDamage
    └─ 触发阈值：CurrentHealth <= 0 → 爆炸/自爆/其他效果
    ↓
★ Description getter 动态注入（每次悬浮tip才计算，不用手动刷新）
    new LocString(...).Add("CurrentHealth", CurrentHealth).Add(...)
```

#### 为什么挂 `RelicModel.AfterDamageReceived` 而不是 `PowerModel.AfterTakeDamage`？

| Hook 点 | 参数里是否有 `DamageResult`（含 UnblockedDamage） | 需要遗物实例存在 | 推荐 |
|---------|---------------------------------------------------|----------------|------|
| `RelicModel.AfterDamageReceived(..., DamageResult result, ...)` | ✅ 有（`result.BlockedDamage` + `result.UnblockedDamage`，**格挡已完全算完**） | ❌ **不需要**（Patch 的是方法签名，挂在广播链出口偷听完就走） | ⭐⭐⭐ **唯一推荐** |
| `PowerModel.AfterTakeDamage(DamageResult result)` | ⚠️ 部分版本有，但 PowerModel 事件链在 CreatureCmd 中是单独广播，**顺序与 Relic 不同步**，且部分版本没有 DamageResult 重载 | ✅ Power 实例本身需要存在（当然有），但**事件广播不稳定** | ❌ 不推荐 |
| 直接 Patch `CreatureCmd.Damage` Postfix | ❌ 方法内部的 local 变量拿不到，且 `DamageResult` 还在栈上没构造完 | - | ❌ 极易 DLL 初始化失败（async 状态机 + 迭代链） |
| 传统模式：写一个配套 Relic 类实现 AfterDamageReceived | ✅ 有 | ✅ **必须创建遗物实例 + 注册遗物池 + 玩家获得遗物** | ❌ 耦合重 |

#### 完整可复制模板

##### 模板 Part 1：Harmony 广播补丁

```csharp
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MyModCode.Powers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MyModCode.Patches;

[HarmonyPatch]
public static class YourPowerDamagePatch
{
    private static MethodBase TargetMethod()
    {
        // ★ 精准定位带 DamageResult 的重载，避免匹配到其他重载
        return typeof(RelicModel).GetMethod("AfterDamageReceived",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[]
            {
                typeof(PlayerChoiceContext),
                typeof(Creature),       // target：受伤者
                typeof(DamageResult),   // result：★含 Blocked/Unblocked
                typeof(ValueProp),
                typeof(Creature),       // dealer：攻击者（可空）
                typeof(CardModel)       // cardSource：来源卡（可空）
            },
            null);
    }

    // ★ 外层去重：同一次伤害被 N 个 Relic 回调 N 次 -> 只放行一次
    private static readonly HashSet<int> _processedGlobalEvents = new();

    private static async void Postfix(
        PlayerChoiceContext choiceContext,
        Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        // 快速失败 4 层过滤（95% 调用在这里 return，性能极低开销）
        if (target == null || !target.IsAlive || result == null || result.UnblockedDamage <= 0)
            return;

        var powers = target.Powers?.OfType<YourStatefulPower>().ToList();
        if (powers == null || powers.Count == 0)
            return;

        // 引用地址哈希（值类型 DamageResult 取 boxed 引用地址稳定）
        int eventHashCode = target.GetHashCode()
                            ^ RuntimeHelpers.GetHashCode(result)
                            ^ (dealer != null ? RuntimeHelpers.GetHashCode(dealer) : 0)
                            ^ (cardSource != null ? RuntimeHelpers.GetHashCode(cardSource) : 0);

        if (!_processedGlobalEvents.Add(eventHashCode))
            return;

        // 防止超长战斗内存膨胀，超过阈值自动清空
        const int maxEvents = 4096;
        if (_processedGlobalEvents.Count > maxEvents)
            _processedGlobalEvents.Clear();

        // InstanceType=Instanced 时同一 Creature 上可能有多个独立实例 -> 逐个推送
        foreach (var power in powers)
        {
            power.ReceiveUnblockedDamage((int)result.UnblockedDamage, eventHashCode);
        }
    }
}
```

##### 模板 Part 2：Power 自接收 + 动态描述注入

```csharp
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Collections.Generic;

namespace MyModCode.Powers;

public class YourStatefulPower : PowerModel
{
    private static readonly PowerValueStore.PowerValues Values = MyPowerValues.YourStatefulPower;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced; // ★独立状态用 Instanced

    // ★ 自定义独立状态字段（决定了必须用 Instanced）
    public int CurrentHealth { get; set; } = (int)Values.Damage;
    public int CurrentEnergy { get; set; } = (int)Values.MagicNumber;
    public bool IsUpgraded { get; set; } = false;

    // 防重入 + 内层去重
    private bool _isTriggering = false;
    private readonly HashSet<int> _processedDamageEventIds = new();

    // ★★★ 动态描述：每次悬浮 tip 时才 new，注入实时状态（不需要手动刷新）
    public override LocString Description
    {
        get
        {
            var locString = new LocString("powers", base.Id.Entry + ".description");
            // 常量类注入（升级态/基础态覆盖）
            locString.Add("Energy", IsUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : CurrentEnergy);
            locString.Add("Health", IsUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage);
            // ★ 实时变化的自定义字段直接注入（CurrentHealth 扣了就立刻显示新值）
            locString.Add("CurrentHealth", CurrentHealth);
            return locString;
        }
    }

    // ★ 静态 Apply 方法（卡牌 OnPlay 调用这个，不要手写 OfType→ModifyAmount，会破坏 Instanced 语义）
    public static async Task<YourStatefulPower?> ApplyToCreature(Creature owner, bool isUpgraded = false)
    {
        var power = await PowerCmd.Apply<YourStatefulPower>(
            new ThrowingPlayerChoiceContext(), owner, 1m, owner, null);
        if (power != null)
        {
            power.CurrentEnergy = isUpgraded ? (int)(Values.MagicNumber + Values.MagicNumberUpgraded) : (int)Values.MagicNumber;
            power.CurrentHealth = isUpgraded ? (int)(Values.Damage + Values.DamageUpgraded) : (int)Values.Damage;
            power.IsUpgraded = isUpgraded;
        }
        return power;
    }

    // ★ 接收补丁推送来的未格挡伤害（由 Part 1 的 Postfix 逐个调用）
    public void ReceiveUnblockedDamage(int unblockedDamage, int eventHashCode)
    {
        if (Owner == null || !Owner.IsAlive || unblockedDamage <= 0) return;

        // 1. 防重入：效果过程中产生的新伤害（例如 Poison 扣血）直接忽略，防止连锁
        if (_isTriggering)
        {
            GD.Print($"[YourStatefulPower] 效果进行中，忽略连锁伤害 {unblockedDamage}");
            return;
        }

        // 2. 内层去重：再保险（如果外层 hash 因极端情况冲突，这里兜底）
        if (!_processedDamageEventIds.Add(eventHashCode))
        {
            GD.Print($"[YourStatefulPower] 同事件已处理，跳过 {eventHashCode:X8}");
            return;
        }

        // 3. 更新独立状态字段
        CurrentHealth -= unblockedDamage;
        GD.Print($"[YourStatefulPower] 受 {unblockedDamage} 点未格挡伤害，剩余 {CurrentHealth}");

        // 4. 阈值触发（示例：血量<=0 触发效果，可替换为其他阈值）
        if (CurrentHealth <= 0)
        {
            _ = TriggerEffectAsync();
        }
    }

    private async Task TriggerEffectAsync()
    {
        _isTriggering = true;
        try
        {
            // ... 你的效果：造成伤害、施加 Poison、清场等
            // 注意：PowerCmd.Apply 施加的 Poison/其他可能再次触发伤害，
            //       但 _isTriggering=true 已在 ReceiveUnblockedDamage 开头拦截，不会再连锁

            // 最后处理：Instanced 每个实例独立，一般直接 Remove 整个实例
            // 若你用 Amount 叠层（InstanceType=None），则改为 ModifyAmount -1 重置状态
            await PowerCmd.Remove(this);
        }
        finally
        {
            // 防御性代码：如果 Remove 成功（Owner不再包含this），永久锁死
            if (Owner == null || !Owner.Powers.Contains(this))
                _isTriggering = true;
        }
    }
}
```

#### 动态描述注入原理（为什么不需要手动刷新 UI？）

| 传统方式（不推荐） | 动态 getter 方式（推荐） |
|-------------------|------------------------------------|
| 把 Description 当缓存字段，在事件触发后手动修改或赋值 `this.description = ...` | **Description 是一个 `override LocString get` 属性，每次访问（鼠标悬浮）才 new 一个新 LocString 并注入当前字段值** |
| 问题：修改完容易漏掉某些入口，或框架内部缓存不刷新导致玩家看到旧数值 | **零手动刷新**：玩家鼠标悬浮的那个瞬间，拿到的永远是 CurrentHealth/CurrentEnergy 最新值 |

**正确/错误写法对比：**
```csharp
// 正确写法（每次 get 重新 new + 注入当前字段值）
public override LocString Description
{
    get
    {
        var locString = new LocString("powers", Id.Entry + ".description");
        locString.Add("CurrentHealth", CurrentHealth);  // 字段，不是常量
        return locString;
    }
}

// 错误写法 1：构造函数里 new 一次存字段，后续永远显示初始值
private LocString _cachedDesc;  // ← 字段缓存，永远不刷新
public MyPower() { _cachedDesc = new LocString(...).Add("CurrentHealth", Values.Damage); }
public override LocString Description => _cachedDesc;  // ← 永远是初始值

// 错误写法 2：Description.get 里注入常量，虽然每次 new 但显示旧值
locString.Add("CurrentHealth", Values.Damage);  // ← Values 是常量，不是 CurrentHealth 字段
```

#### 防重入（带副作用的伤害型效果最容易炸的点）

```
正常状态: _isTriggering = false
   ↓ 受击
触发效果: _isTriggering = true;  // 先设标志，再做任何副作用
   ↓ 施加 Poison（PowerCmd.Apply<PoisonPower>）
   ↓ Poison 立即结算回合内 tick（CreatureCmd.Damage）
   ↓ CreatureCmd.Damage 又广播 RelicModel.AfterDamageReceived
   ↓ 补丁又 Postfix，又遍历 OfType<YourStatefulPower>，又调 Receive()
   ↓ Receive() 第一行：
     if (_isTriggering) { GD.Print("效果进行中，忽略"); return; }  // ★就拦在这里！
   ↓ 不会再扣血，不会连锁触发第二次、第三次效果
   ↓ 施加完 Poison，回到 TriggerEffectAsync 尾部
   ↓ PowerCmd.Remove(this) 移除能力实例
```

如果没这个标志，典型日志会这样（连锁 30+ 次，因为 Poison 一直在结算里排队）：
```
受 11 点未格挡伤害，血量 -7 → 触发效果 → 受 11 点（再次扣血）→ 触发效果 → 受 11 点...
```

#### 常见坑速查

| 现象 | 根因 | 修复 |
|-----|------|------|
| 一刀伤害扣了 N 次血（N = 生物身上 Relic 数） | 只写了内层去重没写外层 `_processedGlobalEvents` | Part 1 补丁加全局 `HashSet<int>` 按引用 hash 去重 |
| 效果触发后，Poison 扣血又触发第二次效果 → 连锁 | 没加 `_isTriggering` 防重入标志 | `ReceiveUnblockedDamage` 开头 `if (_isTriggering) return;`，`TriggerEffectAsync` 第一行 `_isTriggering = true;` |
| CurrentHealth 描述显示和战斗日志对不上 | `Description.get` 里用了常量而不是注入字段值 | 必须 `locString.Add("CurrentHealth", CurrentHealth)` 注入**字段** |
| InstanceType=Instanced 但打了两张卡只看到一个图标 Amount=2 | `ApplyToCreature` 里写了 `OfType().FirstOrDefault() → ModifyAmount` 手动叠层 | 删掉手动叠层，直接 `PowerCmd.Apply<T>(amount: 1, ...)` 让框架按 Instanced 语义每次新建 |
| CurrentHealth 修改后没显示变化 → 显示的总是初始值 | 用了字段缓存（`private LocString _desc;` 或在构造函数里 new LocString 保存） | LocString 必须写在 Description getter 里，每次 get 重新 new |

#### 何时用旧模式（遗物监听）？

只有一种情况可以接受遗物模式：**监听逻辑天然和玩家身份绑定，且需要在多次战斗间持续生效**（比如"每场战斗开始时获得某效果"这种遗物效果）。除此之外，所有"战斗内、能力本身需要响应伤害"的情况，一律用 Power 自监听模式——代码更少、耦合更低、精度更高、不污染遗物池。

### 9.6 能力图标配置

由于 `PowerModel.Icon` 属性不是 `virtual` 的，无法通过重写来设置自定义图标。需要使用 `PowerIconPatch` 来拦截图标获取：

```csharp
[HarmonyPatch]
public static class PowerIconPatch
{
    // 能力类型到图标路径的映射字典
    private static readonly Dictionary<Type, string> _customIconPaths = new()
    {
        { typeof(MyBlockOnPlayBuff), "res://MyModResources/images/packed/powers/my_block_on_play_buff.png" },
        // 添加更多能力类型和图标路径
    };

    // 拦截 Icon 属性（战斗界面底部状态栏显示的小图标）
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.Icon), MethodType.Getter)]
    public static bool IconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        Type type = __instance.GetType();
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (ResourceLoader.Exists(iconPath))
            {
                __result = ResourceLoader.Load<Texture2D>(iconPath);
                return false; // 跳过原方法
            }
        }
        return true; // 执行原方法
    }

    // 拦截 PackedIconPath 属性
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.PackedIconPath), MethodType.Getter)]
    public static bool PackedIconPathPrefix(PowerModel __instance, ref string __result)
    {
        Type type = __instance.GetType();
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            __result = iconPath;
            return false;
        }
        return true;
    }

    // 拦截 BigIcon 属性（悬停提示时显示的大图标）
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerModel), nameof(PowerModel.BigIcon), MethodType.Getter)]
    public static bool BigIconPrefix(PowerModel __instance, ref Texture2D __result)
    {
        Type type = __instance.GetType();
        if (_customIconPaths.TryGetValue(type, out string iconPath))
        {
            if (ResourceLoader.Exists(iconPath))
            {
                __result = ResourceLoader.Load<Texture2D>(iconPath);
                return false;
            }
        }
        return true;
    }
}
```

**重要提示**：
1. **新增能力类型后，必须将其添加到 `_customIconPaths` 字典中**，否则图标将无法正常显示
2. **图标文件存放位置**：建议将能力图标放在 `<ModID>Resources/images/packed/powers/` 目录下
3. **图标路径格式**：`res://<ModID>Resources/images/packed/powers/<能力名称>Power.png`

**常见问题排查**：

| 检查项 | 说明 |
|--------|------|
| `_customIconPaths` 注册 | 确认能力类型已添加到字典中 |
| 图标文件路径 | 确认路径拼写正确，区分大小写 |
| 文件存在性 | 确认图标文件确实存在于指定位置 |
| 图标格式 | 确保是有效的 PNG 格式图片 |
| 缓存问题 | 尝试清理游戏缓存后重新测试 |

**调试技巧**：可以在 `IconPrefix` 方法中添加日志输出来验证是否正确拦截了图标获取：
```csharp
GD.Print($"[PowerIconPatch] 拦截能力图标: {type.FullName}, 路径: {iconPath}");
```

### 9.7 资源路径

```
res://images/powers/<能力ID小写>.png
res://images/atlases/power_atlas.sprites/<能力ID小写>.tres
```

### 9.8 本地化文本

`res://<ModID>/localization/zhs/powers.json`:
```json
{
  "MY_BLOCK_ON_PLAY_BUFF.title": "格挡精进",
  "MY_BLOCK_ON_PLAY_BUFF.description": "打出牌时会提供格挡。",
  "MY_BLOCK_ON_PLAY_BUFF.smartDescription": "每当你打出一张牌，获得 {Amount} 点[gold]格挡[/gold]。"
}
```

### 9.9 数值可变能力的叠加逻辑

对于数值会变化的能力（如每回合产出不同的同名能力），需要实现特殊的叠加逻辑：**检查数值相同的能力是否存在，存在便叠加上去，否则创建独立能力**。

**示例场景**：
- 打出两张基础能力（每回合产出 X）→ 叠加为 1 个能力，显示"XX 2"
- 打出一张基础能力（X）+ 一张升级能力（Y）→ 创建 2 个独立能力

**实现模式**：
```csharp
public static async Task ApplyYourPowers(Creature owner, int count, bool isUpgraded = false)
{
    // 计算目标数值
    int targetValuePerTurn = (int)Values.Value + (isUpgraded ? (int)Values.ValueUpgraded : 0);

    // 查找相同数值的能力
    var existingPower = owner.Powers
        .OfType<YourPower>()
        .FirstOrDefault(p => p.CurrentValuePerTurn == targetValuePerTurn);

    if (existingPower != null)
    {
        // 数值相同 → 叠加层数
        await PowerCmd.ModifyAmount(ctx, existingPower, count, owner, null);
    }
    else
    {
        // 数值不同 → 创建新能力
        var newPower = await PowerCmd.Apply<YourPower>(ctx, owner, count, owner, null);
        newPower.CurrentValuePerTurn = targetValuePerTurn;
    }
}
```

### 9.10 动态切换能力类型（Buff/Debuff）

能力类型可以根据状态动态切换，实现视觉上的状态区分（生产中/停产、激活/禁用等状态显示为不同颜色）。

**实现方式**：
```csharp
public class StatusPower : PowerModel
{
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 根据状态动态返回能力类型
    /// 激活 -> Buff（绿色数字）
    /// 禁用 -> Debuff（红色数字）
    /// </summary>
    public override PowerType Type => IsActive ? PowerType.Buff : PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
```

**效果说明**：
- 当 `IsActive = true`：能力图标显示为绿色边框，数字为绿色
- 当 `IsActive = false`：能力图标显示为红色边框，数字为红色

这种方式可以让玩家直观地通过颜色区分能力的当前状态。

---

## 10. 自定义事件（EventModel）

### 10.1 基础事件

```csharp
public class MyCustomEvent : EventModel
{
    protected override List<DynamicVar> CanonicalVars => new()
    {
        new StringVar("MyCustomCard", ModelDb.Card<MyCustomCard>().Title),
        new GoldVar(50)
    };

    public override bool IsAllowed(RunState runState) => true;

    protected override List<EventOption> GenerateInitialOptions()
    {
        return new List<EventOption>
        {
            new EventOption(this, ActMeditation, InitialOptionKey("MEDITATION")),
            new EventOption(this, ActRecharge, InitialOptionKey("LEAVE"))
        };
    }

    private async Task ActMeditation()
    {
        CardModel card = Owner.RunState.CreateCard<MyCustomCard>(Owner);
        await CardPileCmd.Add(card, PileType.Deck);
        SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.MEDITATION.description"));
    }

    private Task ActRecharge()
    {
        PlayerCmd.GainGold(DynamicVars.Gold.BaseValue, Owner);
        SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.LEAVE.description"));
        return Task.CompletedTask;
    }
}
```

### 10.2 多页选项

```csharp
private Task ActGaze()
{
    SetEventState(
        L10NLookup("MY_CUSTOM_EVENT.pages.GAZE.description"),
        new List<EventOption>
        {
            new EventOption(this, async () => {
                await RelicCmd.Obtain(ModelDb.Relic<Pear>().ToMutable(), Owner);
                SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.GAZE_PEAR.description"));
            }, "MY_CUSTOM_EVENT.pages.GAZE.options.PEAR", HoverTipFactory.FromRelic<Pear>())
        }
    );
    return Task.CompletedTask;
}
```

### 10.3 添加到游戏（HarmonyPatch）

```csharp
[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.AllEvents), MethodType.Getter)]
public static class OvergrowthAllEventsPatch
{
    static void Postfix(ref IEnumerable<EventModel> __result)
    {
        __result = __result.Concat(new[] { ModelDb.Event<MyCustomEvent>() }).Distinct();
    }
}
```

**章节类对应：**
- `Overgrowth` - 第一层（密林）
- `Hive` - 第二层
- `Beyond` - 第三层

### 10.4 资源路径

```
res://images/events/<事件ID小写>.png  # 背景图 (3440×1613)
```

### 10.5 本地化文本

`res://<ModID>/localization/zhs/events.json`:
```json
{
  "MY_CUSTOM_EVENT.title": "幻境苹果树",
  "MY_CUSTOM_EVENT.pages.INITIAL.description": "你的眼前出现一棵结满苹果的苹果树...",
  "MY_CUSTOM_EVENT.pages.INITIAL.options.MEDITATION.title": "沉思",
  "MY_CUSTOM_EVENT.pages.INITIAL.options.MEDITATION.description": "将一张 {MyCustomCard} 放入手牌。"
}
```

### 10.6 先古之民事件（AncientEventModel）

```csharp
public sealed class MyAncient : AncientEventModel
{
    public override List<EventOption> AllPossibleOptions => new()
    {
        new EventOption(this, TakeGold, InitialOptionKey("TAKE_GOLD"))
    };

    protected override AncientDialogueSet DefineDialogues()
    {
        return new AncientDialogueSet
        {
            FirstVisitEverDialogue = new AncientDialogue("第一次见面的对话文本"),
            CharacterDialogues = new Dictionary<string, IReadOnlyList<AncientDialogue>>
            {
                [CharKey<Ironclad>()] = new[] { new AncientDialogue("铁甲战士专属对话") { VisitIndex = 0 } }
            },
            AgnosticDialogues = new[] { new AncientDialogue("通用对话") }
        };
    }

    protected override List<EventOption> GenerateInitialOptions() => AllPossibleOptions.ToList();

    private async Task TakeGold() { await PlayerCmd.GainGold(30, Owner); Done(); }
}
```

**先古之民本地化键格式：**
```
<先古ID>.talk.firstVisitEver.<对话组索引>-<台词行索引>.ancient
<先古ID>.talk.<角色ID>.<索引>.ancient
<先古ID>.talk.ANY.<索引>r.ancient  # 注意带r
```

### 10.7 先古之民对话本地化（RitsuLib）

#### 核心原理

先古之民（Neow、建筑师 `THE_ARCHITECT`、Darv、Orobas 等）的对话通过 `ancients.json` 的本地化键定义。角色通过 `[RegisterCharacter]` 注册到 RitsuLib 后，RitsuLib 会在 `AncientDialogueSet.PopulateLocKeys` 执行前，自动把本地化表里属于该角色的对话**追加**进对话集——**无需也不应再写 Harmony 补丁硬编码对话**，否则会与 RitsuLib 的追加叠加，导致列表索引错位、生成缺失键。

#### 键名格式

```text
{ANCIENT}.talk.{角色Entry}.{对话序号}-{行序号}[r].{ancient|char}
{ANCIENT}.talk.{角色Entry}.{对话序号}-{行序号}[r].next
```

| 部分 | 说明 |
|------|------|
| `ANCIENT` | 先古 ID，如 `NEOW`、`THE_ARCHITECT` |
| `角色Entry` | 角色的 **ModelId Entry**（如 `<你的角色ID>`），不是短别名 |
| `对话序号` | 从 0 开始连续编号；扫描到首个缺失序号即停止 |
| `行序号` | 该段对话内的行，从 0 开始连续编号 |
| `r` | 可选；带 `r` 表示该段对话可重复（进入重复池，多次通关后仍会随机出现） |
| `ancient` / `char` | 发言者：`.ancient` 为先古说，`.char` 为角色说；同一行优先读 `.ancient` |
| `.next` | 除最后一行外，每行必须配"继续"按钮文本；不带 `.ancient/.char` 后缀 |

#### 建筑师（THE_ARCHITECT）特殊规则

- 对话序号即拜访序号：`VisitIndex = dialogueIndex`（RitsuLib 自动解析），角色第 N 次通关显示第 N-1 段（`charVisits = TotalWins`）
- 超过最后一段后，只从带 `r` 后缀的对话中随机复用；因此建议每段都加 `r`
- 攻击演出可选键（值为 `None` / `Player` / `Architect` / `Both`）：
  - `{对话序号}-attack`：结束攻击者
  - `{对话序号}-startattack`：开场攻击者
  - `{对话序号}-endattack`：结束攻击者（缺省 `Architect`）

#### Neow 等其他先古的规则

- 拜访序号映射：`0→0`、`1→1`、`2→4`，之后每段 `+3`（与原版 Neow 一致）
- 原版 Neow 对话不带 `r`，默认不重复

#### 完整示例

```json
{
  "NEOW.talk.<你的角色Entry>.0-0.ancient": "……欢迎……新的灵魂……",
  "NEOW.talk.<你的角色Entry>.0-0.next": "回应",
  "NEOW.talk.<你的角色Entry>.0-1.char": "我已准备好启程。",
  "NEOW.talk.<你的角色Entry>.0-1.next": "继续",
  "NEOW.talk.<你的角色Entry>.0-2.ancient": "……很好……去吧……",

  "THE_ARCHITECT.talk.<你的角色Entry>.0-0r.ancient": "你的到来令人印象深刻。",
  "THE_ARCHITECT.talk.<你的角色Entry>.0-0r.next": "回应",
  "THE_ARCHITECT.talk.<你的角色Entry>.0-1r.char": "目标明确！",
  "THE_ARCHITECT.talk.<你的角色Entry>.0-1r.next": "继续",
  "THE_ARCHITECT.talk.<你的角色Entry>.0-2r.ancient": "没有目的的智慧只是噪音。",
  "THE_ARCHITECT.talk.<你的角色Entry>.0-2r.next": "继续",
  "THE_ARCHITECT.talk.<你的角色Entry>.0-3r.char": "我们出发吧！",
  "THE_ARCHITECT.talk.<你的角色Entry>.0-endattack": "Both"
}
```

#### 注意事项

1. **角色 Entry 必须用真实 ID**：代码中通过 `ModelDb.GetId<MyCharacter>().Entry` 获取，不要写短别名，否则键永远不会被读到
2. **不要同时硬编码 `TheArchitect.DefineDialogues` 补丁**：RitsuLib 会按本地化键再追加一份，两套叠加后列表下标偏移，会出现 `THE_ARCHITECT.talk.XXX.7-0.char` 之类的缺失键（原样显示键名）
3. 修改 `ancients.json` 后需要重新导出 pck（本地化文件在 pck 内）；若同时改了代码，DLL 也要一起替换
4. 各语言文件（zhs / eng / jpn / kor 等）的键集合必须保持一致，只翻译值
5. 若某角色完全没配置对话，RitsuLib 的"空对话回退"需要开启调试兼容总开关 + Ancient/THE_ARCHITECT 兼容设置才会生效（避免 PROCEED 时空引用崩溃）；对话追加本身不依赖该开关

---

## 11. 自定义角色（CharacterModel）

### 11.1 角色基类

```csharp
public sealed class Watcher : CharacterModel
{
    public override int StartingHp => 72;
    public override int StartingGold => 99;
    public override CardPoolModel CardPool => ModelDb.CardPool<WatcherCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<WatcherRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<WatcherPotionPool>();
    public override CharacterModel? UnlocksAfterRunAs => null;  // null表示初始可用

    // UI颜色
    public override Color NameColor => Colors.Purple;
    public override Color DialogueColor => Colors.LightPurple;
    public override Color MapDrawingColor => Colors.Purple;
}
```

### 11.2 角色关联池

#### 卡池（CardPoolModel）
```csharp
public class WatcherCardPool : CardPoolModel
{
    public override string Title => "Watcher";  // 影响卡牌图标路径
    public override string EnergyColorName => "purple";
    public override string CardFrameMaterialPath => "materials/card_frame_watcher.tres";
    public override Color DeckEntryCardColor => Colors.Purple;
    public override Color EnergyOutlineColor => Colors.DarkPurple;

    public override List<CardModel> GenerateAllCards()
    {
        return new List<CardModel>
        {
            // 返回该角色卡池中的所有卡牌
        };
    }
}
```

#### 遗物池（RelicPoolModel）
```csharp
public class WatcherRelicPool : RelicPoolModel
{
    public override List<RelicModel> GenerateAllRelics()
    {
        return new List<RelicModel>
        {
            // 返回该角色专属遗物
        };
    }
}
```

#### 药水池（PotionPoolModel）
```csharp
public class WatcherPotionPool : PotionPoolModel
{
    public override List<PotionModel> GenerateAllPotions()
    {
        return new List<PotionModel>
        {
            // 返回该角色专属药水
        };
    }
}
```

### 11.3 必需资源列表

| 资源类型 | 路径 |
|---------|------|
| 待机动画场景 | `res://scenes/creature_visuals/<角色ID>.tscn` |
| 头像图标场景 | `res://scenes/ui/character_icons/<角色ID>_icon.tscn` |
| 能量计数器场景 | `res://scenes/combat/energy_counters/<角色ID>_energy_counter.tscn` |
| 商店待机动画 | `res://scenes/merchant/characters/<角色ID>_merchant.tscn` |
| 篝火休息动画 | `res://scenes/rest_site/characters/<角色ID>_rest_site.tscn` |
| 头像纹理 | `res://images/ui/top_panel/character_icon_<角色ID>.png` |
| 角色选择背景图 | `res://images/packed/character_select/char_select_<角色ID>.png` |
| 卡牌拖尾特效 | `res://scenes/vfx/card_trail_<角色ID>.tscn` |

**场景结构要点：**
所有角色的视觉场景根节点必须是挂载了特殊脚本（如 `NCreatureVisuals`）的 `Node2D`，内部必须包含名为 `%Visuals`, `%Bounds`, `%IntentPos`, `%CenterPos` 等特定子节点。

### 11.4 注册角色（HarmonyPatch）

```csharp
// 添加候选角色
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCharacters), MethodType.Getter)]
public static class AllCharactersPatch
{
    static void Postfix(ref IEnumerable<CharacterModel> __result)
    {
        __result = __result.Append(new Watcher()).Distinct();
    }
}

// 添加关联池
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.AllCardPools), MethodType.Getter)]
public static class AllCardPoolsPatch
{
    static void Postfix(ref IEnumerable<CardPoolModel> __result)
    {
        __result = __result.Append(ModelDb.CardPool<WatcherCardPool>()).Distinct();
    }
}

// 同理添加 AllRelicPools 和 AllPotionPools
```

### 11.5 本地化文本

`res://<ModID>/localization/zhs/characters.json`:
```json
{
  "WATCHER.title": "观者",
  "WATCHER.description": "一名目盲的修行者...",
  "WATCHER.pronounObject": "她"
}
```

### 11.6 重要注意事项

**Spine 动画导入：**
- Godot 不直接支持 Spine 动画
- 需要下载 Godot-Spine 的 GDExtension 插件并放置在 `bin/` 文件夹下
- Spine 的 JSON 文件后缀需改为 `.spine-json`

**脚本检索问题：**
```csharp
// 在ModInitializer中调用
Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());
```

**音效配置：**
- 固定 FMOD 路径格式：`event:/sfx/characters/<角色ID>/...`
- 可重写 `CharacterSelectSfx` 等属性借用其他角色音效

---

## 12. 自定义敌怪（MonsterModel + EncounterModel）

### 12.1 怪物基类

```csharp
public sealed class MyCustomMonster : MonsterModel
{
    public override int MinInitialHp => 30;
    public override int MaxInitialHp => 34;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState(
            "ATTACK_STATE",
            AttackMove,
            new SingleAttackIntent(8)
        );
        attack.FollowUpState = attack;  // 循环攻击

        return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
    }

    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(8)
            .FromMonster(this)
            .WithAttackerAnim("Attack", 0.2f)
            .WithHitFx("vfx/vfx_attack_blunt")  // 添加攻击特效
            .Execute(null);
    }
}
```

### 12.2 AI 行为模式

#### 简单循环
```csharp
MoveState attack = new MoveState("ATTACK", AttackMove, new SingleAttackIntent(8));
MoveState buff = new MoveState("BUFF", BuffMove, new BuffIntent());
attack.FollowUpState = buff;
buff.FollowUpState = attack;  // 攻击→强化→攻击→强化...
```

#### 随机分支
```csharp
RandomBranchState random = new RandomBranchState("RANDOM");
random.AddBranch(attack, MoveRepeatType.CannotRepeat, 0.7f);   // 70%概率攻击
random.AddBranch(defend, MoveRepeatType.CannotRepeat, 0.3f);   // 30%概率防御
```

#### 条件分支
```csharp
ConditionalBranchState condition = new ConditionalBranchState("CHECK_HP");
condition.AddState(bigAttack, () => Creature.CurrentHp <= 10);  // 生命≤10时强力攻击
condition.AddState(normalAttack, () => true);                   // 否则普通攻击
```

### 12.3 常用怪物意图

| 意图类 | 说明 |
|--------|------|
| `SingleAttackIntent(int damage)` | 单次攻击 |
| `MultiAttackIntent(int damage, int repeat)` | 多次攻击 |
| `AoeAttackIntent(int damage)` | 群体攻击 |
| `DefendIntent()` / `DefendIntent(int block)` | 防御 |
| `BuffIntent()` | 强化 |
| `DebuffIntent()` | 削弱 |
| `StatusIntent(int count)` | 添加状态牌 |
| `SummonIntent()` | 召唤其他怪物 |
| `EscapeIntent()` | 逃跑 |
| `StunIntent()` | 眩晕 |

### 12.4 遭遇类（EncounterModel）

```csharp
public sealed class MyCustomEncounter : EncounterModel
{
    public override RoomType RoomType => RoomType.Monster;  // Monster/Elite/Boss
    public override bool IsWeak => true;                    // 弱怪池

    public override List<MonsterModel> AllPossibleMonsters => new()
    {
        ModelDb.Monster<MyCustomMonster>()
    };

    protected override List<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (ModelDb.Monster<MyCustomMonster>().ToMutable(), null)  // null使用默认站位
        };
    }
}
```

### 12.5 注册遭遇（HarmonyPatch）

```csharp
[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.GenerateAllEncounters))]
public static class OvergrowthGenerateAllEncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result.Concat(new[] { ModelDb.Encounter<MyCustomEncounter>() }).Distinct();
    }
}
```

### 12.6 资源路径

```
res://scenes/creature_visuals/<怪物ID小写>.tscn
res://scenes/encounters/<遭遇ID小写>.tscn  # 可选，自定义站位
res://images/monsters/<怪物ID>/<怪物ID>_000.png
res://images/monsters/<怪物ID>/<怪物ID>_attack_000.png
```

**场景结构：**
```
NCreatureVisuals : Node2D
⨽ Node2D(%Visuals)
⨽ Control(%Bounds)
⨽ Marker2D(%IntentPos)
⨽ Marker2D(%CenterPos)
```

### 12.7 本地化文本

`res://<ModID>/localization/zhs/monsters.json`:
```json
{
  "MY_CUSTOM_MONSTER.name": "神秘生物",
  "MY_CUSTOM_MONSTER.moves.ATTACK_STATE.title": "攻击"
}
```

`res://<ModID>/localization/zhs/encounters.json`:
```json
{
  "MY_CUSTOM_ENCOUNTER.title": "神秘角色",
  "MY_CUSTOM_ENCOUNTER.loss": "{character}被{encounter}解决掉了。"
}
```

---

## 13. 攻击特效（VFX）

### 13.1 特效概述

攻击特效是增强战斗视觉体验的重要组成部分。游戏提供了多种内置特效，同时也支持自定义特效。

### 13.2 内置特效类型

游戏解包资源中包含丰富的攻击特效：

| 特效名称 | 路径 | 适用场景 |
|---------|------|---------|
| `vfx_attack_slash` | `res://scenes/vfx/vfx_attack_slash.tscn` | 斩击类攻击 |
| `vfx_attack_blunt` | `res://scenes/vfx/vfx_attack_blunt.tscn` | 钝器类攻击 |
| `vfx_attack_stab` | `res://scenes/vfx/vfx_attack_stab.tscn` | 突刺类攻击 |
| `vfx_attack_lightning` | `res://scenes/vfx/vfx_attack_lightning.tscn` | 闪电类攻击 |
| `vfx_attack_fire` | `res://scenes/vfx/vfx_attack_fire.tscn` | 火焰类攻击 |
| `vfx_attack_frost` | `res://scenes/vfx/vfx_attack_frost.tscn` | 冰霜类攻击 |
| `vfx_attack_poison` | `res://scenes/vfx/vfx_attack_poison.tscn` | 毒素类攻击 |
| `vfx_smoke_puff` | `res://scenes/vfx/vfx_smoke_puff.tscn` | 烟雾效果 |

### 13.3 在伤害命令中使用特效

最常见的使用方式是在 `DamageCmd` 中通过 `WithHitFx()` 方法添加特效：

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this, cardPlay)
        .Targeting(cardPlay.Target)
        .WithHitFx("vfx/vfx_attack_slash")  // 指定特效路径
        .Execute(choiceContext);
}
```

### 13.4 使用 VFX 节点类创建特效

除了通过 `DamageCmd`，还可以直接使用 VFX 节点类手动创建特效：

```csharp
// 创建刺击特效
var stabVfx = NStabVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(stabVfx);

// 创建斩击特效
var slashVfx = NSlashVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(slashVfx);

// 创建火焰燃烧特效（带持续时间）
var fireVfx = NFireBurningVfx.Create(target, duration: 1.5f, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);

// 创建毒药冲击特效
var poisonVfx = NPoisonImpactVfx.Create(target, goingRight: true);
NCombatRoom.Instance?.CombatVfxContainer.AddChild(poisonVfx);
```

### 13.5 常用 VFX 节点类速查

| 节点类 | 说明 | 参数 |
|-------|------|------|
| `NStabVfx` | 刺击特效 | target, goingRight |
| `NSlashVfx` | 斩击特效 | target, goingRight |
| `NFireBurningVfx` | 火焰燃烧特效 | target, duration, goingRight |
| `NPoisonImpactVfx` | 毒药冲击特效 | target, goingRight |
| `NSmokePuffVfx` | 烟雾特效 | position |

### 13.6 创建自定义特效场景

#### 步骤1：准备特效图片

创建帧序列图片，命名格式为 `vfx_my_effect_00-03.png`（00到03为帧索引）。

#### 步骤2：创建场景文件

```gdscript
# res://scenes/vfx/vfx_my_custom_attack.tscn
[gd_scene load_steps=3 format=3]

[ext_resource type="Texture2D" path="res://images/vfx/vfx_my_custom_attack_00-03.png" id="1"]
[ext_resource type="Script" path="res://scripts/vfx/my_custom_vfx.cs" id="2"]

[node name="MyCustomVfx" type="Node2D"]
script = ExtResource("2")

[node name="Sprite" type="Sprite2D" parent="."]
texture = ExtResource("1")
centered = false

[node name="AnimationPlayer" type="AnimationPlayer" parent="."]
```

#### 步骤3：创建 C# 脚本

```csharp
public class MyCustomVfx : Node2D
{
    [Export] public Sprite2D Sprite;
    [Export] public AnimationPlayer AnimationPlayer;

    public static MyCustomVfx Create(Creature target, bool goingRight = true)
    {
        var scene = GD.Load<PackedScene>("res://scenes/vfx/vfx_my_custom_attack.tscn");
        var instance = scene.Instantiate<MyCustomVfx>();

        // 设置位置
        instance.Position = target.Position;
        instance.Scale = new Vector2(goingRight ? 1 : -1, 1);

        return instance;
    }

    public override void _Ready()
    {
        // 播放动画后自动销毁
        AnimationPlayer.Play("attack");
        AnimationPlayer.AnimationFinished += (animName) => QueueFree();
    }
}
```

### 13.7 特效资源路径规范

```
res://scenes/vfx/vfx_<特效名称>.tscn        # 场景文件
res://images/vfx/vfx_<特效名称>_00-03.png   # 帧序列图片
res://images/atlases/vfx_atlas.sprites/<特效名称>.tres  # 裁切纹理
res://scripts/vfx/<特效名称>.cs             # C#脚本（可选）
```

### 13.8 实战示例：组合特效

在能力中组合多个特效：

```csharp
// 播放轰击特效
var hitVfx = NStabVfx.Create(target, goingRight: true);
if (hitVfx != null)
{
    NCombatRoom.Instance?.CombatVfxContainer.AddChild(hitVfx);
}

// 播放火焰燃烧特效
var fireVfx = NFireBurningVfx.Create(target, 1.5f, goingRight: true);
if (fireVfx != null)
{
    NCombatRoom.Instance?.CombatVfxContainer.AddChild(fireVfx);
}
```

### 13.9 特效性能优化

| 优化策略 | 说明 |
|---------|------|
| 复用场景 | 使用 `PackedScene` 复用而不是每次创建新场景 |
| 限制数量 | 避免同时播放过多特效 |
| 及时销毁 | 使用 `QueueFree()` 在动画结束后销毁节点 |
| 使用对象池 | 对于频繁使用的特效，考虑使用对象池模式 |

---

## 14. DamageVar 与增伤机制

游戏中的伤害数值通过 `DamageVar` 定义，其第二个参数 `ValueProp` 决定了伤害是否能受到增益效果（如力量加成、迟缓 debuff 增伤等）的影响。

### 14.1 ValueProp 枚举类型

| 枚举值 | 说明 | 是否受增伤buff影响 |
|--------|------|------------------|
| `ValueProp.Move` | 攻击卡牌造成的伤害 | ✅ 是 |
| `ValueProp.Unpowered` | 能力/遗物/药水造成的伤害 | ❌ 否 |

### 14.2 使用示例

**攻击卡牌（受增伤 buff 影响）**：
```csharp
// 攻击卡牌使用 ValueProp.Move，伤害会受到力量等buff加成
protected override List<DynamicVar> CanonicalVars => new List<DynamicVar>
{
    new DamageVar(6m, ValueProp.Move)
};
```

**能力卡牌（不受增伤 buff 影响）**：
```csharp
// 能力/回合触发伤害使用 ValueProp.Unpowered，不受力量等buff加成
protected override List<DynamicVar> CanonicalVars => new List<DynamicVar>
{
    new DamageVar(8m, ValueProp.Unpowered)
};
```

### 14.3 关键原则

| 伤害来源 | 是否受增伤 buff | ValueProp |
|---------|--------------|-----------|
| 攻击卡（直接打出造成的伤害） | ✅ | `ValueProp.Move` |
| 技能卡（非能力类，直接打出） | ✅ | `ValueProp.Move` |
| 能力卡（Power 回合触发的伤害） | ❌ | `ValueProp.Unpowered` |
| 遗物/药水伤害 | ❌ | `ValueProp.Unpowered` |

**要点**：
- 所有通过打出卡牌直接造成的伤害（攻击卡、技能卡）应使用 `ValueProp.Move`
- 所有通过能力（Power）回合触发造成的伤害应使用 `ValueProp.Unpowered`

---

## 15. 本地化键名规则

| 类型 | 键格式 | 文件 |
|------|--------|------|
| 卡牌 | `CARD_ID.title/description` | cards.json |
| 遗物 | `RELIC_ID.title/description/flavor` | relics.json |
| 药水 | `POTION_ID.title/description` | potions.json |
| 能力 | `POWER_ID.title/smartDescription` | powers.json |
| 事件 | `EVENT_ID.pages.INITIAL.options.OPT.title` | events.json |
| 角色 | `CHAR_ID.title/description` | characters.json |
| 怪物 | `MONSTER_ID.name/moves.STATE.title` | monsters.json |
| 遭遇 | `ENCOUNTER_ID.title/loss` | encounters.json |
| 附魔 | `ENCHANT_ID.title/description` | enchantments.json |
| 自定义词条/UI文本 | `keyword.title/description` 或 `ui.xxx` | card_keywords.json |

**ID 转换规则**：`MyClassName` → `MY_CLASS_NAME`（类名自动转换为大写加下划线格式）

- `MyCustomCard` → `MY_CUSTOM_CARD`
- `MyCustomRelic` → `MY_CUSTOM_RELIC`
- `MyCustomMonster` → `MY_CUSTOM_MONSTER`

---

## 16. UI 选择页面本地化配置

### 16.1 核心原理

游戏仅加载特定名称的 JSON 本地化文件，自定义的 `ui_strings.json`、`engineer_choices.json` 等文件**不会被游戏自动识别**。自定义 UI 文本必须整合到游戏原生支持的本地化文件中，推荐使用 `card_keywords.json`。

### 16.2 游戏支持的本地化文件

| 文件 | 用途 |
|------|------|
| `cards.json` | 卡牌标题和描述 |
| `card_keywords.json` | 卡牌词条、自定义 UI 文本 |
| `powers.json` | 能力标题和描述 |
| `relics.json` | 遗物标题和描述 |
| `characters.json` | 角色标题和描述 |
| `monsters.json` | 怪物名称和动作 |
| `events.json` | 事件标题和选项 |
| `ancients.json` | 先古之民内容 |
| `modifiers.json` | 修饰词 |

### 16.3 实现步骤

#### 1. 在 card_keywords.json 中添加本地化键

```json
{
    "ui.card_select.title_multi": "请选择 1-{count} 张牌",
    "ui.card_select.title_single": "请选择一张牌",
    "ui.card_select.cost_label": "费用",
    "ui.deploy_choice.title": "选择行动",
    "ui.deploy_choice.confirm": "确认选择",
    "ui.deploy_choice.cancel": "X 取消"
}
```

#### 2. 在 UI 类中添加 GetLocStringText 方法

```csharp
private string GetLocStringText(object? locStringObj)
{
    if (locStringObj == null) return string.Empty;
    if (locStringObj is string str) return str;

    System.Reflection.MethodInfo? rawMethod = locStringObj.GetType().GetMethod("GetRawText");
    if (rawMethod != null)
    {
        object? result = rawMethod.Invoke(locStringObj, null);
        if (result is string rawText && !string.IsNullOrEmpty(rawText))
        {
            return rawText;
        }
    }

    System.Reflection.MethodInfo? formatMethod = locStringObj.GetType().GetMethod("GetFormattedText");
    if (formatMethod != null)
    {
        try
        {
            object? result = formatMethod.Invoke(locStringObj, null);
            if (result is string formattedText && !string.IsNullOrEmpty(formattedText))
            {
                return formattedText;
            }
        }
        catch { }
    }

    string toString = locStringObj.ToString() ?? string.Empty;
    if (!toString.StartsWith("MegaCrit.Sts2.Core.Localization") && !toString.Contains("LocString"))
    {
        return toString;
    }

    return string.Empty;
}
```

#### 3. 在代码中使用 LocString

```csharp
// 简单文本
Text = GetLocStringText(new LocString("card_keywords", "ui.card_select.title_single"));

// 带动态变量的文本
var titleLocString = new LocString("card_keywords", "ui.card_select.title_multi");
titleLocString.Add("count", _maxSelection);
Text = GetLocStringText(titleLocString);

// 在 ChoiceOption 类中使用
public class ChoiceOption
{
    public object Title { get; set; } = string.Empty;
    public object Description { get; set; } = string.Empty;
}

// 创建选项时使用 LocString
new MyChoiceScreen.ChoiceOption
{
    Title = new LocString("card_keywords", "ui.deploy_choice.title"),
    Description = new LocString("card_keywords", "ui.deploy_choice.confirm")
}
```

### 16.4 命名空间约定

为了避免键名冲突，建议使用以下命名空间前缀：

| 前缀 | 用途 |
|------|------|
| `ui.card_select.xxx` | 卡牌选择界面 |
| `ui.production_queue.xxx` | 生产序列界面 |
| `ui.deploy_choice.xxx` | 部署/行动选择界面 |
| `engineer_choice.xxx` | 专属选项界面 |
| `ui.<你的面板>.xxx` | 其他自定义面板 |

### 16.5 动态变量替换

LocString 支持动态变量，使用 `{变量名}` 格式：

```json
{
    "ui.card_select.title_multi": "请选择 1-{count} 张牌",
    "ui.my_panel.desc": "造成 {Damage} 点伤害，赋予 1 层易伤"
}
```

在代码中添加变量：

```csharp
var locString = new LocString("card_keywords", "ui.my_panel.desc");
locString.Add("Damage", DynamicVars.Damage.BaseValue);
Text = GetLocStringText(locString);
```

### 16.6 支持的 Add 方法重载

```csharp
locString.Add("name", decimal value);    // 数值
locString.Add("name", bool value);       // 布尔值
locString.Add("name", string value);     // 字符串
locString.Add("name", IList<string> value); // 字符串列表
locString.Add("name", LocString value);  // 嵌套本地化字符串
```

### 16.7 注意事项

1. **不要创建自定义 JSON 文件**：游戏不会自动加载非标准名称的本地化文件
2. **使用 object 类型**：`ChoiceOption` 的 Title 和 Description 应定义为 `object` 类型，同时支持 `string` 和 `LocString`
3. **统一使用 GetLocStringText**：所有显示文本的地方都应通过此方法处理
4. **多语言文件同步**：修改中文 `zhs/card_keywords.json` 后，必须同步修改英文 `eng/card_keywords.json` 等文件

---

## 17. 控制台命令

```bash
card CARD_ID                    # 获得卡牌
addcard CARD_ID                 # 添加到卡组
relic RELIC_ID                  # 获得遗物
potion POTION_ID                # 获得药水
power POWER_ID AMOUNT TARGET    # 施加能力（0=玩家，1+=敌人）
enchant ENCHANT_ID AMOUNT INDEX # 为手牌附魔
event EVENT_ID                  # 触发事件
ancient ANCIENT_ID              # 触发先古之民
fight ENCOUNTER_ID              # 进入遭遇战
```

---

## 18. 关键 API 速查

### 伤害命令
```csharp
await DamageCmd.Attack(damage)
    .FromCard(card, cardPlay) / .FromMonster(monster) / .FromOsty(osty)
    .Targeting(creature) / .TargetingAllOpponents(state)
    .WithHitFx("vfx/path")
    .Execute(context);
```

### 能力命令
```csharp
await PowerCmd.Apply<PowerType>(target, amount, source, sourceCard);
await PowerCmd.Remove(powerInstance);
await PowerCmd.Decrement(powerInstance);
```

### 玩家命令
```csharp
await PlayerCmd.GainEnergy(amount, owner);
await PlayerCmd.GainGold(amount, owner);
await PlayerCmd.GainBlock(amount, owner);
```

### 生物命令
```csharp
await CreatureCmd.Damage(context, target, amount, props, source, card);
await CreatureCmd.Heal(target, amount);
await CreatureCmd.GainBlock(target, amount, props, card, fast);
```

### 卡牌命令
```csharp
await CardPileCmd.Add(card, PileType.Deck);
await CardPileCmd.Draw(context, count, owner);
await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, owner);
CardCmd.Enchant<EnchantType>(card, amount);
CardCmd.RemoveKeyword(card, CardKeyword.Exhaust);
CardCmd.Upgrade(card);
CardCmd.Transform(selectedCard, replacement);
```

### 遗物命令
```csharp
await RelicCmd.Obtain(relic.ToMutable(), owner);
```

---

## 19. 开发最佳实践

1. **保留解包的游戏源代码**，方便查阅 API 和资源路径
2. **使用 Harmony 时注意版本兼容性**，确保与 .NET 9.0 兼容
3. **资源路径严格区分大小写**，与游戏原版保持一致
4. **每次修改代码后重新构建 DLL**
5. **每次修改资源后重新导出 PCK**
6. **美化包可将 `affects_gameplay` 设为 `false`**
7. **使用 `Log.Info()` 或 `GD.Print()` 输出调试信息**
8. **数值集中存储**，卡牌类中引用数值存储类而非硬编码
9. **伤害/格挡变量在描述中使用 `{Damage:diff()}` 格式**，保证 buff 修正实时显示
10. **随机数使用游戏提供的 `RunState.Rng`（而非 `new Random()`）**，便于结果复现与调试

---

## 20. 快速开始检查清单

- [ ] 安装 Megadot 编辑器
- [ ] 配置 .NET 9.0 环境
- [ ] 创建 Godot 项目
- [ ] 添加 sts2.dll 和 0Harmony.dll 引用
- [ ] 创建 ModInitializer 入口类
- [ ] 编写 JSON 配置文件
- [ ] 实现第一个功能（遗物/卡牌/角色等）
- [ ] 导出 PCK 和 DLL
- [ ] 放入 mods 文件夹测试
- [ ] 使用控制台命令验证功能

---

## 21. 附录：游戏解包资源结构

游戏解包目录（如 `D:\RedAlert2Project\SlayTheSpire2Export_beta\`）包含以下资源，可作为 Mod 资源路径与 API 参考：

```
SlayTheSpire2Export/
├── resources/
│   ├── scenes/
│   │   ├── creature_visuals/      # 角色/怪物待机动画
│   │   │   ├── ironclad.tscn
│   │   │   ├── silent.tscn
│   │   │   └── ...
│   │   ├── vfx/                   # 特效场景
│   │   │   ├── vfx_attack_slash.tscn
│   │   │   ├── vfx_attack_blunt.tscn
│   │   │   └── ...
│   │   ├── ui/
│   │   │   └── character_icons/   # 角色头像图标
│   │   └── combat/
│   │       └── energy_counters/   # 能量计数器
│   ├── images/
│   │   ├── packed/
│   │   │   └── character_select/  # 角色选择立绘
│   │   ├── atlases/               # 裁切纹理
│   │   ├── vfx/                   # 特效图片
│   │   ├── relics/                # 遗物图标
│   │   ├── potions/               # 药水图标
│   │   └── powers/                # 能力图标
│   └── localization/              # 本地化文件
└── src/
    └── Core/
        └── Nodes/
            └── Vfx/                # VFX节点类
                ├── NStabVfx.cs
                ├── NSlashVfx.cs
                ├── NFireBurningVfx.cs
                └── ...
```

另外，游戏安装目录的 `data_sts2_<platform>/` 包含 `sts2.dll` 与 `0Harmony.dll`（Mod 依赖），解包源码中的 `CombatManager.cs`、`PowerCmd.cs`、`RelicModel.cs` 等是排查 API 行为的第一手资料。

---

*本文档基于《杀戮尖塔2》官方 Mod 开发指南，适用于 Godot 引擎和 C# 语言开发。*
