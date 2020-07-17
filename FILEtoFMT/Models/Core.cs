using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace FILEtoFMT.Models
{
    public static class Core
    {
        public static async Task ReadFile(IFormFile Ifilename, string delimiter, string lineBreak)
        {
            var strinBuilder = new System.Text.StringBuilder();

            try
            {
                // using libera o fluxo do stream e lê apenas a primeira linha
                using (var reader = new StreamReader(Ifilename.OpenReadStream()))
                {
                    strinBuilder.AppendLine(await reader.ReadLineAsync());
                }
                geraFMT(strinBuilder, delimiter, lineBreak);
            }
            catch (Exception e)
            {
                throw e;
            }


        }

        public static string RemoveAccentsAndEspecialCHars(this string text)
        {

            string pattern = @"(?i)[^0-9a-záéíóúàèìòùâêîôûãõç_\s]";
            Regex rgx = new Regex(pattern);

            text = rgx.Replace(text, "");
            text = text.Replace("\r", "").Replace("\n", "");
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString().Replace(" ", "_").ToUpper();
        }

        private static void geraFMT(StringBuilder strinBuilder, string delimiter, string lineBreak)
        {
            ArquivoInfo arquivoInfo = new CsvReader().lerArquivo(strinBuilder, delimiter);
 
            using (var streamWriter = new StreamWriter(arquivoInfo.file))
            {
                // Cria linha versao FMT
                streamWriter.WriteLine("10.0");

                // quantidade de campos
                streamWriter.WriteLine(arquivoInfo.totalLinhas + 1);

                //gera colunas e formato
                foreach (var c in arquivoInfo.cabecalho) {
                    string linha = geraLinhaFMT(arquivoInfo.cabecalho.IndexOf(c) + 1, delimiter, arquivoInfo.cabecalho.IndexOf(c) + 1, RemoveAccentsAndEspecialCHars(c).ToString());
                    streamWriter.WriteLine(linha);
                }

                // insere ultima linha
                var linhaFinal = geraLinhaFMT(arquivoInfo.totalLinhas + 1, lineBreak, arquivoInfo.cabecalho.Count + 1, "dummy");
                streamWriter.WriteLine(linhaFinal);
                streamWriter.WriteLine();
            }

            
        }

        private static string geraLinhaFMT(int ordem, string delimitador, int ordemColuna, string nomeColuna )
        {
                                      //ordem       tipoCampo      inicio  tamanhocampo  delimitador       ordem colunas   nome campo    collation da coluna       
            return String.Format("{0}           SQLCHAR        0        255        \" {1} \"           {2}           {3}           Latin1_General_CI_AS",
                                        ordem,
                                        delimitador,
                                        ordemColuna,
                                        nomeColuna
                                      );
        }
    }
}
