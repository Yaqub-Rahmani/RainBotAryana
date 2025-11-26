using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    public class Market : IEntity
    {
        public Guid Id { get; set; }

        public string MarketName { get; set; } = null!;

        public bool Status { get; set; } = true; // varsayılan aktif

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Floor> Floors { get; set; } = new List<Floor>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Shop> Shops { get; set; } = new List<Shop>();
    }
}
