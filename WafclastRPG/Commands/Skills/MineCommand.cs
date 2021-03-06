﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Text;
using System.Threading.Tasks;
using WafclastRPG.Attributes;
using WafclastRPG.DataBases;
using WafclastRPG.Extensions;
using WafclastRPG.Properties;

namespace WafclastRPG.Commands.Skills
{
    public class MineCommand : BaseCommandModule
    {
        public DataBase database;

        [Command("minerar")]
        [Aliases("mine")]
        [Description("Permite minerar por recursos preciosos.")]
        [Usage("minerar")]
        [Cooldown(1, 600, CooldownBucketType.User)]
        public async Task UseCommandAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            Response response;
            using (var session = await database.StartDatabaseSessionAsync())
                response = await session.WithTransactionAsync(async (s, ct) =>
                {
                    var player = await session.FindPlayerAsync(ctx.User);
                    if (player == null)
                        return new Response(Messages.NaoEscreveuComecar);

                    var rd = new Random();
                    var str = new StringBuilder();

                    double pickaxePower = 0;
                    double dropBonus = 1;

                    if (player.Character.Pickaxe != null)
                    {
                        pickaxePower = player.Character.Pickaxe.Hardness;
                        dropBonus = player.Character.Pickaxe.DropChanceBonus;
                    }


                    double incrementPorcent = (player.Character.MineSkill.Level / 100) + 1;

                    foreach (var drop in DataBase.MineDrop)
                    {
                        if (pickaxePower >= drop.Hardness)
                            if (rd.Chance((drop.DropChance * incrementPorcent) * dropBonus))
                            {
                                var quantityMax = (ulong)Math.Truncate((player.Character.MineSkill.Level / drop.MinLevel) + 1);
                                var quantity = Convert.ToUInt64(rd.Sortear(1, quantityMax));
                                var exp = (drop.ExperienceGain * quantity);

                                str.AppendLine($"**+{quantity} {drop.Name} e +{exp}{Emojis.Exp}**");

                                player.Character.MineSkill.AddExperience(exp);
                                await player.AddItemAsync(drop, quantity);
                            }
                    }

                    player.Character.MineSkill.AddExperience(5);

                    await session.ReplaceAsync(player);

                    if (str.Length == 0)
                        return new Response("você não conseguiu nada desta vez.", player.Reminder);

                    var embed = new DiscordEmbedBuilder();
                    embed.WithColor(DiscordColor.Green);
                    embed.WithDescription(str.ToString());
                    return new Response(embed, player.Reminder);
                });

            if (!string.IsNullOrWhiteSpace(response.Message))
                await ctx.ResponderAsync(response.Message);

            if (response.Embed != null)
                await ctx.ResponderAsync(response.Embed?.Build());

            if (response.Reminder)
            {
                await Task.Delay(600000);
                await ctx.ResponderAsync($"{Messages.Reminder} `minerar`");
            }
        }
    }
}
