﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System.Text;
using System.Threading.Tasks;
using WafclastRPG.Game;
using WafclastRPG.Game.Entidades;

namespace WafclastRPG.Bot.Comandos.Exibir
{
    public class ComandoRank : BaseCommandModule
    {
        public Banco banco;

        [Command("top")]
        [Description("Exibe os 10 jogadores mais ricos.")]
        [Cooldown(1, 30, CooldownBucketType.User)]
        public async Task ComandoMochilaAsync(CommandContext ctx)
        {
            var top = await banco.Jogadores.Find(FilterDefinition<WafclastJogador>.Empty).Limit(10)
                .SortByDescending(x => x.Personagem.Mochila.Moedas).ToListAsync();
            StringBuilder str = new StringBuilder();

            int pos = 1;
            foreach (var item in top)
            {
                var member = await ctx.Client.GetUserAsync(item.Id);
                str.AppendLine(Formatter.Bold($"{pos}. {member.Mention} {Emoji.Coins} {item.Personagem.Mochila.Moedas}"));
                pos++;
            }
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.WithDescription(str.ToString());

            await ctx.RespondAsync(embed: embed.Build());
        }
    }
}