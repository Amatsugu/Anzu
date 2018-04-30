using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Anzu.Models;
using Npgsql;

namespace Anzu
{
    class AnzuCore
    {
		public static T RunCommand<T>(Func<NpgsqlCommand, T> func)
		{
			using (var con = new NpgsqlConnection("Host=karuta.luminousvector.com;Username=anzu;Password=;Database=anzu"))
			{
				con.Open();
				using (var cmd = con.CreateCommand())
				{
					return func(cmd);
				}
			}
		}

		internal static TagModel AddTag(TagModel tag) => RunCommand(cmd =>
		{
			tag.Id = Guid.NewGuid();
			cmd.CommandText = $"INSERT INTO tags VALUES ('{tag.Id}', '{Uri.EscapeDataString(tag.Name)}', '{Uri.EscapeDataString(tag.Description)}', {(tag.Parent == default ? "null" : $"'{tag.Parent}'" )})";
			cmd.ExecuteNonQuery();
			return tag;
		});

		public static void AddContradiction(TagModel tag1, TagModel tag2) => AddContradiction(tag1.Id, tag2.Id);

		public static void AddContradiction(Guid tag1, Guid tag2) => RunCommand(cmd =>
		{
			cmd.CommandText = $"INSERT INTO contradictions VALUES('{tag1}', '{tag2}')";
			return cmd.ExecuteNonQuery();
		});

		public static List<ImageModel> Search(string query) => RunCommand(cmd =>
		{
			var tagQueries = query.Split(" ");
			var tags = tagQueries.SelectMany(tagQuery => SearchTags(tagQuery).Select(tag => tag.Id)).Where(tag => tag != default);
			return GetImagesWithTags(tags);
		});

		public static List<ImageModel> GetImagesWithTags(IEnumerable<Guid> tags) => RunCommand(cmd =>
		{
			var images = new List<ImageModel>();
			if (tags.Count() == 0)
				return images;
			var tagQuery = string.Join(" AND ", tags.Select(tag => $"EXISTS(SELECT DISTINCT image_id FROM tagmap WHERE image_id = id AND tag_id = '{tag}')"));
			var tagArray = $"{"'{"}{string.Join(",", tags)}{"}'"}::uuid[]";
			cmd.CommandText = $"SELECT id, name FROM images WHERE (SELECT COUNT(image_id) FROM tagmap WHERE image_id = id AND tag_id = ANY({tagArray})) = array_length({tagArray}, 1)";
			Console.WriteLine(cmd.CommandText);
			using (var reader = cmd.ExecuteReader())
			{
				while(reader.Read())
				{
					images.Add(new ImageModel
					{
						Id = reader.GetGuid(0),
						Name = Uri.UnescapeDataString(reader.GetString(1))
					});
				}
			}
			return images.Distinct().ToList();
		});

		public static ImageModel GetImage(Guid imageId) => RunCommand(cmd =>
		{
			cmd.CommandText = $"SELECT name FROM images WHERE id = '{imageId}'";
			using (var reader = cmd.ExecuteReader())
			{
				if (!reader.HasRows)
					return null;
				return new ImageModel
				{
					Id = imageId,
					Name = Uri.UnescapeDataString(reader.GetString(0)),
				};
			}
		});

		public static ImageModel AddImage(ImageModel image) => RunCommand(cmd =>
		{
			image.Id = Guid.NewGuid();
			cmd.CommandText = $"INSERT INTO images VALUES('{image.Id}', '{Uri.EscapeDataString(image.Name)}', 'FFFF')";
			cmd.ExecuteNonQuery();
			return image;
		});

		public static List<TagModel> SearchTags(string query) => RunCommand(cmd => 
		{
			var tags = new List<TagModel>();
			query = Uri.EscapeDataString(query).Replace("%20", " ");
			if (string.IsNullOrWhiteSpace(query))
				return null;
			cmd.CommandText = $"SELECT id, name, description, parent FROM tags WHERE name LIKE '{query}%'";
			using (var reader = cmd.ExecuteReader())
			{
				while(reader.Read())
					tags.Add(new TagModel
					{
						Id = reader.GetGuid(0),
						Name = Uri.UnescapeDataString(reader.GetString(1)),
						Description = Uri.UnescapeDataString(reader.GetString(2)),
						Parent = reader.IsDBNull(3) ? default : reader.GetGuid(3)
					});
			}
			return tags;
		});

		public static List<TagModel> GetTags(Guid imageId) => RunCommand(cmd =>
		{
			var tags = new List<TagModel>();
			cmd.CommandText = $"SELECT tag_id FROM tagmap WHERE image_id = '{imageId}'";
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
					tags.Add(GetTag(reader.GetGuid(0)));
			}
			return tags;
		});

		public static void AddTag(Guid imageId, Guid tagId) => RunCommand(cmd =>
		{
			cmd.CommandText = $"INSERT INTO tagmap VALUES('{imageId}', '{tagId}')";
			cmd.ExecuteNonQuery();
			return 0;
		});

		public static void CreateTag(TagModel tag) => RunCommand(cmd =>
		{
			cmd.CommandText = $"INSERT INTO tags VALUES('{Guid.NewGuid()}', '{tag.Name}', '{tag.Description}', '{((tag.Parent != default) ? tag.Parent.ToString() : null)}')";
			cmd.ExecuteNonQuery();
			return 0;
		});

		public static List<TagModel> GetContradictions(Guid id) => RunCommand(cmd =>
		{
			var tags = new List<TagModel>();
			cmd.CommandText = $"SELECT tag1_id, tag2_id FROM contradictions WHERE tag1_id = '{id}' OR tag2_id = '{id}'";
			using (var reader = cmd.ExecuteReader())
			{
				while(reader.Read())
				{
					if (reader.GetGuid(0) == id)
						tags.Add(GetTag(reader.GetGuid(1)));
					else
						tags.Add(GetTag(reader.GetGuid(0)));
				}
			}
			return tags;
		});

		public static TagModel GetTag(Guid id) => RunCommand(cmd =>
		{
			cmd.CommandText = $"SELECT id, name, description, parent FROM tags WHERE id = '{id}'";
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
					return new TagModel
					{
						Id = reader.GetGuid(0),
						Name = reader.GetString(1),
						Description = reader.GetString(2),
						Parent = reader.IsDBNull(3) ? default : reader.GetGuid(3)
					};
				else
					return null;
			}
		});
	}
}
