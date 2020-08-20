using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
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
        private static List<Tabela> columnTable = new List<Tabela>();
        public static async Task<StringBuilder> ReadFileHead(IFormFile Ifilename)
        {
            var strinBuilder = new System.Text.StringBuilder();

            try
            {
                // using libera o fluxo do stream e lê apenas a primeira linha
                using (var reader = new StreamReader(Ifilename.OpenReadStream()))
                {
                    strinBuilder.AppendLine(await reader.ReadLineAsync());
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return strinBuilder;
        }

        public static string RemoveAccentsAndEspecialCHars(this string text)
        {
            text.Trim();
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

        public static ArquivoInfo geraFMT(StringBuilder strinBuilder, string delimiter, string lineBreak)
        {
            ArquivoInfo arquivoInfo = new CsvReader().lerArquivo(strinBuilder, delimiter);
            arquivoInfo.erro = false;
            arquivoInfo.Mensagem = "FMT Gerado com sucesso!";

            using (var streamWriter = new StreamWriter(arquivoInfo.file))
            {
                // Cria linha versao FMT
                streamWriter.WriteLine("10.0");

                // quantidade de campos
                streamWriter.WriteLine(arquivoInfo.totalLinhas + 1);

                //gera colunas e formato
                foreach (var c in arquivoInfo.cabecalho)
                {
                    string linha = geraLinhaFMT(arquivoInfo.cabecalho.IndexOf(c) + 1, delimiter, arquivoInfo.cabecalho.IndexOf(c) + 1, RemoveAccentsAndEspecialCHars(c).ToString());
                    streamWriter.WriteLine(linha);
                }

                // insere ultima linha
                var linhaFinal = geraLinhaFMT(arquivoInfo.totalLinhas + 1, lineBreak, arquivoInfo.cabecalho.Count + 1, "dummy");
                streamWriter.WriteLine(linhaFinal);
                streamWriter.WriteLine();
            }
            return arquivoInfo;
        }

        private static string geraLinhaFMT(int ordem, string delimitador, int ordemColuna, string nomeColuna)
        {
            //ordem       tipoCampo      inicio  tamanhocampo  delimitador       ordem colunas   nome campo    collation da coluna       
            return String.Format("{0}           SQLCHAR        0        255        \" {1} \"           {2}           {3}           Latin1_General_CI_AS",
                                        ordem,
                                        delimitador,
                                        ordemColuna,
                                        nomeColuna
                                      );
        }

        public static async Task<ArquivoInfo> StartGenerateFMT(IFormFile Ifilename, string delimiter, string lineBreak)
        {
            ArquivoInfo _arquivoInfo = new ArquivoInfo();

            try
            {
                var strinBuilder = await Core.ReadFileHead(Ifilename);

                _arquivoInfo = Core.geraFMT(strinBuilder, delimiter, lineBreak);

            }
            catch (Exception e)
            {
                _arquivoInfo.erro = true;
                _arquivoInfo.Mensagem = String.Format("Erro ao gerar FMT: {0}", e.Message);
            }
            return _arquivoInfo;
        }

        public static async Task<ArquivoInfo> StartGenerateSQL(SqlGenerator sqlGenerator)
        {
            ArquivoInfo _arquivoInfo = new ArquivoInfo();
            SqlParcialErros sqlParcialErros = new SqlParcialErros();
            _arquivoInfo.erro = false;
            _arquivoInfo.Mensagem = "SQL Gerado com sucesso!";
            columnTable.Clear();
            var i = 0;

            try
            {
                if (sqlGenerator.HasHeader)
                {
                    StringBuilder cabecalho = await Core.ReadFileHead(sqlGenerator.csvFile);
                    _arquivoInfo.cabecalho = cabecalho.ToString().Split(new[] { sqlGenerator.delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    foreach (var item in _arquivoInfo.cabecalho)
                    {
                        
                        Tabela tabela = new Tabela();
                        var nomeColuna = Core.RemoveAccentsAndEspecialCHars(item);
                        
                        if (columnTable.FindIndex(c => c.NomeCampo == nomeColuna) > 0)
                             nomeColuna = nomeColuna + "_" + i++ ;

                        tabela.NomeCampo = nomeColuna;
                        tabela.TamanhoCampo = 1;
                        columnTable.Add(tabela);
                    }
                }

                string res = ConvertCSVtoSqlInsert(sqlGenerator.csvFile, sqlGenerator.delimiter, sqlGenerator.HasHeader, ref sqlParcialErros);

                gerarSQL(res, sqlGenerator.GenerateCreateTable);

                if (sqlParcialErros.sqlParcialErros)
                {
                    _arquivoInfo.caminhoParcialErros = "ParcialErrors.txt";
                    _arquivoInfo.msgParcialErros = sqlParcialErros.msgParcialErros;

                    using(StreamWriter stw = new StreamWriter("ParcialErrors.txt"))
                    {
                        stw.WriteLine(sqlParcialErros.linhasErros);
                    }
                }

              
            }
            catch (Exception e)
            {
                _arquivoInfo.erro = true;
                _arquivoInfo.Mensagem = String.Format("Erro ao gerar SQL: {0}", e.Message);
            }
            return _arquivoInfo;
        }

        private static void gerarSQL(string insertSQL,  bool generateCreateTable)
        {
            string createTableSQL = "";
            if (generateCreateTable)
            {
                createTableSQL = "CREATE /*<NOME_TABLE>*/ (\n";
                columnTable.ForEach(column =>
                {
                   createTableSQL += $"\t{column.NomeCampo} VARCHAR({column.TamanhoCampo}),\n";
                });
                createTableSQL = createTableSQL.Remove(createTableSQL.Length - 2, 2);
                createTableSQL += "\n\t)";
            }


            using(StreamWriter streamWriter = new StreamWriter("SqlGerado.sql"))
            {
                streamWriter.WriteLine(createTableSQL);
                streamWriter.WriteLine();
                streamWriter.WriteLine(insertSQL);
            };      

        }

        public static string ConvertCSVtoSqlInsert(IFormFile csvFile, string delimiter, bool hasHeader, ref SqlParcialErros sqlParcialErros)
        {
            
            int FILE_LINE = 0;
            string ALL_LINES = "";
            

            using (StreamReader reader = new StreamReader(csvFile.OpenReadStream()))
            {                

                if (hasHeader)
                {
                    string[] headers = reader.ReadLine().Split(delimiter);
                }               

                
                while (!reader.EndOfStream)
                {
                    FILE_LINE++;
                    string LINHAS_INSERT = "INSERT INTO /*<TABLE_NAME>*/ VALUES (";
                    try
                    {
                        var linha = reader.ReadLine();
                        string[] rows = linha.Split(delimiter);
                    
                        if(rows.Length < columnTable.Count)
                        {
                            sqlParcialErros.sqlParcialErros = true;
                            sqlParcialErros.msgParcialErros = "Algumas linhas do arquivo geraram erro. Foi gerado um arquivo com os erros no mesmo diretório do sql";
                            sqlParcialErros.linhasErros += $"LINHA: {FILE_LINE} - REGISTRO: {linha} \n";
                            continue;
                        }

                        for (int i = 0; i < columnTable.Count; i++)
                        {

                            if (hasHeader)
                                if (columnTable[i].TamanhoCampo < rows[i].Length)
                                    columnTable[i].TamanhoCampo = rows[i].Length;

                            LINHAS_INSERT += $"'{rows[i]}',";
                        }
                        // remove ultimo caractere de uma string e fecha parentese
                        LINHAS_INSERT = LINHAS_INSERT.Remove(LINHAS_INSERT.Length - 1, 1);
                        LINHAS_INSERT += " );";
                        ALL_LINES += LINHAS_INSERT + " \n";

                    }
                    catch(Exception e)
                    {
                        if (e.Message == "Index was outside the bounds of the array.")
                        {
                            continue;
                        }
                        else
                        {
                            throw new Exception(e.Message);
                        }
                            
                    }
                    
                }
            };
            return ALL_LINES;
        }           

    }
}
