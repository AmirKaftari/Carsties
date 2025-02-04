﻿using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;
using System.Text.Json;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDb", MongoClientSettings
                .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            var count = await DB.CountAsync<Item>();

            /* if (count == 0)
             {
                 Console.WriteLine("No data - will attemp to seed");
                 var itemData = await File.ReadAllTextAsync("Data/auctions.json");

                 // Insensitive for upper and smaller name in json file
                 var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                 // Change json data to list data
                 var items = JsonSerializer.Deserialize<List<Item>>(itemData, option);

                 // Add items to mongodb
                 await DB.SaveAsync(items);
             }*/

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSrvHttpClient>();

            var items = await httpClient.GetItemsForSearchDb();

            Console.WriteLine(items.Count + "returned from auction service");

            if (items.Count > 0) await DB.SaveAsync(items);
        }
    }
}
