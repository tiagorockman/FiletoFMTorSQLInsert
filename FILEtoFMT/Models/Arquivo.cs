using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FILEtoFMT.Models
{
    public class Arquivo
    {
        [Required(ErrorMessage ="Favor selecionar o Arquivo")]

        public string filename { get; set; }
        [Required(ErrorMessage ="Favor definir o delimitador")]

        public string delimiter { get; set; }
    }
}
