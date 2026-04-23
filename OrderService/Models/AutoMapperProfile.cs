using AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UserDTO, User>();        
        CreateMap<OrderDTO, Order>();
        CreateMap<TicketDTO, Ticket>();
        CreateMap<BasketTicket, Ticket>()
            .ForMember(dest => dest.TicketGuid, opt => opt.MapFrom(src => src.Id));
        //CreateMap<List<BookDTO>, List<Book>>();
    }
}