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
    public class CategoryManager : ICategoryService
    {
        private readonly ICategoryDal _categoryDal;

        public CategoryManager(ICategoryDal categoryDal)
        {
            _categoryDal = categoryDal;
        }

        public IResult Add(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return new ErrorResult("Kategori adı boş olamaz.");

            _categoryDal.Add(category);
            return new SuccessResult("Kategori eklendi.");
        }

        public IResult Delete(int id)
        {
            var entity = _categoryDal.Get(c => c.Id == id);
            if (entity == null) return new ErrorResult("Kategori bulunamadı.");
            _categoryDal.Delete(entity);
            return new SuccessResult("Kategori silindi.");
        }

        public IDataResult<List<Category>> GetAll()
        {
            var data = _categoryDal.GetAll();
            return new SuccessDataResult<List<Category>>(data, "Kategoriler listelendi.");
        }

        public IDataResult<Category> GetById(int id)
        {
            var data = _categoryDal.Get(c => c.Id == id);
            if (data == null) return new ErrorDataResult<Category>(null, "Kategori bulunamadı.");
            return new SuccessDataResult<Category>(data, "Kategori getirildi.");
        }

        public IResult Update(Category category)
        {
            var exists = _categoryDal.Get(c => c.Id == category.Id);
            if (exists == null) return new ErrorResult("Kategori bulunamadı.");
            _categoryDal.Update(category);
            return new SuccessResult("Kategori güncellendi.");
        }
    }
}
