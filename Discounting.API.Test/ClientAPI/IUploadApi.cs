using System.Threading.Tasks;
using Discounting.Entities;
using Refit;

namespace Discounting.Tests.ClientApi
{
    public interface IUploadApi
    {
        [Get("/api/uploads/{id}")]
        Task<Upload> Get(string id);
    }
}