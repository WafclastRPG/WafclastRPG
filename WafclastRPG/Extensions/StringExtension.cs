﻿using DSharpPlus;
using System;
using System.Globalization;
using System.Text;

namespace WafclastRPG.Extensions
{
    public static class StringExtension
    {
        public static string RemoverAcentos(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
     
        public static string Titulo(this string titulo)
            => "⌈" + titulo + "⌋";

        public static string Url(this string texto, string site)
            => Formatter.MaskedUrl($"`{texto}` ", new Uri(site));
    }
}