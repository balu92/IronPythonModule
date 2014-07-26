using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Fougerite;
using Fougerite.Events;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace IronPythonModule
{
	public class IPPlugin
	{
		public class Plugin{
			public string Name;
			public string Code;
			public string Path;
			public object Class;
			public ScriptEngine Engine;
			public ScriptScope Scope;
			public IList<string> Globals;

			public Dictionary<string, IPTimedEvent> Timers { get; private set; }

			public Plugin(string name, string code, string path){
				//	Name = name;
				Code = code;
				Path = path;
				Timers = new Dictionary<string, IPTimedEvent>();
				Engine = Python.CreateEngine();
				Scope = Engine.CreateScope();
				Scope.SetVariable("Plugin", this);
				Engine.Execute(code, Scope);
				Class = Engine.Operations.Invoke(Scope.GetVariable(name));
				Globals = Engine.Operations.GetMemberNames(Class); // FIXME: it finds everything, I need only functions
			}

			public void Invoke(string func, params object[] obj) {
				try {
					if (!Globals.Contains(func))
						func = func.Replace ("On", "On_");
					Engine.Operations.InvokeMember (Class, func, obj);
				} catch (Exception ex) {
					Fougerite.Logger.LogException (ex);
				}
			}

			// deal with hooks
			public void OnPluginInit(){
				try {
					string func = "OnPluginInit";
					if (!this.Globals.Contains(func) && this.Globals.Contains("On_PluginInit"))
						func = "On_PluginInit";
					else return;
					Engine.Operations.InvokeMember (Class, func, new object[0]);
				} catch (Exception ex) {
					Fougerite.Logger.LogException (ex);
				}
			}

			public void OnTablesLoaded(Dictionary<string, LootSpawnList> tables) {
				Invoke ("OnTablesLoaded", tables);
			}

			public void OnAllPluginsLoaded(){
				Invoke("OnPluginsLoaded", new object[0]);
			}

			public void OnBlueprintUse (Fougerite.Player player, BPUseEvent evt)
			{
				if (player == null) { // FIXUS: do we really need all these null-checks?
					throw new ArgumentNullException ("player");
				}
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_BlueprintUse", new object[] { player, evt });
			}

			public void OnChat (Fougerite.Player player, ref ChatString text)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				if (text == null) {
					throw new ArgumentNullException ("text");
				}
				this.Invoke ("On_Chat", new object[] { player, text });
			}

			public void OnCommand (Fougerite.Player player, string command, string[] args)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				if (command == null) {
					throw new ArgumentNullException ("command");
				}
				if (args == null) {
					throw new ArgumentNullException ("args");
				}
				this.Invoke ("On_Command", new object[] { player, command, args });
			}

			public void OnConsole (ref ConsoleSystem.Arg arg, bool external)
			{
				if (arg == null) {
					throw new ArgumentNullException ("arg");
				}
				Fougerite.Player player = Fougerite.Player.FindByPlayerClient (arg.argUser.playerClient);
				if (!external) {
					this.Invoke ("On_Console", new object[] { player, arg });
				}
				else {
					this.Invoke ("On_Console", new object[] { null, arg });
				}
			}

			public void OnDoorUse (Fougerite.Player player, DoorEvent evt)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_DoorUse", new object[] { player, evt });
			}

			public void OnEntityDecay (DecayEvent evt)
			{
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_EntityDecay", new object[] { evt });
			}

			public void OnEntityDeployed (Fougerite.Player player, Entity entity)
			{
				if (entity == null) {
					throw new ArgumentNullException ("entity");
				}
				this.Invoke ("On_EntityDeployed", new object[] { player, entity });
			}

			public void OnEntityHurt (HurtEvent evt)
			{
				if (evt == null)
					throw new ArgumentNullException ("evt");
				if (evt.IsDecay)
					return; // FIXME: this could be done better in Fougerite.Hooks.OnEntityHurt
				this.Invoke ("On_EntityHurt", new object[] { evt });
			}

			public void OnItemsLoaded (ItemsBlocks items)
			{
				if (items == null) {
					throw new ArgumentNullException ("items");
				}
				this.Invoke ("On_ItemsLoaded", new object[] { items });
			}

			public void OnNPCHurt (HurtEvent evt)
			{
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_NPCHurt", new object[] { evt });
			}

			public void OnNPCKilled (DeathEvent evt)
			{
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_NPCKilled", new object[] { evt });
			}

			public void OnPlayerConnected (Fougerite.Player player)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				this.Invoke ("On_PlayerConnected", new object []{ player });
			}

			public void OnPlayerDisconnected (Fougerite.Player player)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				this.Invoke ("On_PlayerDisconnected", new object[] { player });
			}

			public void OnPlayerGathering (Fougerite.Player player, GatherEvent evt)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_PlayerGathering", new object[] { player, evt });
			}

			public void OnPlayerHurt (HurtEvent evt)
			{
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_PlayerHurt", new object[] { evt });
			}

			public void OnPlayerKilled (DeathEvent evt)
			{
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_PlayerKilled", new object[] { evt });
			}

			public void OnPlayerSpawn (Fougerite.Player player, SpawnEvent evt)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_PlayerSpawning", new object[] { player, evt });
			}

			public void OnPlayerSpawned (Fougerite.Player player, SpawnEvent evt)
			{
				if (player == null) {
					throw new ArgumentNullException ("player");
				}
				if (evt == null) {
					throw new ArgumentNullException ("evt");
				}
				this.Invoke ("On_PlayerSpawned", new object[] { player, evt });
			}

			public void OnServerInit ()
			{
				this.Invoke ("On_ServerInit", new object[0]);
			}

			public void OnServerShutdown ()
			{
				this.Invoke ("On_ServerShutdown", new object[0]);
			}

			// timer hooks

			public void OnTimerCB (string name)
			{
				if (Globals.Contains (name + "Callback")) {
					Invoke (name + "Callback", new object[0]);
				}
			}

			public void OnTimerCBArgs (string name, Dictionary<string, object> args)
			{
				if (Globals.Contains (name + "Callback")) {
					Invoke (name + "Callback", args);
				}
			}

			// dealt with hooks

			public IPTimedEvent CreateTimer (string name, int timeoutDelay)
			{
				IPTimedEvent result;
				IPTimedEvent timedEvent = GetTimer (name);
				if (timedEvent == null) {
					timedEvent = new IPTimedEvent (name, (double)timeoutDelay);
					timedEvent.OnFire += new IPTimedEvent.TimedEventFireDelegate (OnTimerCB);
					Timers.Add(name, timedEvent);
					result = timedEvent;
				}
				else {
					result = timedEvent;
				}
				return result;
			}

			public IPTimedEvent CreateTimer (string name, int timeoutDelay, Dictionary<string, object> args)
			{
				IPTimedEvent timedEvent = CreateTimer (name, timeoutDelay);
				timedEvent.Args = args;
				timedEvent.OnFire -= new IPTimedEvent.TimedEventFireDelegate (OnTimerCB);
				timedEvent.OnFireArgs += new IPTimedEvent.TimedEventFireArgsDelegate (OnTimerCBArgs);
				return timedEvent;
			}

			public IPTimedEvent GetTimer (string name)
			{
				IPTimedEvent result;
				if (Timers.ContainsKey (name)) {
					result = Timers [name];
				}
				else {
					result = null;
				}
				return result;
			}

			public void KillTimer (string name)
			{
				IPTimedEvent timer = GetTimer (name);
				if (timer != null) {
					timer.Stop ();
					Timers.Remove (name);
				}
			}

			public void KillTimers ()
			{
				foreach (IPTimedEvent current in Timers.Values) {
					current.Stop ();
				}
				Timers.Clear ();
			}

			public void Log (string path, string text)
			{
				string path1 = Path;
				if (path != null) {
					File.AppendAllText (path1.Replace(Name + ".py", path + ".ini"), string.Concat (new string[] {
						"[", DateTime.Now.ToShortDateString (), " ",
						DateTime.Now.ToShortTimeString (), "] ", text, "\r\n" }));
				}
			}

			public Dictionary<string, object> CreateDict() {
				return new Dictionary<string, object>();
			}
		}

		public IPPlugin ()
		{
		}
	}
}

