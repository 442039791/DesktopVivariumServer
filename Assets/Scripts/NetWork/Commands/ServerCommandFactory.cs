using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Commands
{
    /// <summary>
    /// 服务器命令工厂 - 负责创建和管理所有命令
    /// </summary>
    public class ServerCommandFactory
    {
        private static ServerCommandFactory _instance;
        public static ServerCommandFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServerCommandFactory();
                }
                return _instance;
            }
        }

        private readonly Dictionary<CommandType, IServerCommand> _commands;

        private ServerCommandFactory()
        {
            _commands = new Dictionary<CommandType, IServerCommand>();
            RegisterAllCommands();
        }

        /// <summary>
        /// 注册所有命令
        /// </summary>
        private void RegisterAllCommands()
        {
            // 生物罐管理命令
            RegisterCommand(new SetBioTankIDCommand());

            // 植物/方块管理命令
            RegisterCommand(new CreatePlantCubeCommand());
            RegisterCommand(new RemovePlantCubeCommand());
            RegisterCommand(new PlantReEditCommand());

            // 蓝图命令
            RegisterCommand(new BluePrintClientCommand());
            RegisterCommand(new BluePrintBioTankCommandHandler());

            // 窗口管理命令
            RegisterCommand(new ResetWindowPositionCommand());
            RegisterCommand(new SyncWindowPositionCommand());

            // 日志命令
            RegisterCommand(new LogCommand());

            // 放置结果命令
            RegisterCommand(new PlacementResultCommand());

        }

        /// <summary>
        /// 注册单个命令
        /// </summary>
        private void RegisterCommand(IServerCommand command)
        {
            if (command == null)
            {
                Debug.LogError("[CommandFactory] 尝试注册null命令");
                return;
            }

            if (_commands.ContainsKey(command.Type))
            {
                Debug.LogWarning($"[CommandFactory] 命令类型 {command.Type} 已存在,将被覆盖");
            }

            _commands[command.Type] = command;
        }

        /// <summary>
        /// 获取命令实例
        /// </summary>
        public IServerCommand GetCommand(CommandType type)
        {
            if (_commands.TryGetValue(type, out var command))
            {
                return command;
            }

            Debug.LogWarning($"[CommandFactory] 未找到命令类型: {type}");
            return null;
        }

        /// <summary>
        /// 检查命令是否已注册
        /// </summary>
        public bool HasCommand(CommandType type)
        {
            return _commands.ContainsKey(type);
        }

        /// <summary>
        /// 获取所有已注册的命令类型
        /// </summary>
        public IEnumerable<CommandType> GetRegisteredCommandTypes()
        {
            return _commands.Keys;
        }

        /// <summary>
        /// 动态注册新命令 (用于扩展)
        /// </summary>
        public void RegisterCustomCommand(IServerCommand command)
        {
            RegisterCommand(command);
        }
    }
}
