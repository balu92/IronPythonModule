using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Fougerite;
using Fougerite.Events;
using Microsoft.Scripting.Hosting;

namespace IronPythonModule
{
	public class IPPlugin
	{
		public class Plugin {
			public readonly string Name;
			public readonly string Code;
			public readonly string Path;
			public readonly object Class;
			public readonly ScriptEngine Engine;
			public readonly ScriptScope Scope;
			public readonly IList<string> Globals;

			public readonly Dictionary<string, IPTimedEvent> Timers;

			public Plugin(string name, string code, string path) {
				Name = name;
				Code = code;
				Path = path;
				Timers = new Dictionary<string, IPTimedEvent>();

				Engine = IronPython.Hosting.Python.CreateEngine();
				Scope = Engine.CreateScope();
				Scope.SetVariable("Plugin", this);
				Scope.SetVariable("Server", Fougerite.Server.GetServer());
				Scope.SetVariable("DataStore", DataStore.GetInstance());
				Scope.SetVariable("Util", Util.GetUtil());
				Scope.SetVariable("Entities", new LookUp());
				Scope.SetVariable("World", World.GetWorld());
				Engine.Execute(code, Scope);
				Class = Engine.Operations.Invoke(Scope.GetVariable(name));
				Globals = Engine.Operations.GetMemberNames(Class);
			}

			public void Invoke(string func, params object[] obj) {
				try {
					if (Globals.Contains(func.Replace ("On_", "On")))
						func = func.Replace ("On_", "On");

					if(Globals.Contains(func))
						Engine.Operations.InvokeMember (Class, func, obj);
					else
						Fougerite.Logger.LogDebug ("[IronPython] Function: " + func + " not found in plugin: " + Name);
				} catch (Exception ex) {
					Fougerite.Logger.LogException (ex);
				}
			}

			public bool CreateDir(string path) {
				try {
					string path1 = Path;
					path = path1.Replace (Name + ".py", path);

					if (Directory.Exists(path))
						return false;

					Directory.CreateDirectory(path);

					return true;
				} catch (Exception ex) {
					Logger.LogException(ex);
				}

				return false;
			}

			public IniParser GetIni(string path) {
				string path1 = Path;
				path = path1.Replace (Name + ".py", path + ".ini");

				if (File.Exists(path))
					return new IniParser(path);

				return (IniParser) null;
			}

			public bool IniExists(string path) {
				string path1 = Path;
				path = path1.Replace (Name + ".py", path + ".ini");

				return File.Exists(path);
			}

			public IniParser CreateIni(string path) {
				try {
					string path1 = Path;
					path = path1.Replace (Name + ".py", path + ".ini");

					File.WriteAllText(path, "");
					return new IniParser(path);
				} catch (Exception ex) {
					Logger.LogException(ex);
				}
				return null;
			}

			public List<IniParser> GetInis(string path) {
				string path1 = Path;
				path = path1.Replace (Name + ".py", path);

				return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
			}

			public void DeleteLog(string path) {
				string path1 = Path;
				path = path1.Replace (Name + ".py", path + ".ini");

				if (File.Exists(path))
					File.Delete(path);
			}

			public IPPlugin.Plugin GetPlugin(string name) {
				IPPlugin.Plugin plugin;	
				plugin = IPModule.Plugins[name];
				if (plugin == null)
					return null;
				return plugin;
			}

			public string GetDate() {
				return DateTime.Now.ToShortDateString();
			}

			public int GetTicks() {
				return Environment.TickCount;
			}

			public string GetTime() {
				return DateTime.Now.ToShortTimeString();
			}

			public long GetTimestamp() {
				TimeSpan span = (TimeSpan)(DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0));
				return (long)span.TotalSeconds;
			}

			// deal with hooks
			public void OnTablesLoaded(Dictionary<string, LootSpawnList> tables) {
				Invoke ("On_TablesLoaded", tables);
			}

			public void OnAllPluginsLoaded(){
				Invoke ("On_AllPluginsLoaded", new object[0]);
			}

			public void OnBlueprintUse (Fougerite.Player player, BPUseEvent evt) {
				this.Invoke ("On_BlueprintUse", new object[] { player, evt });
			}

			public void OnChat (Fougerite.Player player, ref ChatString text) {
				Contract.Requires (player != null);
				Contract.Requires (text != null);

				this.Invoke ("On_Chat", new object[] { player, text });
			}

			public void OnCommand (Fougerite.Player player, string command, string[] args) {
				this.Invoke ("On_Command", new object[] { player, command, args });
			}

			public void OnConsole (ref ConsoleSystem.Arg arg, bool external) {
				Fougerite.Player player = Fougerite.Player.FindByPlayerClient (arg.argUser.playerClient);
				if (!external) {
					this.Invoke ("On_Console", new object[] { player, arg });
				} else {
					this.Invoke ("On_Console", new object[] { null, arg });
				}
			}

			public void OnDoorUse (Fougerite.Player player, DoorEvent evt) {
				this.Invoke ("On_DoorUse", new object[] { player, evt });
			}

			public void OnEntityDecay (DecayEvent evt) {
				this.Invoke ("On_EntityDecay", new object[] { evt });
			}

			public void OnEntityDeployed (Fougerite.Player player, Entity entity) {
				this.Invoke ("On_EntityDeployed", new object[] { player, entity });
			}

			public void OnEntityHurt (HurtEvent evt) {
				if (evt.IsDecay)
					return; // FIXME: this could be done better in Fougerite.Hooks.OnEntityHurt
				this.Invoke ("On_EntityHurt", new object[] { evt });
			}

			public void OnItemsLoaded (ItemsBlocks items) {
				this.Invoke ("On_ItemsLoaded", new object[] { items });
			}

			public void OnNPCHurt (HurtEvent evt) {
				this.Invoke ("On_NPCHurt", new object[] { evt });
			}

			public void OnNPCKilled (DeathEvent evt) {
				this.Invoke ("On_NPCKilled", new object[] { evt });
			}

			public void OnPlayerConnected (Fougerite.Player player) {
				this.Invoke ("On_PlayerConnected", new object []{ player });
			}

			public void OnPlayerDisconnected (Fougerite.Player player) {
				this.Invoke ("On_PlayerDisconnected", new object[] { player });
			}

			public void OnPlayerGathering (Fougerite.Player player, GatherEvent evt) {
				this.Invoke ("On_PlayerGathering", new object[] { player, evt });
			}

			public void OnPlayerHurt (HurtEvent evt) {
				this.Invoke ("On_PlayerHurt", new object[] { evt });
			}

			public void OnPlayerKilled (DeathEvent evt) {
				this.Invoke ("On_PlayerKilled", new object[] { evt });
			}

			public void OnPlayerSpawn (Fougerite.Player player, SpawnEvent evt) {
				this.Invoke ("On_PlayerSpawning", new object[] { player, evt });
			}

			public void OnPlayerSpawned (Fougerite.Player player, SpawnEvent evt) {
				this.Invoke ("On_PlayerSpawned", new object[] { player, evt });
			}

			public void OnServerInit () {
				this.Invoke ("On_ServerInit", new object[0]);
			}

			public void OnServerShutdown () {
				this.Invoke ("On_ServerShutdown", new object[0]);
			}

			// timer hooks

			public void OnTimerCB (string name) {
				if (Globals.Contains (name + "Callback")) {
					Invoke (name + "Callback", new object[0]);
				}
			}

			public void OnTimerCBArgs (string name, Dictionary<string, object> args) {
				if (Globals.Contains (name + "Callback")) {
					Invoke (name + "Callback", args);
				}
			}

			// dealt with hooks

			public IPTimedEvent CreateTimer (string name, int timeoutDelay) {
				IPTimedEvent result;
				IPTimedEvent timedEvent = GetTimer (name);
				if (timedEvent == null) {
					timedEvent = new IPTimedEvent (name, (double)timeoutDelay);
					timedEvent.OnFire += new IPTimedEvent.TimedEventFireDelegate (OnTimerCB);
					Timers.Add(name, timedEvent);
					result = timedEvent;
				} else {
					result = timedEvent;
				}
				return result;
			}

			public IPTimedEvent CreateTimer (string name, int timeoutDelay, Dictionary<string, object> args) {
				IPTimedEvent timedEvent = CreateTimer (name, timeoutDelay);
				timedEvent.Args = args;
				timedEvent.OnFire -= new IPTimedEvent.TimedEventFireDelegate (OnTimerCB);
				timedEvent.OnFireArgs += new IPTimedEvent.TimedEventFireArgsDelegate (OnTimerCBArgs);
				return timedEvent;
			}

			public IPTimedEvent GetTimer (string name) {
				IPTimedEvent result;
				if (Timers.ContainsKey (name)) {
					result = Timers [name];
				} else {
					result = null;
				}
				return result;
			}

			public void KillTimer (string name) {
				IPTimedEvent timer = GetTimer (name);
				if (timer != null)
					return;

				timer.Stop ();
				Timers.Remove (name);
			}

			public void KillTimers () {
				foreach (IPTimedEvent current in Timers.Values) {
					current.Stop ();
				}
				Timers.Clear ();
			}

			public void Log (string path, string text) {
				string path1 = Path;
				File.AppendAllText (path1.Replace(Name + ".py", path + ".ini"), string.Concat (new string[] {
					"[", DateTime.Now.ToShortDateString (), " ",
					DateTime.Now.ToShortTimeString (), "] ", text, "\r\n" }));
			}

			public Dictionary<string, object> CreateDict() {
				return new Dictionary<string, object>();
			}
		}
	}
}

