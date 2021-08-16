using Inspector.Framework.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Framework.Interfaces
{
    public interface ITerritorialDivisionAPI
    {
        [Get("/territories/regions")]
        Task<Response<List<Zone>>> GetRegions();

        [Get("/territories/provinces")]
        Task<Response<List<Zone>>> GetProvinces(Zone parameters);

        [Get("/territories/municipalities")]
        Task<Response<List<Zone>>> GetMunicipalities(Zone parameters);

        [Get("/territories/districts")]
        Task<Response<List<Zone>>> GetDistricts(Zone parameters);

        [Get("/territories/hierarchy/{code}")]
        Task<Response<ZoneHierarchy>> GetHierarchy(string code);
    }
}
