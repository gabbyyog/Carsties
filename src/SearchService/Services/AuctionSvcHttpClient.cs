using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionSvcHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly DB _db;

        public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config, DB db)
        {
            _httpClient = httpClient;
            _config = config;
            _db = db;
        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
            var lastUpdatedDate = await _db.Find<Item, DateTime>()
                .Sort(x => x.Descending(x => x.UpdatedAt))
                .Project(x => x.UpdatedAt)
                .ExecuteFirstAsync();

            var url = _config["AuctionServiceUrl"] + "/api/auctions";

            if (lastUpdatedDate != default)
            {
                url += $"?date={lastUpdatedDate:O}";
            }

            Console.WriteLine($"Calling: {url}");

            return await _httpClient.GetFromJsonAsync<List<Item>>(url) ?? new List<Item>();
        }
    }
}