using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Dtos.Dashboard;
using System;
using System.Collections.Generic;

namespace Business.Abstract
{
    public interface IDashboardService
    {
        IDataResult<FinanceDashboardDto> GetFinanceDashboard(DateTime? fromUtc = null, DateTime? toUtc = null, int take = 20);

        // ? Charts (separate section)
        IDataResult<DashboardChartsDto> GetCharts(DateTime? fromUtc = null, DateTime? toUtc = null, int months = 3);

        // ? Server-side tables (CustomTableV2 legacy)
        IDataResult<PagedResult<MoneyAccountBalanceTableRowDto>> GetMoneyAccountsDetails(QueryOptions opt);
        IDataResult<PagedResult<ReserveAccountBalanceTableRowDto>> GetReserveAccountsDetails(QueryOptions opt);

        IDataResult<PagedResult<PoolPartyBalanceRowDto>> GetFamilyPoolDetails(QueryOptions opt);
        IDataResult<PagedResult<PoolPartyBalanceRowDto>> GetFranchisePoolDetails(QueryOptions opt);

        IDataResult<PagedResult<CollectionDashboardRowDto>> GetCollectionsDetails(QueryOptions opt, bool? isApproved = null);
        IDataResult<PagedResult<PeriodDebtRowDto>> GetPeriodDebtsDetails(QueryOptions opt);
        IDataResult<PagedResult<DashboardEntryDto>> GetEntriesDetails(QueryOptions opt, DashboardEntrySource source, Guid? sourceId, DateTime? fromUtc = null, DateTime? toUtc = null);

        //IDataResult<PagedResult<DashboardEntryDto>> GetEntriesDetails(QueryOptions opt, DashboardEntrySource source, Guid? sourceId, DateTime? fromUtc, DateTime? toUtc, int? periodYear, int? periodMonth);


    }
}
