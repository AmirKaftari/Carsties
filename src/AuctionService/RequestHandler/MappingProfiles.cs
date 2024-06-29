using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHandler
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item); // Add Item from Auction to AuctionDto
            CreateMap<Item, AuctionDto>(); // Add Item to AuctionDto
            // Add Item from CreateAuctionDto to Auction
            CreateMap<CreateAuctionDto, Auction>().
                ForMember(destinationMember => destinationMember.Item, option => option.MapFrom(source => source));
            CreateMap<CreateAuctionDto, Item>(); // Add CreateAuctionDto to Item

        }
    }
}
