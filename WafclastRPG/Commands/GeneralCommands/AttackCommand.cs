﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WafclastRPG.Attributes;
using WafclastRPG.DataBases;
using WafclastRPG.Entities.Monsters;
using WafclastRPG.Enums;
using WafclastRPG.Extensions;
using WafclastRPG.Properties;

namespace WafclastRPG.Commands.GeneralCommands
{
    class AttackCommand : BaseCommandModule
    {
        public DataBase database;

        [Command("atacar")]
        [Aliases("at")]
        [Description("Permite executar um ataque físico em um monstro ou jogador.")]
        [Usage("atacar [ ID do Monstro | Jogador ]")]
        [Example("atacar 1", "Faz você atacar o monstro de ID 1.")]
        public async Task AttackCommandAsync(CommandContext ctx, int monsterId = 1)
        {
            var timer = new Stopwatch();
            timer.Start();

            await ctx.TriggerTypingAsync();

            Response response;
            using (var session = await database.StartDatabaseSessionAsync())
                response = await session.WithTransactionAsync(async (s, ct) =>
                {
                    var player = await session.FindAsync(ctx.User);

                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(player.Language);

                    if (player.Character == null)
                        return new Response(Messages.NaoEscreveuComecar);
                    #region Questions
                    var map = await session.FindAsync(ctx.Channel);
                    if (map == null)
                        return new Response(Messages.CanalNaoEsMapa);

                    if (player.Character.Localization != map)
                        return new Response(Messages.ComandoEmLocalizacaoDiferente);

                    if (map.Tipo == MapType.Cidade)
                        return new Response(Messages.NaoPodeAtacarNaCidade);

                    var target = await session.FindAsync(new WafclastMonsterBase(ctx.Channel.Id, monsterId));
                    if (target == null)
                        return new Response("você procurou, mas não encontrou nenhum monstro com este ID!");

                    if (!target.ItsPillaged)
                        return new Response("o monstro não foi saqueado ainda!");

                    if (target.DateSpawn > DateTime.UtcNow)
                        return new Response($"este monstro já está morto! Tente atacar outro!");
                    #endregion
                    //Combat
                    var random = new Random();
                    var str = new StringBuilder();
                    var embed = new DiscordEmbedBuilder();
                    embed.WithColor(DiscordColor.Red);
                    embed.WithAuthor($"{ctx.User.Username} [Nv.{player.Character.Level}]", iconUrl: ctx.User.AvatarUrl);
                    embed.WithTitle($"Relatorio do combate contra {target.Name}.");

                    if (!random.Chance(player.Character.HitChance(target.Evasion.MaxValue)))
                    {
                        var playerDamage = random.Sortear(player.Character.PhysicalDamage.CurrentValue);
                        playerDamage = target.DamageReduction(playerDamage);

                        var isTargetDead = target.ReceberDano(playerDamage);
                        str.AppendLine($"{target.Name} não conseguiu desviar!");
                        str.AppendLine($"{target.Name} recebeu {playerDamage:N2} de dano.");

                        if (isTargetDead)
                        {
                            player.MonsterKill++;

                            if (player.Character.Karma < 0)
                                player.Character.Karma += 1;

                            str.AppendLine($"{Emojis.CrossBone} {target.Name.Titulo()} {Emojis.CrossBone}");

                            await session.ReplaceAsync(player);
                            await session.ReplaceAsync(target);
                            embed.WithDescription(str.ToString());
                            return new Response(embed);
                        }
                    }
                    else
                        str.AppendLine($"{target.Name} conseguiu desviar!");

                    embed.AddField(target.Name, $"{Emojis.GerarVidaEmoji(target.Life.CurrentValue / target.Life.MaxValue)} {target.Life.CurrentValue:N2} ", true);

                    var isPlayerDead = false;

                    if (random.Chance(player.Character.DodgeChance(target.Accuracy.MaxValue)))
                        str.AppendLine($"{player.Mention} desviou do ataque!");
                    else
                    {
                        var targetDamage = random.Sortear(target.PhysicalDamage.MaxValue);
                        targetDamage = player.Character.DamageReduction(targetDamage);

                        str.AppendLine($"{player.Mention} não conseguiu desviar!");
                        str.AppendLine($"{player.Mention} recebeu {targetDamage:N2} de dano.");

                        isPlayerDead = player.Character.ReceiveDamage(targetDamage);

                        if (isPlayerDead)
                        {
                            str.AppendLine($"{player.Mention} morreu!");
                            player.Deaths++;
                        }
                    }

                    if (isPlayerDead)
                        embed.AddField(ctx.User.Username, $"{Emojis.GerarVidaEmoji(0 / player.Character.Life.MaxValue)} 0 ", true);
                    else
                        embed.AddField(ctx.User.Username, $"{Emojis.GerarVidaEmoji(player.Character.Life.CurrentValue / player.Character.Life.MaxValue)} {player.Character.Life.CurrentValue:N2} ", true);

                    await session.ReplaceAsync(player);
                    await session.ReplaceAsync(target);
                    return new Response(embed.WithDescription(str.ToString()));
                });

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                await ctx.ResponderAsync(response.Message);
                return;
            }

            timer.Stop();
            response.Embed.WithFooter($"{timer.Elapsed.Seconds}.{timer.ElapsedMilliseconds + ctx.Client.Ping}s.");
            await ctx.RespondAsync(ctx.User.Mention, response.Embed.Build());
        }

        [Command("atacar")]
        [Example("atacar @Talion", "Faz você atacar o jogador mencionado.")]
        [Priority(1)]
        public async Task AttackCommandAsync(CommandContext ctx, DiscordUser targetUser)
        {
            var timer = new Stopwatch();
            timer.Start();

            await ctx.TriggerTypingAsync();

            if (targetUser == ctx.User) return;

            Response response;
            using (var session = await database.StartDatabaseSessionAsync())
                response = await session.WithTransactionAsync(async (s, ct) =>
                {
                    var player = await session.FindAsync(ctx.User);

                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(player.Language);

                    if (player.Character == null)
                        return new Response(Messages.NaoEscreveuComecar);
                    #region Questions
                    var map = await session.FindAsync(ctx.Channel);
                    if (map == null)
                        return new Response(Messages.CanalNaoEsMapa);

                    if (map.Id != player.Character.Localization.ChannelId)
                        return new Response(Messages.ComandoEmLocalizacaoDiferente);

                    if (map.Tipo == MapType.Cidade)
                        return new Response(Messages.NaoPodeAtacarNaCidade);

                    var target = await session.FindAsync(targetUser);
                    if (target.Character == null)
                        return new Response(Messages.JogadorAlvoNaoCriouPersonagem);

                    if (target.Character.Localization != player.Character.Localization)
                        return new Response(Messages.JogadorAlvoEstaEmOutraLocalizacao);
                    #endregion
                    //Combat
                    var random = new Random();
                    var str = new StringBuilder();
                    var embed = new DiscordEmbedBuilder();
                    embed.WithColor(DiscordColor.Red);
                    embed.WithAuthor($"{ctx.User.Username} [Nv.{player.Character.Level}]", iconUrl: ctx.User.AvatarUrl);
                    embed.WithTitle($"Relatorio do combate contra {targetUser.Username}.");

                    if (target.Character.Karma == 0)
                        player.Character.Karma -= 1;

                    if (random.Chance(player.Character.HitChance(target.Character.Evasion.CurrentValue)))
                        str.AppendLine($"{target.Mention} conseguiu desviar.");
                    else
                    {
                        var playerDamage = random.Sortear(player.Character.PhysicalDamage.CurrentValue);
                        playerDamage = target.Character.DamageReduction(playerDamage);
                        var isTargetDead = target.Character.ReceiveDamage(playerDamage);

                        str.AppendLine($"{target.Mention} não conseguiu desviar.");
                        str.AppendLine($"{target.Mention} recebeu {playerDamage:N2}({Emojis.Adaga}) de dano.");

                        if (isTargetDead)
                        {
                            if (target.Character.Karma == 0)
                                player.Character.Karma -= 10;
                            str.AppendLine($"{Emojis.CrossBone} {target.Mention} {Emojis.CrossBone}");
                            player.PlayerKill++;
                            target.Deaths++;
                        }
                    }

                    await session.ReplaceAsync(player);
                    await session.ReplaceAsync(target);
                    embed.WithDescription(str.ToString());
                    return new Response(embed);
                });

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                await ctx.ResponderAsync(response.Message);
                return;
            }

            timer.Stop();
            response.Embed.WithFooter($"{timer.Elapsed.Seconds}.{timer.ElapsedMilliseconds + ctx.Client.Ping}s.");
            await ctx.RespondAsync($"{ctx.User.Mention} {targetUser.Mention}", response.Embed.Build());
        }
    }
}