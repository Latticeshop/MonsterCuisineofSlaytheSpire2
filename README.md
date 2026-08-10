# 怪物料理（Monster Cuisine）

《杀戮尖塔2》Mod：击杀对应怪物时掉落本 Mod 的怪物料理卡牌；篝火处新增"料理"选项（与原版休息事件独立），选择 2-4 张料理卡烹饪，根据料理参数（粘稠度/素度/肉度/怪物度/特殊料理）获得对应遗物。

料理特点：
- 每次休息站仅可料理一次（与原版休息/锻造等选项相互独立）
- 不消耗休息次数：料理后仍可选择休息/锻造等原版选项
- 料理卡描述中的"打出 X 次后从牌组移除"会随打出次数动态减少（5→4→3…）

## 分工

- 代码与编译 `.dll`：Codex
- Godot 编译 `.pck` + 移植游戏测试：你（用户）

## 目录结构

```
MonsterCuisine.csproj          # C# 项目
MonsterCuisine.json            # Mod 清单（与 .dll/.pck 同名）
project.godot                  # Godot 项目配置
libs/                          # 游戏依赖（sts2.dll / 0Harmony.dll / GodotSharp.dll）
MonsterCuisineCode/             # C# 源码
  Cards/                       # 18 张料理卡 + 基类 + 数值存储
  Powers/                      # 蛇果肉/禁出牌/攻击费用提升
  Relics/                      # 5 个料理遗物
  Pools/                       # 卡池/遗物池
  RestSite/                    # 篝火"料理"选项
  Patches/                     # 怪物掉落补丁 / ModelDb 池注册补丁
  Utils/                       # 料理参数 + 料理系统（配方可扩展）
MonsterCuisine/                 # Godot 资源（本地化等，最终打进 .pck）
  localization/zhs|eng/        # 本地化 JSON
build/                         # 构建输出（.dll/.json）
```

## 编译（.dll）

```bash
.\build.ps1
# 等价于：
# $env:NUGET_PACKAGES = "C:\Users\Latticeshop\.nuget\packages"
# dotnet build MonsterCuisine.csproj -c Debug -o build
```

产物：`build/MonsterCuisine.dll` + `build/MonsterCuisine.json`。

> 说明：本机 NuGet 缓存已包含 `Godot.NET.Sdk/4.5.1`，项目根目录的 `NuGet.config` 指向本地缓存，无需联网。

## 导出 .pck（你负责）

1. 用 Megadot（Godot 4.5.1 mono）打开项目根目录的 `project.godot`
2. 项目 → 导出 → Windows Desktop → **导出 PCK/ZIP**
3. 输出为 `build/MonsterCuisine.pck`（取消"使用调试导出"与"导出为补丁"）

## 部署到游戏

把以下三个同名文件放进游戏 `mods/MonsterCuisine/` 目录：

```
MonsterCuisine.dll
MonsterCuisine.pck
MonsterCuisine.json
```

游戏日志：`C:\Users\<用户名>\AppData\Roaming\SlayTheSpire2\logs\godot.log`

## 料理配方（当前默认，可在 `CookingManager.cs` 扩展）

| 条件 | 遗物 | 效果 |
|------|------|------|
| 素度 ≥ 2 | 素食沙拉 | 每场战斗开始获得 1 敏捷 |
| 肉度 ≥ 2 | 肉食大餐 | 每场战斗开始获得 1 力量 |
| 粘稠度 ≥ 2 | 粘稠甜点 | 每场战斗开始回复 5 生命 |
| 怪物度 ≥ 1 | 黑暗料理 | 每场战斗开始获得 2 力量 + 1 易伤 |
| 特殊料理 ≥ 1 | 秘制料理 | 每场战斗开始获得 1 能量 |

同时满足多个条件时会获得多个遗物；已拥有的遗物不会重复获得。
> 遗物配方与效果为占位设计，后续根据《怪物料理.txt》调整。
