using AutoMapper;
using MRT.Contract.DTOs.User;
using MRT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Infractructure.Mappers.UserProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDTO, ApplicationUser>().ReverseMap();
            CreateMap<CreateUserDTO, ApplicationUser>().ReverseMap();
        }
    }
}
