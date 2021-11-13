using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VuelosCliente.Models;

namespace VuelosCliente.Models.ViewModels
{
    public class IndexViewModel
    {
        public int IdCode { get; set; }
        public string HttpRespuesta { get; set; }
        public IEnumerable<Registro> lstVuelos { get; set; }
    }
}
