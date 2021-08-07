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

        [Get("/territories/regions/{regionId}/provinces")]
        Task<Response<List<Zone>>> GetRegionProvince(string regionId);

        [Get("/territories/regions/{regionId}/provinces/{provinceId}/municipalities")]
        Task<Response<List<Zone>>> GetProvinceMunicipality(string regionId, string provinceId);

        [Get("/territories/regions/{regionId}/provinces/{provinceId}/municipalities/{municipalityId}/districts")]
        Task<Response<List<Zone>>> GetMunicipalityDistrict(string regionId, string provinceId, string municipalityId);
    }
}
