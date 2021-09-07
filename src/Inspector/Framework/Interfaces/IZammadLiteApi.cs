using Inspector.Framework.Dtos.Zammad;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Framework.Interfaces
{
    public interface IZammadLiteApi
    {
        [Get("/api/v1/users/search")]
        Task<List<ZammadUser>> SearchUser([Header("Authorization")] string token, string query);

        [Post("/api/v1/users")]
        Task<ZammadUser> CreateUser([Header("Authorization")] string token, [Body(BodySerializationMethod.Serialized)] ZammadUser user);
        
        [Put("/api/v1/users/{id}")]
        Task<ZammadUser> UpdateUser([Header("Authorization")] string token, string id, [Body(BodySerializationMethod.Serialized)] ZammadUser user);

        [Get("/api/v1/groups")]
        Task<List<ZammadGroup>> GetGroups([Header("Authorization")] string token);
    }
}
