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
		                               Level  header_level,
		                               int    header_position)
		                              : base (header)
		{
			this.note            = note;
			this.header_position = header_position;
			
			// Set TOC style
			/* +------------------+
			   |[] NOTE TITLE     |
			   | > Header H2      |
			   | > Header H2      |
			   |   └→  Header H3  |
			   |   └→  Header H3  |
			   |   └→  Header H3  |
			   | > Header H2      |
			   +------------------+ */
			
			Gtk.Label label = (Gtk.Label)this.Child;
			
			if (header_level == Level.H1) {
				this.Image = new Gtk.Image (GuiUtils.GetIcon ("note", 16));
				label.Markup = "<b>"+ note.Title + "</b>";
			}
			else if (header_level == Level.H2) {
				this.Image = new Gtk.Image (Gtk.Stock.GoForward, Gtk.IconSize.Menu);
			}
			else if (header_level == Level.H3) {
				label.Text = "└→  " + header;
			}
		}

		protected override void OnActivated ()
		{
			if (note == null)
				return;

			// Jump to the header
			Gtk.TextIter header_iter;
			header_iter = this.note.Buffer.GetIterAtOffset (this.header_position);
			note.Window.Editor.ScrollToIter (header_iter, 0.1, true, 0.0, 0.0);
			this.note.Buffer.PlaceCursor (header_iter);
		}

	}/*class TableOfContentMenuItem*/
}
