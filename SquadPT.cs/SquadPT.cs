using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;

namespace WindowsGSM.Plugins
{
    public class SquadPT : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "Squad Public Test", // WindowsGSM.XXXX
            author = "DiscoveryOV",
            description = "WindowsGSM plugin that adds support for Squad Public Test server.",
            version = "1.1",
            url = "https://github.com/DiscoveryOV/WindowsGSM.SquadPT", // Github repository link (Best practice)
            color = "#ffc40b" // Color Hex
        };

        // - Settings properties for SteamCMD installer
        //public override bool loginAnonymous => false;
        public override string AppId => "774961"; // Game server appId, the Squad PT is 774961

        // - Standard Constructor and properties
        public SquadPT(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;


        // - Game server Fixed variables
        public override string StartPath => "SquadGameServer.exe"; // Game server start path
        public string FullName = "Squad Public Test server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()


        // - Game server default values
        public string Port = "7787"; // Default port
        public string QueryPort = "27165"; // Default query port
        public string Defaultmap = ""; // Default map name
        public string Maxplayers = "100"; // Default maxplayers
        public string Additional = ""; // Additional server start parameter


        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            // Creating config path
            string configDir = Path.Combine(ServerPath.GetServersServerFiles(_serverData.ServerID), @"SquadGame\ServerConfig");
            Directory.CreateDirectory(configDir);

            // cfg files
            string serverCFG = Path.Combine(configDir, "Server.cfg");

            var sb = new StringBuilder();
            sb.Append($"-log");
            sb.Append(string.IsNullOrWhiteSpace(_serverData.ServerIP) ? string.Empty : $" MULTIHOME={_serverData.ServerIP}");
            sb.Append(string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $" Port={_serverData.ServerPort}");
            sb.Append(string.IsNullOrWhiteSpace(_serverData.ServerQueryPort) ? string.Empty : $" QueryPort={_serverData.ServerQueryPort}");
            sb.Append(string.IsNullOrWhiteSpace(_serverData.ServerMaxPlayer) ? string.Empty : $" FIXEDMAXPLAYERS={_serverData.ServerMaxPlayer}");
            sb.Append(string.IsNullOrWhiteSpace(_serverData.ServerParam) ? string.Empty : $" {_serverData.ServerParam}");
        }


        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string runPath = Path.Combine(ServerPath.GetServersServerFiles(_serverData.ServerID), "SquadGame\\Binaries\\Win64\\SquadGameServer.exe");

            string param = "-log";
            param += $" {_serverData.ServerParam}";

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = runPath,
                    Arguments = param.ToString()
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;

                // Start Process
                try
                {
                    p.Start();
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null; // return null if fail to start
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                return p;
            }

            // Start Process
            try
            {
                p.Start();
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }


        // - Stop server function
        public async Task Stop(Process p) => await Task.Run(() => { p.Kill(); });
    }
}
