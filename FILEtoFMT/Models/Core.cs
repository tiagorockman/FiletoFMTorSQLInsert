using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FILEtoFMT.Models
{
    public static class Core
    {
        public static async Task<string> ReadFile(IFormFile filename, string delimiter)
        {
            var strinBuilder = new System.Text.StringBuilder();

            try
            {
                // using libera o fluxo do stream e lê apenas a primeira linha
                using (var reader = new StreamReader(filename.OpenReadStream()))
                {
                    strinBuilder.AppendLine(await reader.ReadLineAsync());
                }
                geraFMT(strinBuilder, delimiter);
            }
            catch (Exception e)
            {
                throw e;
            }

            return "ok";
        }

        public static string RemoveAccentsAndEspecialCHars(this string text)
        {

            string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç\n\s]";
            Regex rgx = new Regex(pattern);

            text = rgx.Replace(text, "");
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString().Replace(" ", "_").ToUpper();
        }

        private static void geraFMT(StringBuilder strinBuilder, string delimiter)
        {
            List<string> cabecalho = strinBuilder.ToString().Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string file = "file.fmt";
            string caminho = Path.GetFullPath(file); ;
            using (var streamWriter = new StreamWriter(file))
            {
                // Cria linha versao FMT
                streamWriter.WriteLine("10.0");

                // quantidade de campos
                streamWriter.WriteLine(cabecalho.Count.ToString());

                //gera colunas e formato
                foreach (var c in cabecalho) {
                                               //ordem       tipoCampo      inicio  tamanhocampo  delimitador       ordem colunas   nome campo    collation da coluna       
                    var linha = String.Format("{0}           SQLCHAR        0        255        \" {1} \"           {2}           {3}       Latin1_General_CI_AS", 
                                                cabecalho.IndexOf(c)+1, 
                                                delimiter,
                                                cabecalho.IndexOf(c)+1,
                                                RemoveAccentsAndEspecialCHars(c).ToString()
                                              );
                    streamWriter.WriteLine(linha);
                }

            }

            
        }
    }
}
