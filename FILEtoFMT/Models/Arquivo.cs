using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace FILEtoFMT.Models
{
    public class Arquivo
    {       

        [Required(ErrorMessage ="Favor selecionar o Arquivo")]

        public IFormFile filename { get; set; }
        
        [Required(ErrorMessage ="Favor definir o delimitador")]
        public string delimiter { get; set; }

        [Required(ErrorMessage = "Favor definir o indicador de fim de linha")]
        public string lineBreak { get; set; }

    }

    public  class ArquivoInfo 
    {
        public  List<string> cabecalho { get; set; }
        public  string file  { get; set; }
        public  string caminho { get; set; }
        public  int totalLinhas { get; set; }
        public string Mensagem { get; set; }
        public bool erro { get; set; }
        public string msgParcialErros { get; set; }
        public string caminhoParcialErros { get; set; }

    }

    public  interface ICSVReader
    {
        ArquivoInfo lerArquivo(StringBuilder stringBuilder, string delimiter);
    }

    public class CsvReader : ICSVReader
    {
        [TempData]
        public string Caminho { get; set; }

        public ArquivoInfo lerArquivo(StringBuilder stringBuilder, string delimiter)
        {
            ArquivoInfo arquivoInfo = new ArquivoInfo();
            arquivoInfo.cabecalho = stringBuilder.ToString().Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            arquivoInfo.file = "file.fmt";
            arquivoInfo.caminho = Path.GetFullPath(arquivoInfo.file); ;
            arquivoInfo.totalLinhas = arquivoInfo.cabecalho.Count;
            Caminho = arquivoInfo.caminho;
            return arquivoInfo;
        }
    }
 
}