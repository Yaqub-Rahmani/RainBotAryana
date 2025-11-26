using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IMediaAssetService
    {
        // Okumalar
        IDataResult<List<MediaAsset>> GetAll(Guid? ownerId = null, MediaOwnerType? ownerType = null);
        IDataResult<MediaAsset> GetById(Guid id);
        IDataResult<PagedResult<MediaAssetDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<MediaAssetDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(MediaAsset asset);
        IResult Update(MediaAsset asset);
        IResult Delete(Guid id);

        // Yardımcı
        IResult SetPrimary(Guid id);
    }
}
