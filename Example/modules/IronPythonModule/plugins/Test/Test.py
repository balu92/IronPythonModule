import clr
import sys
clr.AddReferenceByPartialName("UnityEngine")
clr.AddReferenceByPartialName("Fougerite")
import UnityEngine
import Fougerite

class Test:
	def GetPlugin(self, plugin): # not yet done
		plugin.Name = "TestPluginName"
		plugin.Author = "balu92"
		plugin.Version = "1.0.0.0"
		return plugin

	def On_TablesLoaded(self, tables):
		UnityEngine.Debug.Log("OnTablesLoaded hooked with " + tables.Count.ToString() + " element, from Python")
		return tables

	def On_AllPluginsLoaded(self):
		UnityEngine.Debug.Log("The knights who say NI! I mean HI!")

	def On_ServerInit(self):
		UnityEngine.Debug.Log("OnServerInit hooked from Python")
		dic = Plugin.CreateDict()
		dic.Add("key", "value")
		Plugin.CreateTimer("testtimer", 5000, dic).Start()

	def testtimerCallback(self, dic):
		plug2 = Plugin.GetPlugin("Test2")
		plug2.Invoke("TestSharedFunction", "Hello", "world!")
		UnityEngine.Debug.Log(dic["key"])

	def On_PluginInit(self):
		UnityEngine.Debug.Log("ONPluginInit hooked from Python")

	def On_ServerShutdown(self):
		UnityEngine.Debug.Log("OnServerShutdown hooked from Python")

#	def OnItemsLoaded(self, items):
#		UnityEngine.Debug.Log("OnItemsLoaded hooked with " + items.Count + " element, from Python")
#		return items ## not sure if you need to return the 'items' here
	def On_Chat(self, Player, Text):
		UnityEngine.Debug.Log(Player.Name + " says: " + Text)

	def On_BlueprintUse(self, Player, BPUseEvent):
		UnityEngine.Debug.Log(Player.Name + " researched " + BPUseEvent.ItemName)

	def On_Command(self, Player, cmd, args):
		UnityEngine.Debug.Log("On_Command(" + Player.Name + ", " + cmd + ", " + args + ")")

	def On_Console(self, Player, Arg):
		UnityEngine.Debug.Log(Player.Name + " used " + Arg.Class + "." + Arg.Function + " in console ")

	def On_DoorUse(self, Player, DoorEvent):
		UnityEngine.Debug.Log(Player.Name + " tried to use a door")
		UnityEngine.Debug.Log("Succeded? " + ("yes" if DoorEvent.Open else "no"))

#	def On_EntityDecay(self, DecayEvent):
#		return DecayEvent.DamageAmount
	def On_EntityDeployed(self, Player, Enity):
		UnityEngine.Debug.Log(Player.Name + " deployed a(n) " + Entity.Name + " @ " + Player.Location.ToString())