using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos.Employee;

namespace Business.Abstract
{
    public interface IEmployeeService
    {
        IDataResult<Employee> GetById(Guid id);
        IDataResult<PagedResult<EmployeeDetailDto>> GetDetails(QueryOptions opt, bool? active = null);
        IDataResult<EmployeeProfileDto> GetProfile(Guid id);

        IResult Add(Employee employee);
        IResult Update(Employee employee);
        IResult Delete(Guid id);

        IResult Activate(Guid id);
        IResult Deactivate(Guid id);

        IDataResult<EmployeeWorkInfoDto> GetWorkInfo(Guid employeeId);
        IResult UpsertWorkInfo(Guid employeeId, EmployeeWorkInfoUpsertDto dto, int userId);

        /// <summary>
        /// Upsert employee fingerprint template (Biometric table) and optionally link to bitmap MediaAsset.
        /// </summary>
        Task<IResult> UpsertFingerprintAsync(Guid employeeId, EmployeeFingerprintUpsertDto dto);

        /// <summary>
        /// Employee salary payments (Expense.Category=Salary) + linked MoneyEntry info.
        /// </summary>
        IDataResult<PagedResult<EmployeeSalaryPaymentListItemDto>> GetSalaryPayments(Guid employeeId, QueryOptions opt);
    }
}
