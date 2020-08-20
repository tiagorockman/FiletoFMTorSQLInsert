using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FILEtoFMT.Models
{
    public class Tabela
    {
        public string NomeCampo { get; set; }
        public int TamanhoCampo { get; set; }
    }

    public class SqlGenerator
    {

        [DisplayNameAttribute("Gerar Create Table")]
        public bool GenerateCreateTable { get; set; }

        [DisplayNameAttribute("Possui Cabeçalho")]
        public bool HasHeader { get; set; }

        [DisplayNameAttribute("Selecionar Arquivo")]
        [Required(ErrorMessage = "Favor definir o indicador de fim de linha")]
        public IFormFile csvFile { get; set; }

        [DisplayNameAttribute("Delimitador")]
        [Required(ErrorMessage = "Favor definir o delimitador")]
        public string delimiter { get; set; }
    }

    public  class SqlParcialErros
    {
        public bool sqlParcialErros { get; set; }
        public string msgParcialErros { get; set; }
        public string caminho { get; set; }
        public string linhasErros { get; set; }
    }
}
