using AutoMapper;
using System.Text.Json;
using TypeAuth.AspNetCore.Sample.Server.ActionTreeModels;
using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Shared.ActionTreeDtos;
using TypeAuth.AspNetCore.Sample.Shared.RoleDtos;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<Role, RoleDto>();
            CreateMap<CRMActionModel, CRMActionDto>()
                .ForMember(x=> x.Sales,s=> s.MapFrom(x=> ConvertArrayToReadWriteDeledteActionDto(x.Sales)));
            CreateMap<Role, RoleModel>()
                .ForMember(x => x.AccessTree, s => s.MapFrom(x => JsonSerializer.Deserialize<ActionTreeModel>(x.AccessTree,
                new JsonSerializerOptions())));
            CreateMap<RoleModel, RoleDto>();
            CreateMap<ActionTreeModel, ActionTreeDto>();


            CreateMap<CRMActionDto, CRMActionModel>()
                .ForMember(x => x.Sales, s => s.MapFrom(x => ConvertReadWriteDelecteActionDtoToArray(x.Sales)));
            CreateMap<UpdateRoleDto, UpdateRoleModel>();
            CreateMap<UpdateRoleModel, Role>()
                .ForMember(x => x.AccessTree, s => s.MapFrom(x => JsonSerializer.Serialize(x.AccessTree,
                new JsonSerializerOptions())));
            CreateMap<ActionTreeDto, ActionTreeModel>();
        }

        private int[] ConvertReadWriteDelecteActionDtoToArray(ReadWriteDeleteActionDto action)
        {

            List<int> t = new();

            if (action.Read)
                t.Add(1);

            if(action.Write)
                t.Add(2);

            if(action.Delete)
                t.Add(3);

            return t.ToArray();
        }

        private ReadWriteDeleteActionDto ConvertArrayToReadWriteDeledteActionDto(int[] values)
        {
            ReadWriteDeleteActionDto action = new ReadWriteDeleteActionDto();

            if(values.Contains(1))
                action.Read = true;

            if(values.Contains(2))
                action.Write = true;

            if(values.Contains(3))
                action.Delete = true;

            return action;
        }
    }
}
