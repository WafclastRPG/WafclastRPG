﻿using TorreRPG.Entidades;
using TorreRPG.Entidades.Itens;
using TorreRPG.Extensoes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TorreRPG.Services;
using TorreRPG.BancoItens;

namespace TorreRPG.Comandos.Acao
{
    public class ComandoSubir : BaseCommandModule
    {
        public readonly Banco banco;

        [Command("subir")]
        [Description("Permite subir um andar da torre. Encontra novos inimigos!")]
        public async Task ComandoSubirAsync(CommandContext ctx)
        {
            // Verifica se existe o jogador,
            var (naoCriouPersonagem, personagemNaoModificar) = await banco.VerificarJogador(ctx);
            if (naoCriouPersonagem) return;

            if (personagemNaoModificar.Zona.Monstros.Count != 0)
            {
                await ctx.RespondAsync($"{ctx.User.Mention}, você precisa eliminar todos os montros para subir!");
                return;
            }

            int inimigos = 0;
            using (var session = await banco.Cliente.StartSessionAsync())
            {
                BancoSession banco = new BancoSession(session);
                RPJogador jogador = await banco.GetJogadorAsync(ctx);
                RPPersonagem personagem = jogador.Personagem;

                bool temMonstros = RPMetadata.MonstrosNomes.ContainsKey(personagem.Zona.Nivel - 1);
                if (temMonstros)
                {

                    inimigos = personagem.Zona.TrocarZona(personagem.VelocidadeAtaque.Modificado, personagem.Zona.Nivel - 1);

                    await banco.EditJogadorAsync(jogador);
                    await session.CommitTransactionAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention}, apareceu {inimigos} monstro na sua frente!");
                }
                else if (personagem.Zona.Nivel - 1 == 0)
                {
                    foreach (var item in personagem.Frascos)
                        item.AddCarga(double.MaxValue);
                    personagem.Vida.Adicionar(double.MaxValue);
                    personagem.Mana.Adicionar(double.MaxValue);
                    personagem.Efeitos = new List<RPEfeito>();
                    personagem.Zona.Nivel = 0;
                    personagem.Zona.ItensNoChao = new List<RPBaseItem>();
                    await banco.EditJogadorAsync(jogador);
                    await session.CommitTransactionAsync();
                    await ctx.RespondAsync($"{ctx.User.Mention}, você saiu da torre!");
                }
                else
                    await ctx.RespondAsync($"{ctx.User.Mention}, você só pode subir para o céu morrendo!");
            }
        }
    }
}
