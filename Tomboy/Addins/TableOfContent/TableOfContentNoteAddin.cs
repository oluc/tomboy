//
//  "Table of content" is a Note addin for Tomboy.
//     It lists Note's table of contents in a menu.
//     Headers are bold/large and bold/huge lines.
//
//  Copyright (C) 2011 Luc Pionchon <pionchon.luc@gmail.com>
//
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public License
//  as published by the Free Software Foundation; either version 2.1
//  of the License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free
//  Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA
//  02111-1307 USA
//
//  Originally based on Tomboy/Addins/Backlinks/*

using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using Gdk;
using Tomboy;

namespace Tomboy.TableOfContent
{
	public enum Level {H1, H2, H3, None}; // H1=title, H2:bold/huge, H3:bold/large
	
	public class TableOfContentNoteAddin : NoteAddin
	{
		Gtk.ImageMenuItem  menu_item;   // TOC menu entry in the plugin menu
		Gtk.Menu           menu;        // TOC submenu, containing the TOC

		bool submenu_built;

		Gtk.TextTag tag_bold, tag_large, tag_huge;

		public override void Initialize () // Called when tomboy starts
		{
			submenu_built = false;
		}

		public override void Shutdown ()
		{
			if (menu      != null)      menu.Hidden    -= OnMenuHidden;
			if (menu_item != null) menu_item.Activated -= OnMenuItemActivated;
		}

		public override void OnNoteOpened ()
		{
			// Build Addin menu item
			menu         = new Gtk.Menu ();
			menu.Hidden += OnMenuHidden;
			menu.Show();

			menu_item            = new Gtk.ImageMenuItem (Catalog.GetString ("Table of content"));
			menu_item.Image      = new Gtk.Image (Gtk.Stock.JumpTo, Gtk.IconSize.Menu);
			menu_item.Submenu    = menu;
			menu_item.Activated += OnMenuItemActivated;
			menu_item.Show ();

			this.AddPluginMenuItem (menu_item);

			// Reacts to key press events
			this.Window.KeyPressEvent += OnKeyPressed;

			// Header tags
			tag_bold  = this.Buffer.TagTable.Lookup ("bold");
			tag_large = this.Buffer.TagTable.Lookup ("size:large");
			tag_huge  = this.Buffer.TagTable.Lookup ("size:huge");
		}

		private void OnMenuItemActivated (object sender, EventArgs args) // TOC menu entry activated
		{
			if (submenu_built == false)
				UpdateMenu ();
			
			if(sender == null) // activated pragramatically
				this.menu.Popup();
		}

		private void OnMenuHidden (object sender, EventArgs args)
		{
			// Force the submenu to rebuild next time it's supposed to show
			submenu_built = false;
		}

		private void UpdateMenu ()
		{
			// Clear out the old list
			foreach (Gtk.MenuItem old_item in menu.Children) {
				menu.Remove (old_item);
			}

			// Build a new list
			foreach (TableOfContentMenuItem item in GetTableOfContentMenuItems ()) {
				item.ShowAll ();
				menu.Append (item);
			}

			// If nothing was found, add an explanatory text
			if (menu.Children.Length == 0) {
				Gtk.MenuItem item = new Gtk.MenuItem (Catalog.GetString (
					"The Table of Content is empty\n\n"               +
					
					"When you set headers, they will show here\n\n"   +
					
					"A header is a complete line formatted with:\n"   +
					"- level 1: bold + huge \t(Ctrl-1)\n"             +
					"- level 2: bold + large \t(Ctrl-2)\n\n"          +
					
					"You can set the style with normal formatting commands, or\n"      +
					"Select one line and type Ctrl-1 (resp. Ctrl-2), or\n"             +
					"On a new line type Ctrl-1 (resp. Ctrl-2) and enter your text\n\n" +
					
					"Open the Table of Content in a popup menu with Ctrl-Alt-1"
					));
					
				//item.Sensitive = false; keep it sensitive, so it is readable.
				item.ShowAll ();
				menu.Append (item);
			}

			submenu_built = true;
		}
		
		private Level RangeLevel (Gtk.TextIter start, Gtk.TextIter end)
		{
			if( hasTagOverRange (tag_bold, start, end))
				if      (hasTagOverRange (tag_huge , start, end)) return Level.H2;
				else if (hasTagOverRange (tag_large, start, end)) return Level.H3;
			return Level.None;
		}

		// Build the menu items
		private TableOfContentMenuItem [] GetTableOfContentMenuItems ()
		{
			List<TableOfContentMenuItem> items = new List<TableOfContentMenuItem> ();

			TableOfContentMenuItem item = null;

			string header = null;
			Level  header_level;
			int    header_position;

			Gtk.TextIter iter, eol;

			//for each line of the buffer,
			//check if the full line has bold and (large or huge) tags
			iter = this.Note.Buffer.StartIter;
			
			while (iter.IsEnd == false) {
				eol = iter;
				eol.ForwardToLineEnd();
				
				header_level = this.RangeLevel (iter, eol);
				
				if (header_level == Level.H2 || header_level == Level.H3) {
					header_position = iter.Offset;
					header = iter.GetText(eol);
					if (items.Count == 0) {
						//It's the first header found,
						//we also insert an entry linked to the Note's Title:
						item = new TableOfContentMenuItem (this.Note, this.Note.Title, Level.H1, 0);
						items.Add (item);
					}
					item = new TableOfContentMenuItem (this.Note, header, header_level, header_position);
					items.Add (item);
				}
				//next line
				iter.ForwardVisibleLine();
			}
			return items.ToArray ();
		}

		//true if tag is set from start to end
		static private bool hasTagOverRange (Gtk.TextTag tag, Gtk.TextIter start, Gtk.TextIter end){
			Gtk.TextIter iter = start;
			bool has = false;
			while (iter.Compare(end) != 0 && (has = iter.HasTag(tag))){
				iter.ForwardChar();
			}
			return has;
		}

		private void OnKeyPressed (object sender, Gtk.KeyPressEventArgs args)
		{
			args.RetVal = false; // not treated
			
			switch (args.Event.Key) {
			
			case Gdk.Key.Key_1: 
					if (args.Event.State == (Gdk.ModifierType.ControlMask | Gdk.ModifierType.Mod1Mask))
					{
						this.OnMenuItemActivated (null, null); // activates TOC menu
						args.RetVal = true;
						return;
					}
					else if (args.Event.State == Gdk.ModifierType.ControlMask)
					{
						this.HeadificationSwitch (Level.H2);
						args.RetVal = true;
						return;
					}
					else
					{ return; }
			break;
			
			case Gdk.Key.Key_2:
					if (args.Event.State == Gdk.ModifierType.ControlMask)
					{
						this.HeadificationSwitch (Level.H3);
						args.RetVal = true;
						return;
					}
			break;
			
			default:
				args.RetVal = false;
				return;
			}
		}/* OnKeyPressed() */
		
		
		private void HeadificationSwitch (Level header_request) 
		{
			// Apply the correct header style ==> switch  H2 <--> H3 <--> text
			
			Gtk.TextIter start, end;
			this.Buffer.GetSelectionBounds (out start, out end);
			
			Level current_header = this.RangeLevel (start, end);
			
			this.Buffer.RemoveAllTags (start, end);//reset all tags
			
			if( current_header == Level.H2 && header_request == Level.H3) //existing vs requested
			{
				this.Buffer.SetActiveTag ("bold");
				this.Buffer.SetActiveTag ("size:large");
			}
			else if( current_header == Level.H3 && header_request == Level.H2) 
			{
				this.Buffer.SetActiveTag ("bold");
				this.Buffer.SetActiveTag ("size:huge");
			}
			else if( current_header == Level.None)
			{
				this.Buffer.SetActiveTag ("bold");
				this.Buffer.SetActiveTag ( (header_request == Level.H2)?"size:huge":"size:large");
			}
			else {/*nothing*/}
			
		}/* HeadificationSwitch() */
		
		
	}/*class TableOfContentNoteAddin*/
}
