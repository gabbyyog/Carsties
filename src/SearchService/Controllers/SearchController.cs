using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly DB _db;

        public SearchController(DB db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams search)
        {
            var query = _db.PagedSearch<Item>();

            if (!string.IsNullOrEmpty(search.SearchTerm))
            {
                query.Match(Search.Full, search.SearchTerm).SortByTextScore();
            }
            else
            {
                query.Sort(s => s.Ascending(a => a.Make));
            }

            switch (search.OrderBy)
            {
                case "make":
                    query.Sort(x => x.Ascending(a => a.Make));
                    break;
                case "new":
                    query.Sort(x => x.Descending(a => a.CreatedAt));
                    break;
                default:
                    query.Sort(x => x.Ascending(a => a.AuctionEnd));
                    break;
            }

            switch (search.FilterBy)
            {
                case "finished":
                    query.Match(x => x.AuctionEnd < DateTime.UtcNow);
                    break;
                case "endingSoon":
                    query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow);
                    break;
                default:
                    query.Match(x => x.AuctionEnd > DateTime.UtcNow);
                    break;
            }

            if (!string.IsNullOrEmpty(search.Seller))
            {
                query.Match(x => x.Seller == search.Seller);
            }

            if (!string.IsNullOrEmpty(search.Winner))
            {
                query.Match(x => x.Winner == search.Winner);
            }

            query.PageNumber(search.PageNumber);
            query.PageSize(search.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
}