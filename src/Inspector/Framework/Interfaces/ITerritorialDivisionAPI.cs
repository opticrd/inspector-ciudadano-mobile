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
        Task<ResponseZone> GetRegions();

        [Get("/territories/provinces")]
        Task<ResponseZone> GetProvinces(QueryZone parameters);

        [Get("/territories/municipalities")]
        Task<ResponseZone> GetMunicipalities(QueryZone parameters);

        [Get("/territories/districts")]
        Task<ResponseZone> GetDistricts(QueryZone parameters);

        [Get("/territories/sections")]
        Task<ResponseZone> GetSections(QueryZone parameters);

        [Get("/territories/neighborhoods")]
        Task<ResponseZone> GetNeighborhoods(QueryZone parameters);

        [Get("/territories/sub-neighborhoods")]
        Task<ResponseZone> GetSubNeighborhoods(QueryZone parameters);

        [Get("/territories/hierarchy/{code}")]
        Task<Response<ZoneHierarchy>> GetHierarchy(string code);
    }
}
