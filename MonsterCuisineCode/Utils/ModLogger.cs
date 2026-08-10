using MegaCrit.Sts2.Core.Logging;

namespace MonsterCuisineCode.Utils;

/// <summary>尖塔乐事统一日志（游戏日志中前缀 [SpireDelight]）。</summary>
public static class ModLogger
{
    public static Logger Instance { get; } =
        new("SpireDelight", LogType.Generic);
}
