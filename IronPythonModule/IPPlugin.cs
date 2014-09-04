using System;
using System.Linq;
using System.Reflection;
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
			public readonly object Class;
			public readonly DirectoryInfo RootDir;
			public readonly ScriptEngine Engine;
			public readonly ScriptScope Scope;
			public readonly IList<string> Globals;

			public readonly Dictionary<string, IPTimedEvent> Timers;

			public Plugin(string name, string code, DirectoryInfo path) {
				Name = name;
				Code = code;
				RootDir = path;
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
					if (Globals.Contains(func))
						Engine.Operations.InvokeMember (Class, func, obj);
					else
						Fougerite.Logger.LogDebug ("[IronPython] Function: " + func + " not found in plugin: " + Name);
				} catch (Exception ex) {
					Fougerite.Logger.LogException (ex);
				}
			}

			#region file operations

			private static string NormalizePath(string path) {
				return Path.GetFullPath(new Uri(path).LocalPath)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}

			private string ValidateRelativePath(string path) {
				string normalizedPath = NormalizePath(Path.Combine(RootDir.FullName, path));
				string rootDirNormalizedPath = NormalizePath(RootDir.FullName);

				if (!normalizedPath.StartsWith(rootDirNormalizedPath))
					return null;

				return normalizedPath;
			}

			public bool CreateDir(string path) {
				try {
					path = ValidateRelativePath(path);
					if (path == null)
						return false;

					if (Directory.Exists(path))
						return true;

					Directory.CreateDirectory(path);
					return true;
				} catch (Exception ex) {
					Logger.LogException(ex);
				}
				return false;
			}

			public void ToJsonFile(string path, string json)
			{
				path = ValidateRelativePath(path + ".json");
				File.WriteAllText(path, json);
			}

			public string FromJsonFile(string path)
			{
				string json = @"";
				path = ValidateRelativePath(path + ".json");
				if (File.Exists(path))
					json = File.ReadAllText(path);

				return json;
			}

			public void DumpPropsToFile(string path, object obj) {
				path = ValidateRelativePath(path + ".dump");
				if (path == null)
					return;

				string objprops;
				string nuline = "\r\n";

				if (obj is AppDomain)
					objprops = "Public properties of appDomain: " + ((AppDomain)obj).FriendlyName + nuline + nuline;
				else
					objprops = "Public properties of: " + obj.GetType() + nuline + nuline;

				PropertyInfo[] pInfos = obj.GetType().GetProperties();

				foreach (PropertyInfo pInfo in pInfos) {
					objprops += pInfo.Name + " = " + pInfo.GetValue (obj, null).ToString () + nuline;
				}

				File.AppendAllText (path, objprops + nuline);
			}

			public void DeleteLog(string path)
			{
				path = ValidateRelativePath(path + ".log");
				if (path == null)
					return;

				if (File.Exists(path))
					File.Delete(path);
			}

			public void Log(string path, string text)
			{
				path = ValidateRelativePath(path + ".log");
				if (path == null)
					return;

				File.AppendAllText(path, "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + text + "\r\n");
			}

			public void RotateLog(string logfile, int max = 6)
			{
				logfile = ValidateRelativePath(logfile + ".log");
				if (logfile == null)
					return;

				string pathh, pathi;
				int i, h;
				for (i = max, h = i - 1; i > 1; i--, h--) {
					pathi = ValidateRelativePath(logfile + i + ".log");
					pathh = ValidateRelativePath(logfile + h + ".log");

					try {
						if (!File.Exists(pathi))
							File.Create(pathi);

						if (!File.Exists(pathh)) {
							File.Replace(logfile, pathi, null);
						} else {
							File.Replace(pathh, pathi, null);
						}
					} catch (Exception ex) {
						Logger.LogError("[JintPlugin] RotateLog " + logfile + ", " + pathh + ", " + pathi + ", " + ex);
						continue;
					}
				}
			}

			#endregion

			#region inifiles

			public IniParser GetIni(string path) {
				path = ValidateRelativePath(path + ".ini");
				if (path == null)
					return;

				if (File.Exists(path))
					return new IniParser(path);

				return (IniParser) null;
			}

			public bool IniExists(string path) {
				path = ValidateRelativePath(path + ".ini");
				if (path == null)
					return (IniParser) null;

				return File.Exists(path);
			}

			public IniParser CreateIni(string path) {
				try {
					path = ValidateRelativePath(path + ".ini");
					if (path == null)
						return (IniParser) null;

					File.WriteAllText(path, "");
					return new IniParser(path);
				} catch (Exception ex) {
					Logger.LogException(ex);
				}
				return (IniParser) null;
			}

			public List<IniParser> GetInis(string path) {
				path = ValidateRelativePath(path);
				if (path == null)

				return Directory.GetFiles(path).Select(p => new IniParser(p)).ToList();
			}

			#endregion

			public IPPlugin.Plugin GetPlugin(string name) {
				IPPlugin.Plugin plugin;	
				plugin = IPModule.Plugins[name];
				if (plugin == null)
					return null;
				return plugin;
			}

			#region time
			// CONSIDER: putting these into a separate class along with some new shortcut
			//				Time.GetDate() looks more appropriate than Plugin.GetDate()
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

			#endregion

			#region hooks

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
				// CONSIDER: if(!evt.Entity...IsAlive) return;

				this.Invoke ("On_EntityDecay", new object[] { evt });
			}

			public void OnEntityDeployed (Fougerite.Player player, Entity entity) {
				this.Invoke ("On_EntityDeployed", new object[] { player, entity });
			}

			public void OnEntityDestroyed (Events.DestroyEvent evt) {
				this.Invoke ("On_EntityDestroyed", new object[] { evt });
			}

			public void OnEntityHurt (HurtEvent evt) {
				if (evt.DamageEvent.status != LifeStatus.IsAlive) {
					Events.DestroyEvent de = new Events.DestroyEvent(ref evt.DamageEvent, evt.Entity, evt.IsDecay);
					IPModule.OnEntityDestroyed (de);
					return;
				}

				if (evt.IsDecay)
					return; // NOTE: this should be done in Fougerite.Hooks imo

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

			#endregion

			#region timer methods

			public IPTimedEvent CreateTimer (string name, int timeoutDelay) {
				IPTimedEvent timedEvent = GetTimer (name);
				if (timedEvent == null) {
					timedEvent = new IPTimedEvent (name, (double)timeoutDelay);
					timedEvent.OnFire += new IPTimedEvent.TimedEventFireDelegate (OnTimerCB);
					Timers.Add(name, timedEvent);
				}
				return timedEvent;
			}

			public IPTimedEvent CreateTimer (string name, int timeoutDelay, Dictionary<string, object> args) {
				IPTimedEvent timedEvent = GetTimer (name);
				if (timedEvent == null) {
					timedEvent = new IPTimedEvent (name, (double)timeoutDelay);
					timedEvent.Args = args;
					timedEvent.OnFireArgs += new IPTimedEvent.TimedEventFireArgsDelegate (OnTimerCBArgs);
					Timers.Add(name, timedEvent);
				}
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

			#endregion

			// temporarly, for my laziness
			public Dictionary<string, object> CreateDict() {
				return new Dictionary<string, object>();
			}
		}
	}
}

