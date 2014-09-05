namespace IronPythonModule
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Fougerite;

	public class IPModule : Fougerite.Module
	{
		public override string Name { get { return "IPModule"; } }
		public override string Author { get { return "balu92"; } }
		public override string Description { get { return "Python (!Monty)"; } }
		public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

		private static Dictionary<string, IPPlugin> plugins;
		public static Dictionary<string, IPPlugin> Plugins{ get { return plugins; } }
		private DirectoryInfo pluginDirectory;

		#region hooks

		public static event IPModule.AllLoadedDelegate OnAllLoaded;

		public delegate void AllLoadedDelegate();

		public static event IPModule.EntityDestroyedDelegate OnEntityDestroyed;

		public delegate void EntityDestroyedDelegate(Events.DestroyEvent de);

		public static void EntityDestroyed(Events.DestroyEvent de) {
			OnEntityDestroyed(de);
		}

		#endregion

		#region Init/Deinit

		public override void Initialize() {
			pluginDirectory = new DirectoryInfo(ModuleFolder);
			plugins = new Dictionary<string, IPPlugin>();
			ReloadPlugins();
		}

		public override void DeInitialize() {
			UnloadPlugins();
		}

		#endregion

		private IEnumerable<String> GetPluginNames() {
			foreach (DirectoryInfo dirInfo in pluginDirectory.GetDirectories()) {
				string path = Path.Combine(dirInfo.FullName, dirInfo.Name + ".py");
				if (File.Exists(path)) yield return dirInfo.Name;
			}
		}

		private string GetPluginDirectoryPath(string name) {
			return Path.Combine(pluginDirectory.FullName, name);
		}
			
		private string GetPluginScriptPath(string name) {
			return Path.Combine(GetPluginDirectoryPath(name), name + ".py");
		}

		private string GetPluginScriptText(string name) {
			string path = GetPluginScriptPath(name);
			return File.ReadAllText(path);
		}

		#region re/un/loadplugin(s)

		public void LoadPlugins() {
			foreach (string name in GetPluginNames())
				LoadPlugin(name);

			if(OnAllLoaded != null) OnAllLoaded();
		}

		public void UnloadPlugins() {
			foreach (string name in plugins.Keys)
				UnloadPlugin(name);
			plugins.Clear();
		}

		public void ReloadPlugins() {
			UnloadPlugins();
			LoadPlugins();
		}

		public void LoadPlugin(string name) {
			Logger.LogDebug("[IPModule] Loading plugin " + name + ".");

			if (plugins.ContainsKey(name)) {
				Logger.LogError("[IPModule] " + name + " plugin is already loaded.");
				throw new InvalidOperationException("[IPModule] " + name + " plugin is already loaded.");
			}

			try {
				string code = GetPluginScriptText(name);
				DirectoryInfo path = new DirectoryInfo(Path.Combine(pluginDirectory.FullName, name));
				IPPlugin plugin = new IPPlugin(name, code, path);

				InstallHooks(plugin);
				plugins.Add(name, plugin);

				Logger.Log("[IPModule] " + name + " plugin was loaded successfuly.");
			} catch (Exception ex) {
				string arg = name + " plugin could not be loaded.";
				Server.GetServer().BroadcastFrom(Name, arg);
				Logger.LogException(ex);
			}
		}

		public void UnloadPlugin(string name) {
			Logger.LogDebug("[IPModule] Unloading " + name + " plugin.");

			if (plugins.ContainsKey (name)) {
				IPPlugin plugin = plugins [name];

				plugin.KillTimers();
				RemoveHooks(plugin);
				plugins.Remove(name);

				Logger.LogDebug("[IPModule] " + name + " plugin was unloaded successfuly.");
			} else {
				Logger.LogError("[IPModule] Can't unload " + name + ". Plugin is not loaded.");
				throw new InvalidOperationException("[IPModule] Can't unload " + name + ". Plugin is not loaded.");
			}
		}

		public void ReloadPlugin(IPPlugin plugin) {
			UnloadPlugin(plugin.Name);
			LoadPlugin(plugin.Name);
		}

		#endregion

		#region install/remove hooks

		private void InstallHooks(IPPlugin plugin) {
			foreach (string method in plugin.Globals) {
				if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
					continue;

				Logger.LogDebug("Found function: " + method);
				switch (method){
				case "On_ServerInit": Hooks.OnServerInit += new Hooks.ServerInitDelegate(plugin.OnServerInit); break;
				case "On_ServerShutdown": Hooks.OnServerShutdown += new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown); break;
				case "On_ItemsLoaded": Hooks.OnItemsLoaded += new Hooks.ItemsDatablocksLoaded(plugin.OnItemsLoaded); break;
				case "On_TablesLoaded": Hooks.OnTablesLoaded += new Hooks.LootTablesLoaded(plugin.OnTablesLoaded); break;
				case "On_Chat": Hooks.OnChat += new Hooks.ChatHandlerDelegate(plugin.OnChat); break;
				case "On_Console": Hooks.OnConsoleReceived += new Hooks.ConsoleHandlerDelegate(plugin.OnConsole); break;
				case "On_Command": Hooks.OnCommand += new Hooks.CommandHandlerDelegate(plugin.OnCommand); break;
				case "On_PlayerConnected": Hooks.OnPlayerConnected += new Hooks.ConnectionHandlerDelegate(plugin.OnPlayerConnected); break;
				case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected += new Hooks.DisconnectionHandlerDelegate(plugin.OnPlayerDisconnected); break;
				case "On_PlayerKilled": Hooks.OnPlayerKilled += new Hooks.KillHandlerDelegate(plugin.OnPlayerKilled); break;
				case "On_PlayerHurt": Hooks.OnPlayerHurt += new Hooks.HurtHandlerDelegate(plugin.OnPlayerHurt); break;
				case "On_PlayerSpawn": Hooks.OnPlayerSpawning += new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawn); break;
				case "On_PlayerSpawned": Hooks.OnPlayerSpawned += new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawned); break;
				case "On_PlayerGathering": Hooks.OnPlayerGathering += new Hooks.PlayerGatheringHandlerDelegate(plugin.OnPlayerGathering); break;
				case "On_EntityHurt": Hooks.OnEntityHurt += new Hooks.EntityHurtDelegate(plugin.OnEntityHurt); break;
				case "On_EntityDecay": Hooks.OnEntityDecay += new Hooks.EntityDecayDelegate(plugin.OnEntityDecay); break;
				case "On_EntityDestroyed": IPModule.OnEntityDestroyed += new IPModule.EntityDestroyedDelegate(plugin.OnEntityDestroyed); break;
				case "On_EntityDeployed": Hooks.OnEntityDeployed += new Hooks.EntityDeployedDelegate(plugin.OnEntityDeployed); break;
				case "On_NPCHurt": Hooks.OnNPCHurt += new Hooks.HurtHandlerDelegate(plugin.OnNPCHurt); break;
				case "On_NPCKilled": Hooks.OnNPCKilled += new Hooks.KillHandlerDelegate(plugin.OnNPCKilled); break;
				case "On_BlueprintUse": Hooks.OnBlueprintUse += new Hooks.BlueprintUseHandlerDelagate(plugin.OnBlueprintUse); break;
				case "On_DoorUse": Hooks.OnDoorUse += new Hooks.DoorOpenHandlerDelegate(plugin.OnDoorUse); break;
				case "On_AllPluginsLoaded": IPModule.OnAllLoaded += new IPModule.AllLoadedDelegate(plugin.OnAllPluginsLoaded); break;
				case "On_PluginInit": plugin.Invoke("On_PluginInit", new object[0]); break;
				}
			}
		}

		private void RemoveHooks(IPPlugin plugin) {
			foreach (string method in plugin.Globals) {
				if (!method.StartsWith("On_") && !method.EndsWith("Callback"))
					continue;

				Logger.LogDebug("Removing function: " + method);
				switch (method){
				case "On_ServerInit": Hooks.OnServerInit -= new Hooks.ServerInitDelegate(plugin.OnServerInit); break;
				case "On_ServerShutdown": Hooks.OnServerShutdown -= new Hooks.ServerShutdownDelegate(plugin.OnServerShutdown); break;
				case "On_ItemsLoaded": Hooks.OnItemsLoaded -= new Hooks.ItemsDatablocksLoaded(plugin.OnItemsLoaded); break;
				case "On_TablesLoaded": Hooks.OnTablesLoaded -= new Hooks.LootTablesLoaded(plugin.OnTablesLoaded); break;
				case "On_Chat": Hooks.OnChat -= new Hooks.ChatHandlerDelegate(plugin.OnChat); break;
				case "On_Console": Hooks.OnConsoleReceived -= new Hooks.ConsoleHandlerDelegate(plugin.OnConsole); break;
				case "On_Command": Hooks.OnCommand -= new Hooks.CommandHandlerDelegate(plugin.OnCommand); break;
				case "On_PlayerConnected": Hooks.OnPlayerConnected -= new Hooks.ConnectionHandlerDelegate(plugin.OnPlayerConnected); break;
				case "On_PlayerDisconnected": Hooks.OnPlayerDisconnected -= new Hooks.DisconnectionHandlerDelegate(plugin.OnPlayerDisconnected); break;
				case "On_PlayerKilled": Hooks.OnPlayerKilled -= new Hooks.KillHandlerDelegate(plugin.OnPlayerKilled); break;
				case "On_PlayerHurt": Hooks.OnPlayerHurt -= new Hooks.HurtHandlerDelegate(plugin.OnPlayerHurt); break;
				case "On_PlayerSpawn": Hooks.OnPlayerSpawning -= new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawn); break;
				case "On_PlayerSpawned": Hooks.OnPlayerSpawned -= new Hooks.PlayerSpawnHandlerDelegate(plugin.OnPlayerSpawned); break;
				case "On_PlayerGathering": Hooks.OnPlayerGathering -= new Hooks.PlayerGatheringHandlerDelegate(plugin.OnPlayerGathering); break;
				case "On_EntityHurt": Hooks.OnEntityHurt -= new Hooks.EntityHurtDelegate(plugin.OnEntityHurt); break;
				case "On_EntityDecay": Hooks.OnEntityDecay -= new Hooks.EntityDecayDelegate(plugin.OnEntityDecay); break;
				case "On_EntityDestroyed": IPModule.OnEntityDestroyed -= new IPModule.EntityDestroyedDelegate(plugin.OnEntityDestroyed); break;
				case "On_EntityDeployed": Hooks.OnEntityDeployed -= new Hooks.EntityDeployedDelegate(plugin.OnEntityDeployed); break;
				case "On_NPCHurt": Hooks.OnNPCHurt -= new Hooks.HurtHandlerDelegate(plugin.OnNPCHurt); break;
				case "On_NPCKilled": Hooks.OnNPCKilled -= new Hooks.KillHandlerDelegate(plugin.OnNPCKilled); break;
				case "On_BlueprintUse": Hooks.OnBlueprintUse -= new Hooks.BlueprintUseHandlerDelagate(plugin.OnBlueprintUse); break;
				case "On_DoorUse": Hooks.OnDoorUse -= new Hooks.DoorOpenHandlerDelegate(plugin.OnDoorUse); break;
				case "On_AllPluginsLoaded": IPModule.OnAllLoaded -= new IPModule.AllLoadedDelegate(plugin.OnAllPluginsLoaded); break;
				}
			}
		}

		#endregion
	}
}

