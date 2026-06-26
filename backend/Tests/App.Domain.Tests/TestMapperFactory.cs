using App.Infrastructure.Mapping;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace App.Domain.Tests;

public static class TestMapperFactory
{
    public static IMapper Create()
    {
        MapperConfiguration configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        return configuration.CreateMapper();
    }
}
