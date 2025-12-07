using AutoMapper;
using MigrationTestProject.Models;
using MigrationTestProject.Models.MongoDB;

// namespace MigrationTestProject.Mapping
namespace MigrationTestProject.Mapper;

public class MigrationMappingProfile : Profile
{
    public MigrationMappingProfile()
    {
        // ---------------------------
        // AuditLog
        // ---------------------------
        CreateMap<AuditLog, AuditLogDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<AuditLogDocument, AuditLog>();


        // ---------------------------
        // Bicycle
        // ---------------------------
        CreateMap<Bicycle, BicycleDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<BicycleDocument, Bicycle>();


        // ---------------------------
        // Employee
        // ---------------------------
        CreateMap<Employee, EmployeeDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<EmployeeDocument, Employee>();


        // ---------------------------
        // ListOfShift
        // ---------------------------
        CreateMap<ListOfShift, ListOfShiftDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeRefId, opt => opt.Ignore())
            .ForMember(dest => dest.BicycleRefId, opt => opt.Ignore())
            .ForMember(dest => dest.SubstitutedRefId, opt => opt.Ignore()) 
            .ForMember(dest => dest.RouteRefId, opt => opt.Ignore())
            // Convert TimeSpan? -> "hh:mm:ss" or null
            .ForMember(dest => dest.StartTime,
                opt => opt.MapFrom(src => src.StartTime.HasValue ? src.StartTime.Value.ToString(@"hh\:mm\:ss") : null))
            .ForMember(dest => dest.EndTime,
                opt => opt.MapFrom(src => src.EndTime.HasValue ? src.EndTime.Value.ToString(@"hh\:mm\:ss") : null));

        CreateMap<ListOfShiftDocument, ListOfShift>()
            .ForMember(dest => dest.StartTime,
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.StartTime) ? (TimeSpan?)null : TimeSpan.Parse(src.StartTime!)))
            .ForMember(dest => dest.EndTime,
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.EndTime) ? (TimeSpan?)null : TimeSpan.Parse(src.EndTime!)));


        // ---------------------------
        // Route
        // ---------------------------
        CreateMap<Route, RouteDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<RouteDocument, Route>();


        // ---------------------------
        // ShiftPlan
        // ---------------------------
        CreateMap<ShiftPlan, ShiftPlanDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ShiftPlanDocument, ShiftPlan>();


        // ---------------------------
        // Substituted
        // ---------------------------
        CreateMap<Substituted, SubstitutedDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeRefId, opt => opt.Ignore()); // must be mapped via lookup

        CreateMap<SubstitutedDocument, Substituted>();


        // ---------------------------
        // User
        // ---------------------------
        CreateMap<User, UserDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeRefId, opt => opt.Ignore()); // employee ID lookup required

        CreateMap<UserDocument, User>();


        // ---------------------------
        // WorkHoursInMonths
        // ---------------------------
        CreateMap<WorkHoursInMonths, WorkHoursInMonthsDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeRefId, opt => opt.Ignore());

        CreateMap<WorkHoursInMonthsDocument, WorkHoursInMonths>();
    }
}