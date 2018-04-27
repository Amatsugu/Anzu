using Nancy;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anzu.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule()
		{
			Get("/", p => "Hello World");
		}
	}
}
