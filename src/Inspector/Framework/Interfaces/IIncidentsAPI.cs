using System;
using System.Collections.Generic;
using Refit;
using Inspector.Framework.Dtos;
using System.Threading.Tasks;

namespace Inspector.Framework.Interfaces
{
    public interface IIncidentsAPI
    {
        [Get("/incidents/types")]
        Task<Response<List<Incident>>> GetIncidentTypes();

        [Get("/incidents/types/{typeId}/categories")]
        Task<Response<List<Incident>>> GetCategories(int typeId);

        [Get("/incidents/types/{typeId}/categories/{categoryId}/subcategories")]
        Task<Response<List<Incident>>> GetSubCategories(int typeId, int categoryId);
    }
}
