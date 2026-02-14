using Core.Entities;
using System;

namespace Entities.Dtos.Employee
{
    /// <summary>
    /// Employee fingerprint template upsert payload.
    /// TemplateBase64: fingerprint template (base64). Can include data:*;base64, prefix.
    /// RelatedMediaAssetId: optional MediaAsset Id that points to stored bitmap (png).
    /// </summary>
    public class EmployeeFingerprintUpsertDto : IDto
    {
        public string TemplateBase64 { get; set; } = default!;
        public Guid? RelatedMediaAssetId { get; set; }
    }
}
