using Inspector.Framework.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Framework.Interfaces
{
    public interface ICitizenAPI
    {
        [Get("/citizens/{id}/info/basic")]
        Task<Response<Citizen>> GetCitizenBasicInfo(string id);


        [Get("/citizens/{id}/info/birth")]
        Task<Response<Citizen>> GetCitizenBirthInfo(string id);
    }
}
