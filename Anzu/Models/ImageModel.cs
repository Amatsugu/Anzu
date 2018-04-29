using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace Anzu.Models
{
	public class ImageModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public List<TagModel> Tags
		{
			get
			{
				if (_tags == null)
					return _tags = AnzuCore.GetTags(Id);
				else
					return _tags;
			}
		}

		private List<TagModel> _tags;

		public ImageModel AddTag(Guid tagID)
		{
			AnzuCore.AddTag(Id, tagID);
			return this;
		}

		public ImageModel AddTag(TagModel tag)
		{
			AnzuCore.AddTag(Id, tag.Id);
			return this;
		}

	}
}