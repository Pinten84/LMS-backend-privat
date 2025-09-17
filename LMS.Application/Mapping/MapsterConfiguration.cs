using LMS.Application.Contracts.DTOs.Activities;
using LMS.Application.Contracts.DTOs.Courses;
using LMS.Application.Contracts.DTOs.Documents;
using LMS.Application.Contracts.DTOs.Modules;
using LMS.Application.Contracts.DTOs;
using LMS.Domain.Entities;
using Mapster;

namespace LMS.Application.Mapping;
public static class MapsterConfiguration
{
            private static bool _configured;
            public static void Configure()
            {
                if (_configured)
                    return;
                _configured = true;
                TypeAdapterConfig<CreateCourseDto, Course>.NewConfig()
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty);
                TypeAdapterConfig<UpdateCourseDto, Course>.NewConfig()
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty);
                TypeAdapterConfig<CreateModuleDto, Module>.NewConfig()
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty);
                TypeAdapterConfig<UpdateModuleDto, Module>.NewConfig()
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty);
                TypeAdapterConfig<CreateActivityDto, Activity>.NewConfig()
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty);
                TypeAdapterConfig<UpdateActivityDto, Activity>.NewConfig()
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty);
                TypeAdapterConfig<CreateDocumentDto, Document>.NewConfig()
                    .Map(dest => dest.Name, src => src.Title)
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty)
                    .Map(dest => dest.LinkedEntityType, src => src.LinkedEntityType)
                    .Map(dest => dest.LinkedEntityId, src => src.LinkedEntityId)
                    .Ignore(dest => dest.UploadedByUser)
                    .Ignore(dest => dest.UploadedByUserId)
                    .Ignore(dest => dest.Timestamp);
                TypeAdapterConfig<UpdateDocumentDto, Document>.NewConfig()
                    .Map(dest => dest.Name, src => src.Title)
                    .Map(dest => dest.Description, src => src.Description ?? string.Empty)
                    .Ignore(dest => dest.UploadedByUser)
                    .Ignore(dest => dest.UploadedByUserId)
                    .Ignore(dest => dest.Timestamp);
                // Read direction (Entity -> DTO)
                TypeAdapterConfig<Document, DocumentDto>.NewConfig()
                    .Map(dest => dest.UploadedByUserName, src => src.UploadedByUser != null ? (src.UploadedByUser.UserName ?? string.Empty) : string.Empty);
                TypeAdapterConfig<Activity, ActivityDto>.NewConfig()
                    .Map(dest => dest.Documents, src => src.Documents);
                TypeAdapterConfig<Module, ModuleDto>.NewConfig()
                    .Map(dest => dest.Activities, src => src.Activities)
                    .Map(dest => dest.Documents, src => src.Documents);
                TypeAdapterConfig<Course, CourseDto>.NewConfig()
                    .Map(dest => dest.TeacherName, src => src.Teacher != null ? (src.Teacher.UserName ?? string.Empty) : string.Empty)
                    .Map(dest => dest.StudentNames, src => src.Students != null ? src.Students.Select(s => s.UserName ?? string.Empty) : new List<string>())
                    .Map(dest => dest.Modules, src => src.Modules)
                    .Map(dest => dest.Documents, src => src.Documents);
            }
}
