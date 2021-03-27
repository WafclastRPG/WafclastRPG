﻿using System.Globalization;
using System.Threading.Tasks;
using WafclastRPG.Entities;

namespace WafclastRPG.Entities
{
    public class Player : WafclastPlayer
    {
        private readonly DatabaseSession banco;

        public Player(WafclastPlayer jogador, DatabaseSession banco) : base(jogador)
        {
            this.banco = banco;
        }

        public Task SaveAsync() => this.banco.ReplacePlayerAsync(new WafclastPlayer(this));
        public string Mention()
            => $"<@{Id.ToString(CultureInfo.InvariantCulture)}>";
    }
}