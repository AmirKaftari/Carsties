using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuctionDbContext _context;
        public AuctionsController(AuctionDbContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if(!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            /*var auctions = await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();*/

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

            //return _mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto); // Use AutoMapper for Map CreateAuctionDto to Auction
            auction.Seller = "Test";
            auction.Winner = "Abbas";

            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if(!result)
            {
                return BadRequest("Could not save this data!");
            }

            return CreatedAtAction(
                nameof(GetAuctionById), // Call GetAuctionById route after create successfully.
                new { auction.Id }, // parameter for route
                _mapper.Map<AuctionDto>(auction) // response to user
                );


        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            Console.WriteLine($"Received data: Make={updateAuctionDto.Make}, Model={updateAuctionDto.Model}, Year={updateAuctionDto.Year}, Color={updateAuctionDto.Color}, Mileage={updateAuctionDto.Mileage}");


            if (auction == null)
            {
                return NotFound();
            }

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                return Ok();
            }

            return BadRequest("Could not update data!");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) { return NotFound();}

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Could not remove this auction");
            }

            return Ok();
        }
    }
}
