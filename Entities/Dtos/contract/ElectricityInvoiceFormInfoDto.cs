using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.contract
{
    public class ElectricityInvoiceFormInfoDto
    {
        public Guid ContractId { get; set; }
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }

        // Kaynak bilgisi
        public string Source { get; set; } = "";    // "Contract" veya "MeterReading"

        // Bugünün Persian tarihi (Kabul)
        public int TodayYear { get; set; } = 0;
        public int TodayMonth { get; set; } = 0;
        public int TodayDay { get; set; } = 0;

        // Yeni fatura kaydı için önerilen dönem (default: bugün)
        public int PeriodYear { get; set; } = 0;
        public int PeriodMonth { get; set; } = 0;

        // Yeni kayıtta PreviousIndex olarak önerilen değer
        public decimal SuggestedPreviousIndex { get; set; } = 0m;
        public bool HasSuggestedPreviousIndex { get; set; } = false;

        // Son sayaç kaydı bilgisi
        public bool HasLastReading { get; set; } = false;
        public int LastReadingYear { get; set; } = 0;
        public int LastReadingMonth { get; set; } = 0;
        public decimal LastPreviousIndex { get; set; } = 0m;
        public decimal LastCurrentIndex { get; set; } = 0m;
        public decimal LastTotalBilled { get; set; } = 0m;

        // Son kayıttan (veya sözleşme başlangıcından) bugüne kaç ay geçti
        public int MonthsSinceLastReading { get; set; } = 0;

        // UI için açıklama
        public string SourceDescription { get; set; } = "";
    }

}
