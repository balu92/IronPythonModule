import clr
import sys
clr.AddReferenceByPartialName("UnityEngine")
clr.AddReferenceByPartialName("Fougerite")
import UnityEngine
import Fougerite
from UnityEngine import Debug

class Test:
	def On_TablesLoaded(self, tables):
#		Debug.Log("On_TablesLoaded hooked with " + tables.Count.ToString() + " element, from Python")
		Plugin.TryDumpObjToFile("On_TablesLoaded.Tables", tables)
		return tables

	def On_AllPluginsLoaded(self):
		Debug.Log("The knights who say NI!")

	def On_ServerInit(self):
		Debug.Log("On_ServerInit hooked from Python")
		dic = Plugin.CreateDict()
		dic.Add("first", "timer is:")
		dic.Add("second", "working")
		Plugin.CreateTimer("testtimer", 5000, dic).Start()

	def testtimerCallback(self, dic):
		plug2 = Plugin.GetPlugin("Test2")
		plug2.Invoke("TestSharedFunction", dic["first"], dic["second"])
		Debug.Log(dic["first"])
		Debug.Log(dic["second"])
		Plugin.TryDumpObjToFile("testtimerCallback.dic", dic)
		Plugin.KillTimers()

	def On_PluginInit(self):
		Debug.Log("On_PluginInit hooked from Python")

	def On_ServerShutdown(self):
		Debug.Log("On_ServerShutdown hooked from Python")

	def On_ItemsLoaded(self, items):
#		Debug.Log("On_ItemsLoaded hooked with " + items.Count + " element, from Python")
		Plugin.TryDumpObjToFile("On_ItemsLoaded.Items", items)
		return items

	def On_Chat(self, Player, Text):
#		Debug.Log(Player.Name + " says: " + Text)
		Plugin.TryDumpObjToFile("On_Chat.Player", Player)
		Plugin.TryDumpObjToFile("On_Chat.Text", Text)

	def On_BlueprintUse(self, Player, BPUseEvent):
#		Debug.Log(Player.Name + " researched " + BPUseEvent.ItemName)
		Plugin.TryDumpObjToFile("On_BlueprintUse.Player", Player)
		Plugin.TryDumpObjToFile("On_BlueprintUse.BPUseEvent", BPUseEvent)

	def On_Command(self, Player, cmd, args):
#		Debug.Log("On_Command(" + Player.Name + ", " + cmd + ", " + args + ")")
		Plugin.TryDumpObjToFile("On_Command.Player", Player)
		Plugin.TryDumpObjToFile("On_Command.cmd", cmd)
		Plugin.TryDumpObjToFile("On_Command.args", args)

	def On_Console(self, Player, Arg):
#		Debug.Log(Player.Name + " used " + Arg.Class + "." + Arg.Function + " in console ")
		Plugin.TryDumpObjToFile("On_Console.Player", Player)
		Plugin.TryDumpObjToFile("On_Console.Arg", Arg)

	def On_DoorUse(self, Player, DoorEvent):
#		Debug.Log(Player.Name + " tried to use a door")
#		Debug.Log("Succeded? " + ("yes" if DoorEvent.Open else "no"))
		Plugin.TryDumpObjToFile("On_DoorUse.Player", Player)
		Plugin.TryDumpObjToFile("On_DoorUse.DoorEvent", DoorEvent)

	def On_EntityDecay(self, DecayEvent):
		Plugin.TryDumpObjToFile("On_EntityDecay.DecayEvent", DecayEvent)
		return DecayEvent.DamageAmount

	def On_EntityDeployed(self, Player, Enity):
		Plugin.TryDumpObjToFile("On_EntityDeployed.Player", Player)
		Plugin.TryDumpObjToFile("On_EntityDeployed.Enity", Enity)

	def On_EntityHurt(self, he):
		Plugin.TryDumpObjToFile("On_EntityHurt.HurtEvent", he)

	def On_EntityDestroyed(self, de):
		Plugin.TryDumpObjToFile("On_EntityDestroyed.DestroyEvent", de)

	def On_NPCKilled(self, de):
		Plugin.TryDumpObjToFile("On_NPCKilled.DeathEvent", de)

	def On_NPCHurt(self, he):
		Plugin.TryDumpObjToFile("On_NPCHurt.HurtEvent", he)

	def On_PlayerGathering(self, Player, ge):
		Plugin.TryDumpObjToFile("On_PlayerGathering.Player", Player)
		Plugin.TryDumpObjToFile("On_PlayerGathering.GatherEvent", ge)

	def On_PlayerSpawning(self, Player, se):
		Plugin.TryDumpObjToFile("On_PlayerSpawn.Player", Player)
		Plugin.TryDumpObjToFile("On_PlayerSpawn.SpawnEvent", se)

	def On_PlayerSpawned(self, Player, se):
		Plugin.TryDumpObjToFile("On_PlayerSpawned.Player", Player)
		Plugin.TryDumpObjToFile("On_PlayerSpawned.SpawnEvent", se)

	def On_PlayerKilled(self, de):
		Plugin.TryDumpObjToFile("On_PlayerKilled.DeathEvent", de)

	def On_PlayerHurt(self, he):
		Plugin.TryDumpObjToFile("On_PlayerHurt.HurtEvent", he)

	def On_PlayerConnected(self, Player):
		Plugin.TryDumpObjToFile("On_PlayerConnected.Player", Player)

	def On_PlayerDisconnected(self, he):
		Plugin.TryDumpObjToFile("On_PlayerDisconnected.Player", Player)