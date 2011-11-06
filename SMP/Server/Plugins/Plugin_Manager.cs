/*
	Copyright 2011 ForgeCraft team
	
	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at
	
	http://www.opensource.org/licenses/ecl2.php
	http://www.gnu.org/licenses/gpl-3.0.html
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace SMP
{
    public abstract partial class Plugin
    {
        public static List<Plugin> all = new List<Plugin>();
        public abstract void Load(bool startup);
        public abstract void Unload(bool shutdown);
        public abstract string name { get; }
        public abstract string website { get; }
        public abstract Version Version { get; }
        public abstract Version ForgeCraft_Version { get; }
        public abstract string welcome { get; }
	    public abstract string creator { get; }
	    public abstract bool LoadAtStartup { get; }
        public abstract void Help(Player p);
		/// <summary>
		/// Find a Plugin with a name
		/// </summary>
		/// <param name='name'>
		/// Name. The name of the plugin
		/// </param>
        public static Plugin Find(string name)
        {
            List<Plugin> tempList = new List<Plugin>();
            tempList.AddRange(all);
            Plugin tempPlayer = null; bool returnNull = false;

            foreach (Plugin p in tempList)
            {
                if (p.name.ToLower() == name.ToLower()) return p;
                if (p.name.ToLower().IndexOf(name.ToLower()) != -1)
                {
                    if (tempPlayer == null) tempPlayer = p;
                    else returnNull = true;

                }
            }

            if (returnNull == true) return null;
            if (tempPlayer != null) return tempPlayer;
            return null;
        }
		/// <summary>
		/// Load a Plugin
		/// </summary>
		/// <param name='pluginname'>
		/// Pluginname. The path to the plugin dll
		/// </param>
		/// <param name='startup'>
		/// Startup. Is it server startup?
		/// </param>
        public static void Load(string pluginname, bool startup)
        {
	    String creator = "";
            try
            {
                object instance = Activator.CreateInstance(Assembly.LoadFrom(pluginname).GetTypes()[0]);
                if (((Plugin)instance).ForgeCraft_Version > Server.version)
                {
                    Server.Log("This plugin (" + ((Plugin)instance).name + ") isnt compatible with this version of ForgeCraft!");
                    Thread.Sleep(1000);
                    if (Server.unsafe_plugin)
                    {
                        Server.Log("Will attempt to load!");
                        goto here;
                    }
                    else
                        return;
                }
                here:
                Plugin.all.Add((Plugin)instance);
		        creator = ((Plugin)instance).creator;
	        	if (((Plugin)instance).LoadAtStartup)
				{
                    ((Plugin)instance).Load(startup);
                    Server.Log("Plugin: " + ((Plugin)instance).name + " version " + ((Plugin)instance).Version.ToString() + " loaded.");
				}
				else
		    		Server.Log("Plugin: " + ((Plugin)instance).name + " was not loaded, you can load it with /pload");
                	Server.Log(((Plugin)instance).welcome);
            }
            catch (FileNotFoundException)
            {
                //Server.ErrorLog(e);
            }
            catch (BadImageFormatException)
            {
                //Server.ErrorLog(e);
            }
            catch (PathTooLongException)
            {
            }
            catch (FileLoadException)
            {
                //Server.ErrorLog(e);
            }
            catch (Exception)
            {
                //Server.ErrorLog(e);
				Server.Log("The plugin " + pluginname + " failed to load!");
				if (creator != "")
					Server.Log("You can go bug " + creator + " about it");
				Thread.Sleep(1000);
            }
        }
		/// <summary>
		/// Unload the specified p and shutdown.
		/// </summary>
		/// <param name='p'>
		/// P. The plugin object you want to unload
		/// </param>
		/// <param name='shutdown'>
		/// Shutdown. Is the server shutting down?
		/// </param>
        public static void Unload(Plugin p, bool shutdown)
        {
            p.Unload(shutdown);
            all.Remove(p);
            Server.Log(p.name + " was unloaded...how ever you cant re-load it until you restart!");
        }
		/// <summary>
		/// Unload all plugins.
		/// </summary>
		public static void Unload()
		{
			all.ForEach(delegate(Plugin p)
			{
				Unload(p, true);
			});
		}
		/// <summary>
		/// Load all plugins.
		/// </summary>
        public static void Load()
        {
            if (Directory.Exists("plugins"))
            {
                foreach (string file in Directory.GetFiles("plugins", "*.dll"))
                {
                    Load(file, true);
                }
            }
        }
    }
}