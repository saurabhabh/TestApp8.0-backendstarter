using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApp8._0.Domain.Entities;
using TestApp8._0.Dto;

namespace TestApp8._0.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
              
               CreateMap<StudentDto, Student>();
               CreateMap<Student, StudentDto>();
          
        }
    }
}
