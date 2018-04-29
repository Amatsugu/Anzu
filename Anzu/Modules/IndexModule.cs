using Nancy;
using System;
using System.Collections.Generic;
using System.Text;
using Nancy.ViewEngines.Razor;

namespace Anzu.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule()
		{
			Get("/", _ => View["index.html", new { search = "" }]);
			Get("/s/{query}", p => View["index", new { search = p.query }]);
			Get("/search/{query}", args => Response.AsJson(AnzuCore.Search((string)args.query)));
		}
	}
}
