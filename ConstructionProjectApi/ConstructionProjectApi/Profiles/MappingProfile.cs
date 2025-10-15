using AutoMapper;
using ConstructionProjectApi.Models;
using ConstructionProjectApi.DTOs;

namespace ConstructionProjectApi.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ConstructionProject, ConstructionProjectDto>();
            CreateMap<ConstructionProjectDto, ConstructionProject>();
            CreateMap<ProjectTask, ProjectTaskDto>();
            CreateMap<ProjectTaskDto, ProjectTask>();
            CreateMap<Employee, EmployeeDto>();
            CreateMap<EmployeeDto, Employee>();
            CreateMap<Resource, ResourceDto>();
            CreateMap<ResourceDto, Resource>();
            CreateMap<ResourceUsage, ResourceUsageDto>();
            CreateMap<ResourceUsageDto, ResourceUsage>();
        }
    }
}
