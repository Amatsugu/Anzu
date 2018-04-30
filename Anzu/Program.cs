using System;
using Anzu.Models;
using Nancy.Hosting.Self;

namespace Anzu
{
    class Program
    {
        static void Main(string[] args)
        {
			var host = new NancyHost(new Uri("http://localhost:8765"));
			host.Start();
			Console.WriteLine($"Hosting...");
			AnzuCore.RunCommand(cmd =>
			{
				cmd.CommandText = $"DELETE FROM images; DELETE FROM tags;";
				return cmd.ExecuteNonQuery();
			});
			var foodTag = AnzuCore.AddTag(new TagModel
			{
				Name = "Food",
				Description = "Contains Food"
			});
			var animalTag = AnzuCore.AddTag(new TagModel
			{
				Name = "Animal",
				Description = "Contains an animal"
			});

			var cat = AnzuCore.AddTag(new TagModel
			{
				Name = "Cat",
				Description = "Contains a cat",
				Parent = animalTag.Id
			});

			var dog = AnzuCore.AddTag(new TagModel
			{
				Name = "Dog",
				Description = "Contains a dog",
				Parent = animalTag.Id
			});

			var person = AnzuCore.AddTag(new TagModel
			{
				Name = "Person",
				Description = "Contains a person"
			});

			var boy = AnzuCore.AddTag(new TagModel
			{
				Name = "Boy",
				Description = "Contains a boy",
				Parent = person.Id
			});

			var girl = AnzuCore.AddTag(new TagModel
			{
				Name = "Girl",
				Description = "Contains a girl",
				Parent = person.Id
			});

			var outside = AnzuCore.AddTag(new TagModel
			{
				Name = "Outside",
				Description = "Outdoors"
			});

			var inside = AnzuCore.AddTag(new TagModel
			{
				Name = "Inside",
				Description = "Indoors"
			});

			AnzuCore.AddContradiction(inside, outside);

			AnzuCore.AddImage(new ImageModel
			{
				Name = "Cat pic"
			}).AddTag(cat).AddTag(inside);
			AnzuCore.AddImage(new ImageModel
			{
				Name = "Cat pic 2"
			}).AddTag(cat);

			AnzuCore.AddImage(new ImageModel
			{
				Name = "Cat and dog"
			}).AddTag(cat).AddTag(dog);

			AnzuCore.AddImage(new ImageModel
			{
				Name = "Dog"
			}).AddTag(dog).AddTag(outside);

			AnzuCore.AddImage(new ImageModel
			{
				Name = "Boy with dog"
			}).AddTag(dog).AddTag(boy);

			AnzuCore.AddImage(new ImageModel
			{
				Name = "Girl and boy with dog"
			}).AddTag(dog).AddTag(boy).AddTag(girl);

			Console.WriteLine("Added ");	
			Console.ReadLine();
			host.Dispose();
        }
    }
}
