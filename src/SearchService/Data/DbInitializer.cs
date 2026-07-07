using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(DB db, WebApplication app)
        {
            await db.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            var count = await db.CountAsync<Item>();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

            var item = await httpClient.GetItemsForSearchDb();

            Console.WriteLine(item.Count + " returned from the auction service");

            if (item.Count > 0) await db.SaveAsync(item);
        }
    }
}