using Business.Abstract;
using Core.Utilities.Query;
using Entities.Dtos.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        // =========================
        // Summary (existing)
        // =========================
        [HttpGet("finance")]
        public IActionResult GetFinance([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int take = 20)
        {
            var r = _dashboard.GetFinanceDashboard(fromUtc, toUtc, take);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        // =========================
        // Charts (separate section)
        // =========================
        [HttpGet("charts")]
        public IActionResult GetCharts([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int months = 3)
        {
            var r = _dashboard.GetCharts(fromUtc, toUtc, months);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        // =========================
        // Server-side tables (CustomTableV2 legacy)
        // =========================
        [HttpGet("money-accounts/details")]
        public IActionResult MoneyAccountsDetails([FromQuery] QueryOptions opt)
        {
            var r = _dashboard.GetMoneyAccountsDetails(opt);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        [HttpGet("reserve-accounts/details")]
        public IActionResult ReserveAccountsDetails([FromQuery] QueryOptions opt)
        {
            var r = _dashboard.GetReserveAccountsDetails(opt);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        [HttpGet("family-pool/details")]
        public IActionResult FamilyPoolDetails([FromQuery] QueryOptions opt)
        {
            var r = _dashboard.GetFamilyPoolDetails(opt);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        [HttpGet("franchise-pool/details")]
        public IActionResult FranchisePoolDetails([FromQuery] QueryOptions opt)
        {
            var r = _dashboard.GetFranchisePoolDetails(opt);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        /// <summary>
        /// Paid but not approved / approved payments
        /// isApproved=true|false|null (null => all)
        /// </summary>
        [HttpGet("collections/details")]
        public IActionResult CollectionsDetails([FromQuery] QueryOptions opt, [FromQuery] bool? isApproved)
        {
            var r = _dashboard.GetCollectionsDetails(opt, isApproved);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        [HttpGet("period-debts/details")]
        public IActionResult PeriodDebtsDetails([FromQuery] QueryOptions opt)
        {
            var r = _dashboard.GetPeriodDebtsDetails(opt);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        // GET: /api/Dashboard/entries/details
        [HttpGet("entries/details")]
        public IActionResult EntriesDetails(
            [FromQuery] QueryOptions opt,
            [FromQuery] DashboardEntrySource source,
            [FromQuery] Guid? sourceId,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc)
        {
            var r = _dashboard.GetEntriesDetails(opt, source, sourceId, fromUtc, toUtc);
            if (!r.Success) return BadRequest(r.Message);
            return Ok(r.Data);
        }

        //[HttpGet("entries/details")]
        //public IActionResult EntriesDetails(
        //       [FromQuery] QueryOptions opt,
        //       [FromQuery] DashboardEntrySource source,
        //       [FromQuery] Guid? sourceId,
        //       [FromQuery] DateTime? fromUtc,
        //       [FromQuery] DateTime? toUtc,
        //       [FromQuery] int? periodYear,
        //       [FromQuery] int? periodMonth
        //    )
        //            {
        //                var r = _dashboard.GetEntriesDetails(opt, source, sourceId, fromUtc, toUtc, periodYear, periodMonth);
        //                if (!r.Success) return BadRequest(r.Message);
        //                return Ok(r.Data);
        //            }

    }
}
