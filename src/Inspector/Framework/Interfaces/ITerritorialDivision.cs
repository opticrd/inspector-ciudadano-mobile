using Inspector.Framework.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Framework.Interfaces
{
    public interface ITerritorialDivision
    {
        [Get("/territories/regions")]
        Task<Response<List<Zone>>> GetRegions();

        [Get("/territories/regions/{regionId}/provinces")]
        Task<Response<List<Zone>>> GetRegionProvince(int regionId);

        [Get("/territories/provinces/{provinceId}/municipalities")]
        Task<Response<List<Zone>>> GetProvinceMunicipality(int provinceId);

        [Get("/territories/municipalities/{municipalityId}/districts")]
        Task<Response<List<Zone>>> GetMunicipalityDistrict(int municipalityId);
    }
}
