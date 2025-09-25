using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
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

        public ProductManager(IProductDal productDal)
        {
            _productDal = productDal;
        }

        public IResult Add(Product product)
        {
            // Basit örnek kural: isim boş olamaz
            if (string.IsNullOrWhiteSpace(product.Name))
                return new ErrorResult("Ürün adı boş olamaz.");

            _productDal.Add(product);
            return new SuccessResult("Ürün eklendi.");
        }

        public IResult Delete(int id)
        {
            var entity = _productDal.Get(p => p.Id == id);
            if (entity == null) return new ErrorResult("Ürün bulunamadı.");
            _productDal.Delete(entity);
            return new SuccessResult("Ürün silindi.");
        }

        public IDataResult<List<Product>> GetAll()
        {
            var data = _productDal.GetAll();
            return new SuccessDataResult<List<Product>>(data, "Ürünler listelendi.");
        }

        public IDataResult<Product> GetById(int id)
        {
            var data = _productDal.Get(p => p.Id == id);
            if (data == null) return new ErrorDataResult<Product>(null, "Ürün bulunamadı.");
            return new SuccessDataResult<Product>(data, "Ürün getirildi.");
        }

        public IResult Update(Product product)
        {
            var exists = _productDal.Get(p => p.Id == product.Id);
            if (exists == null) return new ErrorResult("Ürün bulunamadı.");
            _productDal.Update(product);
            return new SuccessResult("Ürün güncellendi.");
        }
    }
}
