using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApp8._0.Domain;
using TestApp8._0.Domain.Entities;
using TestApp8._0.Dto;
using TestApp8._0.Helper;
using TestApp8._0.Repository;

namespace TestApp8._0.Service
{
    /// <summary></summary>
    /// <seealso cref="DraftListClient.Services.IDraftListService" />
    public class StudentService : ServiceBase<Student, int>, IStudentService
    {
        private readonly IRepository<Student, int> _repository;
        protected readonly IConfiguration _configuration;
        public readonly IMapper _mapper;
        public StudentService(IMapper mapper, IRepository<Student, int> repository, ILogger<StudentService> logger) : base(repository, logger, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "Id";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name";
        }

        #endregion
    }
}
