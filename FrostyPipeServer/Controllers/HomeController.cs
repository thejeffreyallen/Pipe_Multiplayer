using FrostyPipeServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FrostyPipeServer.ServerFiles;

namespace FrostyPipeServer.Controllers
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

        public IActionResult Configuration()
        {
            return View();
        }

        public IActionResult Docs()
        {
            return View();
        }

        public IActionResult Players()
        {
            return View(new PlayersModel(Servermanager.Players.Values.ToList()));
        }

        public IActionResult Objects()
        {
            return View(new PlayersModel(Servermanager.Players.Values.ToList()));
        }

        public IActionResult Games()
        {
            return View();
        }

        [HttpGet("/serverconfig")]
        public string Getconfig()
        {
            return Servermanager.GiveConfigasJSONString();
        }

        [HttpPost("/applyconfig")]
        public string ApplyChangestoConfig([FromHeader] string jsonconfig)
        {
            bool success = Servermanager.OverwriteConfigFile(jsonconfig);
            if (!success)
            {
                return "Something went wrong";
            }
            else
            {
                return "Changes Applied";
            }
        }

        [HttpGet("/serverstats")]
        public string GetStats()
        {
            return Servermanager.GiveStatsasJSONString();
        }

        [HttpGet("/reloadconfig")]
        public string SendReloadConfig()
        {
            Servermanager.ReloadConfig();
            return "Reloaded config";
        }

        [HttpPost("/banridersend")]
        public string BanRiderSend([FromHeader] string id, [FromHeader] string mins)
        {
            if (ushort.TryParse(id, out ushort playerid))
            {
                int _mins = int.Parse(mins);
                Console.WriteLine("Banning id: " + playerid + " for " + _mins.ToString() + " Mins");
                if (Servermanager.Players.ContainsKey(playerid))
                {
                    ThreadTransfer.GiveToSystemThread(() =>
                    {
                        Servermanager.BanPlayer(Servermanager.Players[playerid].Username, Servermanager.GetPlayerIP(playerid), playerid, _mins);

                    });
                    return "Ban rider Applied";
                }
                else
                {
                    return "No player found";
                }

            }
            else
            {
                return "Bad id";
            }
        }

        [HttpGet("/Garagesetup")]
        public string Garagesetup([FromHeader] string riderid)
        {
            if (ushort.TryParse((string)riderid, out ushort playerid))
            {
                if (Servermanager.Players.ContainsKey(playerid))
                {
                    if (Servermanager.Players[playerid].GarageEnabled)
                    {
                        return Servermanager.Players[playerid].Gear.garagexml;
                    }
                    else
                    {
                        return "Garage Disabled";
                    }
                }
            }

            return "Garage Disabled";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
