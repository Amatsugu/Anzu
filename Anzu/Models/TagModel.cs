using System;
using System.Collections.Generic;
using System.Text;

namespace Anzu.Models
{
    public class TagModel
    {
		public Guid Id { get; set; }
		public Guid Parent { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public List<TagModel> Contradictions
		{
			get
			{
				if (_contradictions == null)
					return _contradictions = AnzuCore.GetContradictions(Id);
				else
					return _contradictions;
			}
		}


		private List<TagModel> _contradictions;
	}
}
