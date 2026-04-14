using AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UserDTO, User>();        
        CreateMap<OrderDTO, Order>();
        CreateMap<TicketDTO, Ticket>();
        //CreateMap<List<BookDTO>, List<Book>>();
    }
}