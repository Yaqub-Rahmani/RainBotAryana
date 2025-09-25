using Business.Abstract;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
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
        public CategoryManager(ICategoryDal categoryDal) { _categoryDal = categoryDal; }

        public IDataResult<List<Category>> GetAll()
            => new SuccessDataResult<List<Category>>(_categoryDal.GetList().ToList());

        public IDataResult<Category> GetById(int id)
        {
            var entity = _categoryDal.Get(x => x.Id == id);
            return entity == null
                ? new ErrorDataResult<Category>(null, "Category not found.")
                : new SuccessDataResult<Category>(entity);
        }

        [ValidationAspect(typeof(CategoryValidator))]
        public IResult Add(Category category)
        {
            // basit benzersizlik kuralı (isteğe bağlı)
            var exists = _categoryDal.Get(x => x.CategoryName == category.CategoryName);
            if (exists != null) return new ErrorResult("Category name already exists.");

            _categoryDal.Add(category);
            return new SuccessResult("Category added.");
        }

        [ValidationAspect(typeof(CategoryValidator))]
        public IResult Update(Category category)
        {
            var current = _categoryDal.Get(x => x.Id == category.Id);
            if (current == null) return new ErrorResult("Category not found.");

            // isim başka bir kayıtta kullanılıyor mu? (isteğe bağlı)
            var nameInUse = _categoryDal.Get(x => x.CategoryName == category.CategoryName && x.Id != category.Id);
            if (nameInUse != null) return new ErrorResult("Category name already exists.");

            _categoryDal.Update(category);
            return new SuccessResult("Category updated.");
        }

        public IResult Delete(int id)
        {
            var entity = _categoryDal.Get(x => x.Id == id);
            if (entity == null) return new ErrorResult("Category not found.");

            _categoryDal.Delete(entity);
            return new SuccessResult("Category deleted.");
        }
    }
}
