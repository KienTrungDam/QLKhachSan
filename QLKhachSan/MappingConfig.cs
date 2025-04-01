using AutoMapper;
using QLKhachSan.Models;
using QLKhachSan.Models.DTO;

namespace QLKhachSan
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Person, PersonDTO>().ReverseMap();
            CreateMap<PersonDTO, Person>().ReverseMap();

        }
    }
}
