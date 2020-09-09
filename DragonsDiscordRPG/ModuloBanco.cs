﻿using DragonsDiscordRPG.Entidades;
using DragonsDiscordRPG.Extensoes;
using DragonsDiscordRPG.Itens.Pocoes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DragonsDiscordRPG
{
    public static class ModuloBanco
    {
        public static IMongoClient Cliente { get; private set; }
        public static IMongoDatabase Database { get; private set; }
        public static IMongoCollection<RPJogador> ColecaoJogador { get; private set; }
        public static IMongoCollection<Wiki> ColecaoWiki { get; private set; }

        public static Dictionary<int, MonstroNomes> MonstrosNomes { get; set; }

        public static void Conectar()
        {
            Cliente = new MongoClient();
            Database = Cliente.GetDatabase("Dragon");

            ColecaoJogador = Database.CriarCollection<RPJogador>();
            ColecaoWiki = Database.CriarCollection<Wiki>();

            RPPocoes.Carregar();

            //BsonSerializer.RegisterSerializer(typeof(float),
            //    new SingleSerializer(BsonType.Double, new RepresentationConverter(
            //    true, //allow truncation
            //    true // allow overflow, return decimal.MinValue or decimal.MaxValue instead
            //)));


            //var notificationLogBuilder = Builders<RPGJogador>.IndexKeys;
            //var indexModel = new CreateIndexModel<RPGJogador>(notificationLogBuilder.Ascending(x => x.NivelAtual));
            //ColecaoJogador.Indexes.CreateOne(indexModel);

            MonstrosNomes = MonstroNomes.GetMonstros();
        }

        public static Task<RPJogador> GetJogadorAsync(DiscordUser user)
              => ColecaoJogador.Find(x => x.Id == user.Id).FirstOrDefaultAsync();

        public static Task<RPJogador> GetJogadorAsync(CommandContext ctx)
           => GetJogadorAsync(ctx.User);
    }
}
