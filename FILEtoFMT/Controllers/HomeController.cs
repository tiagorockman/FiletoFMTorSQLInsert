using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FILEtoFMT.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using Microsoft.Extensions.Options;

namespace FILEtoFMT.Controllers
{

    public class HomeController : Controller
    {        
        private readonly ILogger<HomeController> _logger;

        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> GenerateFMTAsync(Arquivo arquivo)
        {
           var _arquivoInfo = await Core.StartGenerateFMT(arquivo.filename, arquivo.delimiter, arquivo.lineBreak);
            if (!_arquivoInfo.erro)
            {
                TempData["Confirmacao"] = _arquivoInfo.Mensagem;
                TempData["Caminho"] = _arquivoInfo.caminho;
            } else
            {
                TempData["Erro"] = _arquivoInfo.Mensagem;
            }             

            return View("Index");
        }

        public IActionResult SQLIndex()
        {
            return View();
        }

        public async Task<IActionResult> SqlGenerate(SqlGenerator sqlGenerator)
        {
            var _arquivoInfo = await Core.StartGenerateSQL(sqlGenerator);
            if (!_arquivoInfo.erro)
            {
                TempData["Confirmacao"] = _arquivoInfo.Mensagem;
                TempData["Caminho"] = _arquivoInfo.caminho;
            }
            else
            {
                TempData["Erro"] = _arquivoInfo.Mensagem;
            }

            if (!String.IsNullOrEmpty(_arquivoInfo.msgParcialErros))
            {
                TempData["ParcialErrors"] = _arquivoInfo.msgParcialErros;
                TempData["CaminhoParcialErros"] = _arquivoInfo.caminho;
            }

            return View("SQLIndex");
        }

        
    }
}
