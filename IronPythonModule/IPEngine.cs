using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Fougerite;

namespace IronPythonModule
{
	public class IPEngine : Fougerite.Module
	{
		public override string Name { get { return "IPEngine"; } }
		public override string Author { get { return "balu92"; } }
		public override string Description { get { return "Python (!Monty)"; } }
		public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		private static Dictionary<string, IPPlugin.Plugin> plugins = new Dictionary<string, IPPlugin.Plugin>();

		public static Dictionary<string, IPPlugin.Plugin> Plugins { get { return plugins;} }

		private static string pluginsPath = "modules/IronPythonModule/plugins/";

		private static string[] f = { "IO", "File.", "AppendText", "AppendAllText", "OpenWrite", "WriteAll" };

		// hooks
		public static event IPEngine.AllLoadedDelegate OnAllLoaded;

		public delegate void AllLoadedDelegate();

		//public static void AllPluginsLoaded() { if(OnAllLoaded != null) OnAllLoaded(); } // FIXME: why is it always null?
		// hooks end

		public override void Initialize () {
			if (plugins.Count != 0)
				foreach (IPPlugin.Plugin plug in plugins.Values)
					RemoveHooks (plug);
			IPEngine.plugins.Clear ();
			string[] directories = Directory.GetDirectories(pluginsPath);
			foreach (string pluginDir in directories) {

				// get the code
				string shortname = Path.GetFileName (pluginDir);
				string pluginName = shortname + ".py";
				string path = pluginDir + "/" + pluginName;
				if (!File.Exists (path))
					continue;

				string script = System.IO.File.ReadAllText (path);
				var t = true;
				foreach (string fi in f)
					if (script.Contains (fi)) { t = false;}

				if (!t)
					continue;

				var plugin = new IPPlugin.Plugin (shortname, script, path);
				InstallHooks (plugin);
				plugins.Add (shortname, plugin);
			}
			if(OnAllLoaded != null) OnAllLoaded();
		}

		private void InstallHooks(IPPlugin.Plugin plugin){
			foreach(string method in plugin.Globals){
				if (method.Contains ("__"))
					continue;
				Logger.LogDebug ("Found function: " + method);
				switch (method){
				case "OnServerInit": case "On_ServerInit":
					Hooks.OnServerInit += new Hooks.ServerInitDelegate (plugin.OnServerInit);
					break;
				case "OnServerShutdown": case "On_ServerShutdown":
					Hooks.OnServerShutdown += new Hooks.ServerShutdownDelegate (plugin.OnServerShutdown);
					break;
				case "OnItemsLoaded": case "On_ItemsLoaded":
					Hooks.OnItemsLoaded += new Hooks.ItemsDatablocksLoaded (plugin.OnItemsLoaded);
					break;
				case "OnTablesLoaded": case "On_TablesLoaded":
					Hooks.OnTablesLoaded += new Hooks.LootTablesLoaded (plugin.OnTablesLoaded);
					break;
				case "OnChat": case "On_Chat":
					Hooks.OnChat += new Hooks.ChatHandlerDelegate (plugin.OnChat);
					break;
				case "OnConsole": case "On_Console":
					Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate (plugin.OnConsole);
					break;
				case "OnCommand": case "On_Command":
					Hooks.OnCommand += new Hooks.CommandHandlerDelegate (plugin.OnCommand);
					break;
				case "OnPlayerConnected": case "On_PlayerConnected":
					Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate (plugin.OnPlayerConnected);
					break;
				case "OnPlayerDisconnected": case "On_PlayerDisconnected":
					Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate (plugin.OnPlayerDisconnected);
					break;
				case "OnPlayerKilled": case "On_PlayerKilled":
					Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate (plugin.OnPlayerKilled);
					break;
				case "OnPlayerHurt": case "On_PlayerHurt":
					Hooks.OnPlayerHurt += new Hooks.HurtHandlerDelegate (plugin.OnPlayerHurt);
					break;
				case "OnPlayerSpawn": case "On_PlayerSpawn":
					Hooks.OnPlayerSpawning += new Hooks.PlayerSpawnHandlerDelegate (plugin.OnPlayerSpawn);
					break;
				case "OnPlayerSpawned": case "On_PlayerSpawned":
					Hooks.OnPlayerSpawned += new Hooks.PlayerSpawnHandlerDelegate (plugin.OnPlayerSpawned);
					break;
				case "OnPlayerGathering": case "On_PlayerGathering":
					Hooks.OnPlayerGathering += new Hooks.PlayerGatheringHandlerDelegate (plugin.OnPlayerGathering);
					break;
				case "OnEntityHurt": case "On_EntityHurt":
					Hooks.OnEntityHurt += new Hooks.EntityHurtDelegate (plugin.OnEntityHurt);
					break;
				case "OnEntityDecay": case "On_EntityDecay":
					Hooks.OnEntityDecay += new Hooks.EntityDecayDelegate (plugin.OnEntityDecay);
					break;
				case "OnEntityDeployed": case "On_EntityDeployed":
					Hooks.OnEntityDeployed += new Hooks.EntityDeployedDelegate (plugin.OnEntityDeployed);
					break;
				case "OnNPCHurt": case "On_NPCHurt":
					Hooks.OnNPCHurt += new Hooks.HurtHandlerDelegate (plugin.OnNPCHurt);
					break;
				case "OnNPCKilled": case "On_NPCKilled":
					Hooks.OnNPCKilled += new Hooks.KillHandlerDelegate (plugin.OnNPCKilled);
					break;
				case "OnBlueprintUse": case "On_BlueprintUse":
					Hooks.OnBlueprintUse += new Hooks.BlueprintUseHandlerDelegate(plugin.OnBlueprintUse);
					break;
				case "OnDoorUse": case "On_DoorUse":
					Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate (plugin.OnDoorUse);
					break;
				case "OnAllPluginsLoaded": case "On_AllPluginsLoaded":
					IPEngine.OnAllLoaded += new IPEngine.AllLoadedDelegate (plugin.OnAllPluginsLoaded);
					break;
				case "OnPluginInit": case "On_PluginInit":
					plugin.Invoke ("OnPluginInit", new object[0]);
					break;
				}
			}
		}

		private void RemoveHooks(IPPlugin.Plugin plugin){
			foreach(string method in plugin.Globals){
				if (method.Contains ("__"))
					continue;
				Logger.LogDebug ("Removing function: " + method);
				switch (method){
				case "OnServerInit": case "On_ServerInit":
					Hooks.OnServerInit -= new Hooks.ServerInitDelegate (plugin.OnServerInit);
					break;
				case "OnServerShutdown": case "On_ServerShutdown":
					Hooks.OnServerShutdown -= new Hooks.ServerShutdownDelegate (plugin.OnServerShutdown);
					break;
				case "OnItemsLoaded": case "On_ItemsLoaded":
					Hooks.OnItemsLoaded -= new Hooks.ItemsDatablocksLoaded (plugin.OnItemsLoaded);
					break;
				case "OnTablesLoaded": case "On_TablesLoaded":
					Hooks.OnTablesLoaded -= new Hooks.LootTablesLoaded (plugin.OnTablesLoaded);
					break;
				case "OnChat": case "On_Chat":
					Hooks.OnChat -= new Hooks.ChatHandlerDelegate (plugin.OnChat);
					break;
				case "OnConsole": case "On_Console":
					Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate (plugin.OnConsole);
					break;
				case "OnCommand": case "On_Command":
					Hooks.OnCommand -= new Hooks.CommandHandlerDelegate (plugin.OnCommand);
					break;
				case "OnPlayerConnected": case "On_PlayerConnected":
					Hooks.OnPlayerConnected -= new Hooks.ConnectionHandlerDelegate (plugin.OnPlayerConnected);
					break;
				case "OnPlayerDisconnected": case "On_PlayerDisconnected":
					Hooks.OnPlayerDisconnected -= new Hooks.DisconnectionHandlerDelegate (plugin.OnPlayerDisconnected);
					break;
				case "OnPlayerKilled": case "On_PlayerKilled":
					Hooks.OnPlayerKilled -= new Hooks.KillHandlerDelegate (plugin.OnPlayerKilled);
					break;
				case "OnPlayerHurt": case "On_PlayerHurt":
					Hooks.OnPlayerHurt -= new Hooks.HurtHandlerDelegate (plugin.OnPlayerHurt);
					break;
				case "OnPlayerSpawn": case "On_PlayerSpawn":
					Hooks.OnPlayerSpawning -= new Hooks.PlayerSpawnHandlerDelegate (plugin.OnPlayerSpawn);
					break;
				case "OnPlayerSpawned": case "On_PlayerSpawned":
					Hooks.OnPlayerSpawned -= new Hooks.PlayerSpawnHandlerDelegate (plugin.OnPlayerSpawned);
					break;
				case "OnPlayerGathering": case "On_PlayerGathering":
					Hooks.OnPlayerGathering -= new Hooks.PlayerGatheringHandlerDelegate (plugin.OnPlayerGathering);
					break;
				case "OnEntityHurt": case "On_EntityHurt":
					Hooks.OnEntityHurt -= new Hooks.EntityHurtDelegate (plugin.OnEntityHurt);
					break;
				case "OnEntityDecay": case "On_EntityDecay":
					Hooks.OnEntityDecay -= new Hooks.EntityDecayDelegate (plugin.OnEntityDecay);
					break;
				case "OnEntityDeployed": case "On_EntityDeployed":
					Hooks.OnEntityDeployed -= new Hooks.EntityDeployedDelegate (plugin.OnEntityDeployed);
					break;
				case "OnNPCHurt": case "On_NPCHurt":
					Hooks.OnNPCHurt -= new Hooks.HurtHandlerDelegate (plugin.OnNPCHurt);
					break;
				case "OnNPCKilled": case "On_NPCKilled":
					Hooks.OnNPCKilled -= new Hooks.KillHandlerDelegate (plugin.OnNPCKilled);
					break;
				case "OnBlueprintUse": case "On_BlueprintUse":
					Hooks.OnBlueprintUse -= new Hooks.BlueprintUseHandlerDelegate(plugin.OnBlueprintUse);
					break;
				case "OnDoorUse": case "On_DoorUse":
					Hooks.OnDoorUse -= new Hooks.DoorOpenHandlerDelegate (plugin.OnDoorUse);
					break;
				case "OnAllPluginsLoaded": case "On_AllPluginsLoaded":
					IPEngine.OnAllLoaded -= new IPEngine.AllLoadedDelegate (plugin.OnAllPluginsLoaded);
					break;
				}
			}
		}

		public IPEngine () { }
	}
}

