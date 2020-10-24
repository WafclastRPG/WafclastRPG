﻿using TorreRPG.Entidades;
using TorreRPG.Extensoes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;
using TorreRPG.Atributos;
using System.Text;
using TorreRPG.Services;
using System;

namespace TorreRPG.Comandos.Exibir
{
    public class ComandoChao : BaseCommandModule
    {
        public Banco banco;

        [Command("chao")]
        [Aliases("drop")]
        [Description("Permite examinar um item.\n`#ID` se contra no chão.")]
        [ComoUsar("chao [#ID]")]
        [Exemplo("chao #1")]
        public async Task ComandoChaoAsync(CommandContext ctx, string idEscolhido = "")
        {
            //Verifica se existe o jogador,
            var (naoCriouPersonagem, personagemNaoModificar) = await banco.VerificarJogador(ctx);
            if (naoCriouPersonagem) return;

            if (personagemNaoModificar.IsPortalAberto)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, você não pode usar este comando com o portal aberto!");
                return;
            }

            if (personagemNaoModificar.Zona.ItensNoChao.Count == 0)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, você precisa de itens no chão para olhar! Elimine alguns monstros!");
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.WithColor(DiscordColor.Yellow);
            if (string.IsNullOrWhiteSpace(idEscolhido))
            {
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < personagemNaoModificar.Zona.ItensNoChao.Count; i++)
                {
                    var item = personagemNaoModificar.Zona.ItensNoChao[i];
                    str.Append($"`#{i}` ");
                    str.Append(ComandoMochila.GerarEmojiRaridade(item.Raridade));

                    str.AppendLine($" {item.TipoBaseModificado.Titulo().Bold()} ");
                }
                embed.WithDescription("Você está olhando para os itens no chão! Digite `!pegar` para guarda-los na mochila!\n" + str.ToString());
                await ctx.RespondAsync(embed: embed.Build());
                return;
            }

            // Converte o id informado.
            if (idEscolhido.TryParseID(out int id))
            {
                var descricao = ComandoExaminar.ItemDescricao(personagemNaoModificar, id);
                if (descricao != null)
                {
                    descricao.WithAuthor($"{ctx.User.Username} - {personagemNaoModificar.Nome}", iconUrl: ctx.User.AvatarUrl);
                    await ctx.RespondAsync(embed: descricao.Build());
                }
                else
                    await ctx.RespondAsync($"{ctx.User.Mention}, `#ID` não encontrado!");
            }
            else
                await ctx.RespondAsync($"{ctx.User.Mention}, o `#ID` precisa ser numérico. Digite `!chao` para encontrar `#ID`s.");
        }
    }
}