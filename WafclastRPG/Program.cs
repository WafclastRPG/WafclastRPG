﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using WafclastRPG.DataBases;

namespace WafclastRPG
{
    public class Program
    {
        public Config ConfigFile { get; private set; }
        public BotInfo BotInfo { get; private set; }
        public DataBase Database { get; private set; }

        static void Main() => new Program().RodarBotAsync().GetAwaiter().GetResult();

        public async Task RodarBotAsync()
        {
            ConfigFile = Config.LoadFromJsonFile("Config.json");
            if (ConfigFile == null)
            {
                Console.WriteLine("O arquivo config.json não existe!");
                Console.WriteLine("Coloque as informações necessarias no arquivo gerado!");
                Console.WriteLine("Aperte qualquer botão para sair...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            #region Configs
#if DEBUG
            var token = ConfigFile.TokenDebug;
            ConfigFile.PrefixRelease = ConfigFile.PrefixDebug;
            var logLevel = LogLevel.Debug;
#else
            var token = ConfigFile.TokenRelease;
            var logLevel = LogLevel.Debug;
#endif
            #endregion

            Bot bot = new Bot(new DiscordConfiguration
            {
                TokenType = TokenType.Bot,
                ReconnectIndefinitely = true,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                AutoReconnect = true,
                Token = token,
                Intents = DiscordIntents.All,
                MinimumLogLevel = logLevel,
            });

            Database = new DataBase();
            BotInfo = new BotInfo();
            var services = new ServiceCollection()
                .AddSingleton(Database)
                .AddSingleton(ConfigFile)
                .AddSingleton(BotInfo)
                .BuildServiceProvider();

            bot.ModuleCommand(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefixAsync,
                EnableDms = false,
                CaseSensitive = false,
                EnableDefaultHelp = false,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
                Services = services,
            }, Database);

            Database.MineDropBuilder();

            await bot.ConectarAsync();
            await Task.Delay(-1);
        }

        private async Task<int> ResolvePrefixAsync(DiscordMessage msg)
        {
            // Private Messages
            var gld = msg.Channel.Guild;
            if (gld == null)
                return await Task.FromResult(-1);
            if (Database.IsExecutingInteractivity(msg.Author.Id))
                return await Task.FromResult(-1);
#if DEBUG
            var prefix = await Database.GetServerPrefixAsync(gld.Id, ConfigFile.PrefixDebug);
            var pfixLocation = msg.GetStringPrefixLength(prefix);
#else
            var prefix = await Database.GetServerPrefixAsync(gld.Id, ConfigFile.PrefixRelease);
            var pfixLocation = msg.GetStringPrefixLength(prefix);
            if (pfixLocation == -1)
                pfixLocation = msg.GetStringPrefixLength(ConfigFile.PrefixRelease.ToLower());
            if (pfixLocation == -1)
                pfixLocation = msg.GetStringPrefixLength(ConfigFile.PrefixRelease.ToUpper());
#endif
            return await Task.FromResult(pfixLocation);
        }
    }
}
