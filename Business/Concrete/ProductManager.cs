using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.Aspects.Autofac.Validation;
using Core.Utilities.Paging;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IProductDal _productDal;
        private readonly ICategoryDal _categoryDal;

        public ProductManager(IProductDal productDal, ICategoryDal categoryDal)
        {
            _productDal = productDal;
            _categoryDal = categoryDal;
        }

        [LogAspect]
        [CacheAspect(10)]
        [SecuredOperation("admin")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        [PerformanceAspect(5)]
        public IResult Add(Product product)
        {
            // örnek iş kuralı: isim boş olamaz
            if (string.IsNullOrWhiteSpace(product.Name))
                return new ErrorResult("Product name is required.");
            _productDal.Add(product);
            return new SuccessResult("Product added.");
        }

        [ValidationAspect(typeof(ProductValidator))]
        public IResult Update(Product product)
        {
            if (product.Id <= 0) return new ErrorResult("Invalid Id.");
            _productDal.Update(product);
            return new SuccessResult("Product updated.");
        }

        public IResult Delete(int id)
        {
            var entity = _productDal.Get(p => p.Id == id);
            if (entity == null) return new ErrorResult("Not found.");
            _productDal.Delete(entity);
            return new SuccessResult("Product deleted.");
        }

        public IDataResult<List<Product>> GetAll()
          => new SuccessDataResult<List<Product>>(_productDal.GetList().ToList());

        public IDataResult<Product> GetById(int id)
        {
            var p = _productDal.Get(x => x.Id == id);
            return p == null ? new ErrorDataResult<Product>(null, "Not found.") : new SuccessDataResult<Product>(p);
        }


        [TransactionScopeAspect]
        [ValidationAspect(typeof(ProductValidator))]
        public IResult AddWithCategory(Product product, Category category)
        {
            // 1) önce kategori ekle
            _categoryDal.Add(category);

            // 2) yeni eklenen kategorinin Id'sini ürüne bağla (DB kimliği gerekiyorsa EF set edecektir)
            product.CategoryId = category.Id;

            // 3) ürünü ekle
            _productDal.Add(product);

            // 4) burada oluşacak herhangi bir exception tümünü geri aldırır
            return new SuccessResult("Category + Product added in one transaction.");
        }

        public IDataResult<PagedResult<Product>> GetPaged(PagedRequest req)
        {
            if (req.Page <= 0) req.Page = 1;
            if (req.PageSize <= 0 || req.PageSize > 200) req.PageSize = 10;

            // temel query
            var query = _productDal.GetList().AsQueryable();

            // Search: name contains
            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                var key = req.Search.Trim();
                query = query.Where(p => p.Name.Contains(key));
            }

            // Sort
            // "name" => ASC, "-unitPrice" => DESC
            if (!string.IsNullOrWhiteSpace(req.Sort))
            {
                var s = req.Sort.Trim();
                var desc = s.StartsWith("-");
                var prop = desc ? s[1..] : s;

                query = prop.ToLower() switch
                {
                    "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                    "unitprice" => desc ? query.OrderByDescending(x => x.UnitPrice) : query.OrderBy(x => x.UnitPrice),
                    "unitsinstock" => desc ? query.OrderByDescending(x => x.UnitsInStock) : query.OrderBy(x => x.UnitsInStock),
                    "id" or _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var total = query.Count();

            var items = query
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            var result = new PagedResult<Product>
            {
                Items = items,
                Page = req.Page,
                PageSize = req.PageSize,
                TotalCount = total
            };

            return new SuccessDataResult<PagedResult<Product>>(result);
        }
    }

}
