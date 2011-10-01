
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

namespace SMP
{
	public class CmdGive : Command
	{
		public override string Name { get { return "give"; } }
        public override List<string> Shortcuts { get { return new List<string> {"item", "i"}; } }
        public override string Category { get { return "cheat"; } }
        public override bool ConsoleUseable { get { return true; } }
        public override string Description { get { return "Spawns items."; } }
		public override string PermissionNode { get { return "core.cheat.give"; } }

        public override void Use(Player p, params string[] args)
        {
			if (args.Length == 0 || args[0].ToLower() == "help")
			{
				Help(p);
				return;
			}
			
			//probably an easier way, but couldn't think of it
			
			Player toPlayer = null;
			short itemID = -1;
			byte count = 1;
			short meta = 0;
			
			short s; //doesn't actually do anything important
			
			//first arg
			try
			{
				if (!short.TryParse(args[0], out itemID))
				{
					//itemID = short.Parse(args[0]);
				}
				else if (args[0].Contains(":"))
				{
					itemID = short.Parse(args[0].Substring(0, args[0].IndexOf(":")));
					meta = short.Parse(args[0].Substring(args[0].IndexOf(":") + 1));
				}
				else
				{
					toPlayer = Player.FindPlayer(args[0]);	
				}
			}
			catch
			{
				p.SendMessage(HelpBot + "Something is wrong with your first argument.", WrapMethod.Chat);	
			}
			
			if (args.Length == 1)
			{
				if (toPlayer != null)
				{
					p.SendMessage(HelpBot + "Not enough arguments.");
					Help(p);
					return;
				}
				else
				{		
					SendItem(p, itemID, count, meta);
					return;
				}
			}
			
			//second arg
			try
			{
				if (toPlayer != null)
				{
					if (short.TryParse(args[1], out s))
					{
						itemID = short.Parse(args[1]);
					}
					else if (args[1].Contains(":"))
					{
						itemID = short.Parse(args[1].Substring(0, args[1].IndexOf(":")));
						meta = short.Parse(args[1].Substring(args[1].IndexOf(":") + 1));
					}
					else 
					{
						p.SendMessage(HelpBot + "Something is wrong with your second argument.", WrapMethod.Chat);	
						return;
					}
				}
				else
				{
					count = byte.Parse(args[1]);
				}
			}
			catch
			{
				p.SendMessage(HelpBot + "Something is wrong with your second argument.", WrapMethod.Chat);	
				return;
			}
			
			if (args.Length == 2)
			{
				if (toPlayer != null)
				{
					SendItem(toPlayer, itemID, count, meta);
					p.SendMessage(HelpBot + "Gift Sent");
					return;
				}
				else
				{
					SendItem(p, itemID, count, meta);
					return;
				}
			}
			
			//third arg
			try
			{	
				count = byte.Parse(args[2]);	
			}
			catch
			{
				p.SendMessage(HelpBot + "Third Argument is invalid.");
				return;
			}
			
			SendItem(toPlayer, itemID, count, meta);
			p.SendMessage(HelpBot + "Gift Sent");			
			
		}
		public void SendItem(Player p, short item, byte count, short meta)
		{
            if (FindBlocks.ValidItem(item) && item >= 1)
			{
				p.inventory.Add(item, count, meta);
				p.SendMessage(HelpBot + "Enjoy!");
			}
			else
			{
				p.SendMessage(HelpBot + "Invalid item ID.");
			}
		}

		public override void Help(Player p)
		{
			p.SendMessage("Spawns item(s), and if specified to a player.");
			p.SendMessage("/give (player) <item(:meta)> (amount)");
		}
	}
}

