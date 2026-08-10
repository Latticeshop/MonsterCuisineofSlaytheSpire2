# 尖塔乐事（Spire Delight）

《杀戮尖塔2》Mod：击杀对应怪物时掉落本 Mod 的料理卡牌；篝火处新增"料理"选项（与原版休息事件独立），选择 2-4 张料理卡烹饪，根据料理参数合成对应遗物。

> 注：Mod ID 与三个编译文件统一为 `SpireDelight`；游戏内展示名为"尖塔乐事（Spire Delight）"。

料理特点：
- 每次休息站仅可料理一次（与原版休息/锻造等选项相互独立）
- 料理会消耗本次休息次数（与原版休息/锻造等选项一致）
- 料理卡描述中的"打出 X 次后从牌组移除"会随打出次数动态减少（5→4→3…）

## 分工

- 代码与编译 `.dll`：Codex
- Godot 编译 `.pck` + 移植游戏测试：你（用户）

## 目录结构

```
SpireDelight.csproj            # C# 项目
SpireDelight.json              # Mod 清单（id=SpireDelight，与 .dll/.pck 同名）
project.godot                  # Godot 项目配置
libs/                          # 游戏依赖（sts2.dll / 0Harmony.dll / GodotSharp.dll）
MonsterCuisineCode/             # C# 源码
  Cards/                       # 18 张料理卡 + 基类 + 数值存储
  Powers/                      # 蛇果肉/禁出牌/攻击费用提升
  Relics/                      # 5 个料理遗物
  RestSite/                    # 篝火"料理"选项
  Patches/                     # 怪物掉落补丁 / 百科已发现补丁 / 休息站补丁
  Utils/                       # 料理参数 + 料理系统（配方可扩展）
SpireDelight/                   # Godot 资源（本地化等，最终打进 .pck）
  localization/zhs|eng/        # 本地化 JSON
MonsterCuisineResources/        # 素材资源（图片等）
  image/Cards/                 # 卡牌图片（小啃兽肉.jpg 等）
  image/Relics/                # 遗物图片（肉丸.jpg 等）
  image/RestSite/              # 休息站选项图标（料理.png）
build/                         # 构建输出（.dll/.json）
```

## 编译（.dll）

```bash
.\build.ps1
# 等价于：
# $env:NUGET_PACKAGES = "C:\Users\Latticeshop\.nuget\packages"
# dotnet build SpireDelight.csproj -c Debug -o build
```

产物：`build/SpireDelight.dll` + `build/SpireDelight.json`。

> 说明：本机 NuGet 缓存已包含 `Godot.NET.Sdk/4.5.1`，项目根目录的 `NuGet.config` 指向本地缓存，无需联网。

## 导出 .pck（你负责）

1. 用 Megadot（Godot 4.5.1 mono）打开项目根目录的 `project.godot`
2. 项目 → 导出 → Windows Desktop → **导出 PCK/ZIP**
3. 输出为 `build/SpireDelight.pck`（取消"使用调试导出"与"导出为补丁"）

## 部署到游戏

把以下三个同名文件放进游戏 `mods/SpireDelight/` 目录：

```
SpireDelight.dll
SpireDelight.pck
SpireDelight.json
```

游戏日志：`C:\Users\<用户名>\AppData\Roaming\SlayTheSpire2\logs\godot.log`

## 料理配方（当前默认，可在 `CookingManager.cs` 扩展）

每次料理**只合成一项遗物**，按优先级判定：特殊料理 ＞ 混合料理 ＞ 粘稠度 ＞ 怪物度 ＞ 蜜度 ＞ 鱼度 ＞ 贝度 ＞ 肉度 ＞ 素度。

| 条件 | 遗物 | 效果 |
|------|------|------|
| 石头 + 任意其他食材 | 烤石子 | 前三个回合开始时获得 7 格挡 |
| 一次放入两个特殊料理 | 失败料理 | 无效果 |
| 素度 2~3.5 且 肉度 2~3.5 | 汉堡包 | 战斗开始获得 3 再生；第一回合 +1 能量 +1 抽牌 |
| 仅有肉度且 2~3.5 | 肉丸 | 前两个回合 +1 抽牌 +1 能量 |
| 肉度 ≥ 4 | 大锅肉 | 前三个回合 +1 抽牌 +1 能量 |
| 不满足任何要求 | 失败料理 | 无效果 |

已拥有的遗物不会重复获得（卡牌仍会被移除）。蜜度/鱼度/贝度维度已在系统中预留，后续配方可直接扩展。

> 本地化：遗物除 `title`/`description` 外还需同时提供中英文 `.flavor`（小字风味文本），否则会回退显示英文。

## 图片资源配置

已配置（其余暂用游戏默认回退图）：

| 内容 | 文件 | 代码位置 |
|------|------|----------|
| 小啃兽肉 卡牌 | `MonsterCuisineResources/image/Cards/小啃兽肉.jpg` | `NibbitMeat.PortraitPath` |
| 肉丸 遗物 | `MonsterCuisineResources/image/Relics/肉丸.jpg` | `Meatball.PackedIconPath` |
| 料理 选项图标 | `MonsterCuisineResources/image/RestSite/料理.png` | `RestSiteOptionPatches` |

卡牌用 `PortraitPath` 覆盖、遗物用 `PackedIconPath` 覆盖、休息站选项在图标补丁中指定；均带 `ResourceLoader.Exists` 回退，图片缺失时自动使用默认图。后续卡牌/遗物图片放入对应目录并仿照配置即可。

## 百科（图鉴）查阅

- 卡牌注册进原版**无色卡池**（Token 稀有度不参与默认卡牌奖励/商店）
- 遗物注册进原版**事件遗物池**（Event 稀有度不参与默认遗物奖励）
- 打开图鉴时会自动把本 Mod 内容标记为"已见"，无需先获得即可直接查阅
