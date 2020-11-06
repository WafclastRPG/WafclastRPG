﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Collections.Concurrent;
using System.Threading;
using WafclastRPG.Game;

namespace WafclastRPG.Bot.Comandos
{
    public class ComandoAdministrativo : BaseCommandModule
    {
        public Banco banco;

        [Command("purge")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task PurgeAsync(CommandContext ctx, int quantidade)
            => await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(quantidade + 1));

        [Command("mensagem")]
        [RequireOwner]
        public async Task MensagemAsync(CommandContext ctx, [RemainingText] string mensagem)
        {
            var embed = new DiscordEmbedBuilder();
            embed.WithDescription(mensagem);
            embed.WithImageUrl("https://cdn.discordapp.com/attachments/758139402219159562/773918198524936252/Wafclast.png");
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("sudo")]
        [RequireOwner]
        public async Task Sudo(CommandContext ctx, DiscordUser member, [RemainingText] string command)
        {
            await ctx.TriggerTypingAsync();
            var invocation = command.Substring(1);
            var cmd = ctx.CommandsNext.FindCommand(invocation, out var args);
            if (cmd == null)
            {
                await ctx.RespondAsync("Comando não encontrado");
                return;
            }
            var cfx = ctx.CommandsNext.CreateFakeContext(member, ctx.Channel, "", "!", cmd, args);
            await ctx.CommandsNext.ExecuteCommandAsync(cfx);
        }

        [Command("deletar-user")]
        [RequireOwner]
        public async Task DeletarUsuarioAsync(CommandContext ctx, [RemainingText] DiscordUser user = null)
        {
            await ctx.TriggerTypingAsync();
            if (user == null) user = ctx.User;
            await banco.Jogadores.DeleteOneAsync(x => x.Id == user.Id);
            await ctx.RespondAsync($"{user} deletado do banco de dados!");
        }

        public static ConcurrentDictionary<ulong, SemaphoreSlim> Buckets = new ConcurrentDictionary<ulong, SemaphoreSlim>();

        [Command("testetravar")]
        [RequireOwner]
        public async Task ff(CommandContext ctx)
        {
            if (!Buckets.TryGetValue(ctx.User.Id, out var bucket))
            {
                bucket = new SemaphoreSlim(1, 1);
                Buckets.AddOrUpdate(ctx.User.Id, bucket, (k, v) => bucket);
            }

            await bucket.WaitAsync();
            await ctx.RespondAsync($"{ctx.User.Username} aguardando 5 segundos");
            await Task.Delay(TimeSpan.FromSeconds(5));
            bucket.Release();
        }


        [Command("testedes")]
        [RequireOwner]
        public async Task fff(CommandContext ctx)
        {
            if (!Buckets.TryGetValue(ctx.User.Id, out var bucket))
            {
                bucket = new SemaphoreSlim(1, 1);
                Buckets.AddOrUpdate(ctx.User.Id, bucket, (k, v) => bucket);
            }


            bucket.Release();
        }

        [Command("atualizar")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task Atualizar(CommandContext ctx)
        {
            //FilterDefinition<RPJogador> filter = FilterDefinition<RPJogador>.Empty;
            //FindOptions<RPJogador> options = new FindOptions<RPJogador>
            //{
            //    BatchSize = 8,
            //    NoCursorTimeout = false
            //};

            //using (IAsyncCursor<RPJogador> cursor = await banco.Jogadores.FindAsync(filter, options))
            //{
            //    while (await cursor.MoveNextAsync())
            //    {
            //        IEnumerable<RPJogador> usuarios = cursor.Current;

            //        foreach (RPJogador user in usuarios)
            //        {
            //            var f = user.Personagem.Mochila.Itens.FindAll(x => x.Classe == Enuns.RPClasse.PergaminhoSabedoria);
            //            foreach (var item in f)
            //            {
            //                if (item.Classe == Enuns.RPClasse.PergaminhoSabedoria)
            //                    item.TipoBaseModificado = "Pergaminho de sabedoria";
            //            }
            //            await banco.Jogadores.ReplaceOneAsync(x => x.Id == user.Id, user);
            //        }
            //    }
            //}
            //await ctx.RespondAsync("Banco foi atualizado com sucesso!");
        }

        private static ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
                if (codec.FormatID == format.Guid)
                    return codec;
            return null;
        }

        [Command("teste")]
        [RequireOwner]
        public async Task testeCalc(CommandContext ctx, [RemainingText] string text)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                Encoder myEncoder = Encoder.Quality;
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 70L);
                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                myEncoderParameters.Param[0] = myEncoderParameter;

                PointF firstLocation = new PointF(90f, 289f);

                string imageFilePath = @"C:\Users\Talion\Desktop\template.png";
                Bitmap bitmap = (Bitmap)Image.FromFile(imageFilePath);//load the image file

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    using (Font arialFont = new Font("Arial", 89, FontStyle.Italic | FontStyle.Bold))
                    {
                        graphics.DrawString(text, arialFont, Brushes.White, firstLocation);
                    }
                }
                bitmap.Save(@"C:\Users\Talion\Desktop\teste.png");

                new Bitmap(bitmap).Save(memoryStream, jpgEncoder, myEncoderParameters); //save the image file
                memoryStream.Position = 0;
                await ctx.RespondWithFileAsync("Perfil.jpeg", memoryStream);
            }

            //var numeroRandom = Calculo.SortearValor(0, 100.0);
            //switch (numeroRandom)
            //{
            //    case double n when (n > 0.9):
            //        await ctx.RespondAsync($"N maior que 0.9 {numeroRandom}");
            //        break;
            //    case double n when (n > 0.8):
            //        await ctx.RespondAsync($"N maior que 0.8 {numeroRandom}");
            //        break;
            //    case double n when (n > 0.7):
            //        await ctx.RespondAsync($"N maior que 0.7 {numeroRandom}");
            //        break;
            //    case double n when (n > 0.6):
            //        await ctx.RespondAsync($"N maior que 0.6 {numeroRandom}");
            //        break;
            //    case double n when (n > 0.5):
            //        await ctx.RespondAsync($"N maior que 0.5 {numeroRandom}");
            //        break;
            //    case double n when (n > 0.4):
            //        await ctx.RespondAsync($"N maior que 0.4 {numeroRandom}");
            //        break;
            //    case double n when (n > 0.3):
            //        await ctx.RespondAsync($"N maior que 0.3 {numeroRandom}");
            //        break;
            //    case double n when (n > 0.2):
            //        await ctx.RespondAsync($"N maior que 0.2 {numeroRandom}");
            //        break;
            //}

            //var frutas = new List<Fruta> {
            //new Fruta { Cor = 1, Nome = "Morango" },
            //    new Fruta { Cor = 1, Nome = "Framboesa" },
            //    new Fruta { Cor = 1, Nome = "Cereja" },
            //    new Fruta { Cor = 2, Nome = "Limão" },
            //    new Fruta { Cor = 2, Nome = "Melancia" }        };

            //var grupos = frutas.GroupBy(f => f.Cor);

            //Console.WriteLine("Imprimindo grupos");
            //Console.WriteLine("-----------------------");
            //foreach (var nomeGrupo in grupos.Select(g => g.Key))
            //{
            //    Console.WriteLine(nomeGrupo);
            //}

            //Console.WriteLine("-----------------------");
            //var pesquisa = grupos.Where(x => x.Key <= 2).ToList();
            //foreach (var listaDeFrutasPorCor in pesquisa)
            //{
            //    Console.WriteLine("Imprimindo frutas da cor: " + listaDeFrutasPorCor.Key);
            //    Console.WriteLine("-----------------------");
            //    foreach (var fruta in listaDeFrutasPorCor)
            //    {
            //        Console.WriteLine(fruta.Nome);
            //    }
            //    Console.WriteLine("-----------------------");
            //}
        }
    }
}