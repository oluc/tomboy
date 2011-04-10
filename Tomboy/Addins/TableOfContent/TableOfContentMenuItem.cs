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
using Gtk;
using Tomboy;

namespace Tomboy.TableOfContent
{
	public class TableOfContentMenuItem : Gtk.ImageMenuItem
	{
		Note note;
		int  header_position;

		public TableOfContentMenuItem (Note   note,
		                               string header,
		                               int    header_level,
		                               int    header_position)
		                              : base (header)
		{
			this.note            = note;
			this.header_position = header_position;

			//set MenuItem's icon
			if (header_position == 0) {
				this.Image = new Gtk.Image (GuiUtils.GetIcon ("note", 16));
			}
			else if (header_level == 2) {
				this.Image = new Gtk.Image (Gtk.Stock.GoForward, Gtk.IconSize.Menu);
			}
		}

		protected override void OnActivated ()
		{
			if (note == null)
				return;

			NoteBuffer buffer = note.Buffer; //a GtkTextBuffer subclass
			Gtk.TextIter header_iter = buffer.GetIterAtOffset (this.header_position);
			Gtk.TextView editor = note.Window.Editor;
			editor.ScrollToIter (header_iter, 0.1, true, 0.0, 0.0);
			//TODO: possibly move the cursor too (?)
			//TODO: possibly highlight the header (?)
			//**/Console.WriteLine (this.header_position);
		}

	}/*class TableOfContentMenuItem*/
}
