﻿using System;
using System.Collections.Generic;

using sar.Tools;

namespace sar.CNC
{
	public class GrblCommand
	{
		private string responce = "";
		private bool sent = false;
		private bool buffered = false;
		private bool completed = false;
		private bool active = false;
		
		
		private DateTime queuedTimestamp;
		private DateTime sentTimestamp;
		
		public int ID { get; private set; }
		public string Comment { get; private set; }
		public string Command { get; private set; }
		public bool Active { get; set; }
		public string Responce
		{
			get { return responce; }
			set
			{
				this.Sent = true;
				responce = value;
				sentTimestamp = DateTime.Now;
				//Logger.LogRaw(this.ToJSON());
			}
		}
		
		public bool Sent = false;
		public bool Buffered
		{
			get { return this.buffered; }
			set { this.buffered = value; }
		}
		
		public bool Completed
		{
			get { return this.completed; }
			set { this.completed = value; }
		}
		
		public Dictionary<string, object> NamedParameters
		{
			get
			{
				var commandJSON = new Dictionary<string, object>();
				commandJSON.Add("id", this.ID);
				commandJSON.Add("command", this.Command);
				commandJSON.Add("sent", this.Sent);
				commandJSON.Add("buffered", this.Buffered);
				commandJSON.Add("complete", this.Completed);
				commandJSON.Add("responce", this.responce);
				commandJSON.Add("comment", this.Comment);
				
				return commandJSON;
			}
		}
		
		public GrblCommand(int id, string command, string comment)
		{
			this.ID = id;
			this.Command = command;
			this.queuedTimestamp = DateTime.Now;
			this.Comment = comment;
		}
	}
}
