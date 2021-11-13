using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using VuelosCliente.Models;
using VuelosCliente.Models.ViewModels;

namespace VuelosCliente.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index(IndexViewModel viewmodel)
        {
            IndexViewModel vm = new IndexViewModel();
            if (viewmodel == null)
            {
                vm.HttpRespuesta = "";
                vm.IdCode = 0;
            }

            try
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    throw new ArgumentException("Al parecer no está conectado a la red");
                }
                vm.lstVuelos = Registros();
            }
            catch(HttpRequestException)
            {
                vm.IdCode = 2;
                vm.HttpRespuesta = "Error en la conexión, revise si está conectado a Internet";
            }
            catch(WebException)
            {
                vm.IdCode = 2;
                vm.HttpRespuesta = "Error en la conexión, revise si está conectado a Internet";
            }
            catch (Exception)
            {
                vm.IdCode = 2;
                vm.HttpRespuesta = "Error en la conexión, revise si está conectado a Internet";
            }
            return View(vm);
        }

        [Route("Agregar")]
        public IActionResult Agregar(string formJson)
        {
            FormularioViewModel vm;
            if (!string.IsNullOrWhiteSpace(formJson))
            {
                vm = JsonConvert.DeserializeObject<FormularioViewModel>(formJson);
            }
            else
            {
                vm = new FormularioViewModel();
            }
            if (vm.MiRegistro == null || string.IsNullOrWhiteSpace(vm.Mensaje))
            {
                vm.MiRegistro = new Registro { Destino = "", Estado = "", Vuelo = "", Hora = "" };
            }
            else if (vm.MiRegistro.Destino == null && vm.MiRegistro.Vuelo == null && vm.MiRegistro.Hora == null && vm.MiRegistro.Estado == null)
            {
                vm.MiRegistro = new Registro { Destino = "", Estado = "", Vuelo = "", Hora = "" };
            }
            return View(vm);
        }

        [Route("Eliminar/{vuelo}")]
        public IActionResult Eliminar(string vuelo)
        {
            try
            {
                IEnumerable<Registro> registros = Registros();
                Registro reg = registros.FirstOrDefault(x => x.Vuelo == vuelo);
                if (reg == null)
                {
                    return RedirectToAction("Index");
                }
                return View(reg);
            }
            catch (Exception ex)
            {
                IndexViewModel vm = new IndexViewModel();
                vm.lstVuelos = Registros();
                vm.IdCode = 2;
                vm.HttpRespuesta = ex.Message;
                return RedirectToAction("Index", new { vm });
            }

        }

        [Route("Editar/{vuelo}")]
        public IActionResult Editar(string vuelo, string formJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(formJson))
                {
                    IEnumerable<Registro> registros = Registros();
                    Registro reg = registros.FirstOrDefault(x => x.Vuelo == vuelo);
                    if (reg == null)
                    {
                        return RedirectToAction("Index");
                    }
                    FormularioViewModel vm = new FormularioViewModel();
                    vm.MiRegistro = reg;
                    return View(vm);
                }
                else
                {
                    FormularioViewModel vm = JsonConvert.DeserializeObject<FormularioViewModel>(formJson);
                    return View(vm);
                }
                
            }
            catch (Exception ex)
            {
                IndexViewModel vm = new IndexViewModel();
                vm.IdCode = 2;
                vm.HttpRespuesta = ex.Message;
                return RedirectToAction("Index", new { vm });
            }
        }

        [Route("")]
        [HttpPost]
        public IActionResult Index(string Hora, string Destino, string Vuelo, string Estado, string type)
        {
            HttpResponseMessage respuesta;
            IndexViewModel vm = new IndexViewModel();
            HttpClient client = new HttpClient();
            try
            {
                Registro reg = new Registro { Hora = Hora, Destino = Destino, Vuelo = Vuelo, Estado = Estado };
                if (string.IsNullOrWhiteSpace(Hora) || string.IsNullOrWhiteSpace(Vuelo) || string.IsNullOrWhiteSpace(Destino) || string.IsNullOrWhiteSpace(Estado))
                {
                    FormularioViewModel fvm = new FormularioViewModel();
                    fvm.Mensaje = "Debe llenar todos los campos";
                    fvm.MiRegistro = reg;
                    string formJson = JsonConvert.SerializeObject(fvm);

                    if (type=="POST") 
                    {
                        return RedirectToAction("Agregar", new { formJson });
                    }
                    else if(type=="PUT")
                    {
                        return RedirectToAction("Editar", new { Vuelo, formJson });
                    }
                    else 
                    {
                        vm.IdCode = 2;
                        vm.HttpRespuesta = "hay un conflicto con los datos en el formulario";
                        vm.lstVuelos = Registros();
                        return View(vm);
                    }
                }
                string json = JsonConvert.SerializeObject(reg);
                switch (type)
                {
                    case "POST":
                        HttpRequestMessage requPOST = new HttpRequestMessage
                        {
                            Content = new StringContent(json, Encoding.UTF8, "application/json"),
                            Method = HttpMethod.Post,
                            RequestUri = new Uri("http://vuelos.itesrc.net/Tablero")
                        };

                        respuesta = client.SendAsync(requPOST).Result;
                        if (respuesta.IsSuccessStatusCode)
                        {
                            vm.IdCode = 1;
                            vm.HttpRespuesta = $"El vuelo {Vuelo} fue agendado";
                        }
                        else
                        {
                            vm.IdCode = 2;
                        }
                        vm.lstVuelos = Registros();
                        return View(vm);

                    case "DELETE":
                        HttpRequestMessage requDELETE = new HttpRequestMessage
                        {
                            Content = new StringContent(json, Encoding.Default, "application/json"),
                            Method = HttpMethod.Delete,
                            RequestUri = new Uri("http://vuelos.itesrc.net/Tablero")
                        };
                        respuesta = client.SendAsync(requDELETE).Result;
                        if (respuesta.IsSuccessStatusCode)
                        {
                            vm.IdCode = 1;
                            vm.HttpRespuesta = $"El vuelo {Vuelo} fue cancelado";
                        }
                        else
                        {
                            vm.IdCode = 2;
                        }
                        vm.lstVuelos = Registros();
                        return View(vm);

                    case "PUT":
                        vm.lstVuelos = Registros();
                        Registro regC = vm.lstVuelos.FirstOrDefault(x => x.Vuelo == Vuelo);
                        if (regC==null)
                        {
                            vm.IdCode = 2;
                            vm.HttpRespuesta = $"No se logró encontrar el Vuelo {Vuelo}";
                            return View(vm);
                        }
                        HttpRequestMessage requPUT = new HttpRequestMessage
                        {
                            Content = new StringContent(json, Encoding.Default, "application/json"),
                            Method = HttpMethod.Put,
                            RequestUri = new Uri("http://vuelos.itesrc.net/Tablero")
                        };
                        respuesta = client.SendAsync(requPUT).Result;
                        if (respuesta.IsSuccessStatusCode)
                        {
                            vm.IdCode = 1;
                            vm.HttpRespuesta = $"El vuelo {Vuelo} se re-agendó correctamente";
                        }
                        else
                        {
                            vm.IdCode = 2;
                        }
                        vm.lstVuelos = Registros();
                        return View(vm);
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                vm.IdCode = 2;
                vm.HttpRespuesta = "Error en la conexión, revise si está conectado a Internet";
                return RedirectToAction("Index", new { vm });
            }
            return View();
        }

        public IEnumerable<Registro> Registros()
        {
            WebClient web = new WebClient();
            string f = web.DownloadString("http://vuelos.itesrc.net/Tablero");
            var datos = JsonConvert.DeserializeObject<IEnumerable<Registro>>(f);
            return datos;
        }
    }
}
