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
using Tomboy;

namespace Tomboy.TableOfContent
{
	public class TableOfContentNoteAddin : NoteAddin
	{
		Gtk.Menu           menu;
		Gtk.ImageMenuItem  menu_item;

		bool submenu_built;

		public override void Initialize ()
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
			menu = new Gtk.Menu ();
			menu.Hidden += OnMenuHidden;
			menu.ShowAll ();

			menu_item = new Gtk.ImageMenuItem (Catalog.GetString ("Table of content"));
			menu_item.Image = new Gtk.Image (Gtk.Stock.JumpTo, Gtk.IconSize.Menu);
			menu_item.Submenu = menu;
			menu_item.Activated += OnMenuItemActivated;
			menu_item.Show ();

			AddPluginMenuItem (menu_item); //FIXME: to do once on initialize(). only submenu needs repopulate
		}

		void OnMenuItemActivated (object sender, EventArgs args)
		{
			if (submenu_built == true)
				return; // submenu already built.  do nothing.

			UpdateMenu ();
		}

		void OnMenuHidden (object sender, EventArgs args)
		{
			// Force the submenu to rebuild next time it's supposed to show
			submenu_built = false;
		}

		void UpdateMenu ()
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
					"Headers are lines formatted in 'bold',\n"        +
					"whith 'large' or 'huge' font size.\n"));
				item.Sensitive = false;
				item.ShowAll ();
				menu.Append (item);
			}

			submenu_built = true;
		}

		TableOfContentMenuItem [] GetTableOfContentMenuItems ()
		{
			List<TableOfContentMenuItem> items = new List<TableOfContentMenuItem> ();

			TableOfContentMenuItem item = null;

			string header = null;
			int    header_level;
			int    header_position;

			Gtk.TextIter iter = this.Note.Buffer.StartIter;
			Gtk.TextIter eol;
			Gtk.TextTag bold  = this.Note.Buffer.TagTable.Lookup ("bold");
			Gtk.TextTag large = this.Note.Buffer.TagTable.Lookup ("size:large");
			Gtk.TextTag huge  = this.Note.Buffer.TagTable.Lookup ("size:huge");

			//for each line,
			//check if the full line has bold and (large or huge) tags
			header_level = -1;
			while (iter.IsEnd != true) {
				eol = iter;
				eol.ForwardToLineEnd();

				if (hasTagOverRange (bold, iter, eol)) {
					if (hasTagOverRange (large, iter, eol)) {
						header_level = 3;
					}
					else if (hasTagOverRange (huge, iter, eol)) {
						header_level = 2;
					}
				}
				if (header_level == 2 || header_level == 3) {
					header_position = iter.Offset;
					header = iter.GetText(eol);
					if (header_level == 3) header = "└→  " + header;
					if (items.Count == 0) {
						//It's the first header found,
						//we also insert an entry linked to the Note's Title:
						item = new TableOfContentMenuItem (this.Note, this.Note.Title, 2, 0);
						items.Add (item);
					}
					item = new TableOfContentMenuItem (this.Note, header, header_level, header_position);
					items.Add (item);
				}
				//next line
				header_level = -1;
				iter.ForwardVisibleLine();
			}
			return items.ToArray ();
		}

		//true if tag is set from start to end
		static bool hasTagOverRange (Gtk.TextTag tag, Gtk.TextIter start, Gtk.TextIter end){
			Gtk.TextIter iter = start;
			bool has = false;
			while (iter.Compare(end) != 0 && (has = iter.HasTag(tag))){
				iter.ForwardChar();
			}
			return has;
		}
	}/*class TableOfContentNoteAddin*/
}
