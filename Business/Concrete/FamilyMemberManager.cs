using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Transaction;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;
using Entities.Dtos.FamilyMember;
using DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Business.ValidationRules.FluentValidation.FamilyMember;

namespace Business.Concrete
{
    public class FamilyMemberManager : IFamilyMemberService
    {
        private readonly IFamilyMemberDal _familyMemberDal;
        private readonly IFamilyBalanceEntryDal _familyBalanceEntryDal;
        private readonly IMediaAssetDal _mediaDal;                      // Avatar
        private readonly IPhoneDal _phoneDal;                           // Primary phone
        private readonly IAddressDal _addrDal;
        private readonly IBiometricDal _biometricDal;

        public FamilyMemberManager(
            IFamilyMemberDal familyMemberDal,
            IFamilyBalanceEntryDal familyBalanceEntryDal,
            IMediaAssetDal mediaAssetDal,
            IPhoneDal phoneDal,
            IAddressDal addressDal,
            IBiometricDal biometricDal)
        {
            _familyMemberDal = familyMemberDal;
            _familyBalanceEntryDal = familyBalanceEntryDal;
            _mediaDal = mediaAssetDal;
            _phoneDal = phoneDal;
            _addrDal = addressDal;
            _biometricDal = biometricDal;

        }

        public IDataResult<List<FamilyMemberDto>> GetAll(bool includeInactive = false)
        {
            var query = _familyMemberDal.GetList();

            if (!includeInactive)
                query = query.Where(x => x.IsActive).ToList();

            var list = query
                .OrderByDescending(x => x.IsParent)
                .ThenBy(x => x.DisplayName)
                .Select(MapToDto)
                .ToList();

            return new SuccessDataResult<List<FamilyMemberDto>>(list);
        }

        public IDataResult<FamilyMemberDto> GetById(Guid id)
        {
            var entity = _familyMemberDal.Get(x => x.Id == id);
            if (entity == null)
                return new ErrorDataResult<FamilyMemberDto>(null, "عضو خانواده یافت نشد.");

            return new SuccessDataResult<FamilyMemberDto>(MapToDto(entity));
        }

        // ---------------- Profile ----------------

        [CacheAspect(10)]
        public IDataResult<FamilyMemberProfileDto> GetProfile(Guid familyMemberId)
        {
            var ownerType = MediaOwnerType.FamilyMember;

            var kAvatar = MediaKind.Avatar;
            var kIdFront = MediaKind.IdFront;
            var kIdBack = MediaKind.IdBack;
            var kFinger = MediaKind.FingerBitmap;

            var q =
                from f in _familyMemberDal.QueryNoTracking()
                where f.Id == familyMemberId

                // Phones
                let phonePrimary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == f.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).FirstOrDefault()

                let phoneSecondary =
                    (from p in _phoneDal.QueryNoTracking()
                     where p.OwnerType == ownerType && p.OwnerId == f.Id
                     orderby p.IsPrimary descending, p.Id ascending
                     select p.Number).Skip(1).FirstOrDefault()

                // Address (primary first)
                let addr =
                    (from a in _addrDal.QueryNoTracking()
                     where a.OwnerType == ownerType && a.OwnerId == f.Id
                     orderby a.IsPrimary descending
                     select new
                     {
                         Province = a.Province != null ? a.Province.Name : null,
                         District = a.District != null ? a.District.Name : null,
                         a.Line1
                     }).FirstOrDefault()

                // Media
                let avatarUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == f.Id && m.Kind == kAvatar
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let idFrontUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == f.Id && m.Kind == kIdFront
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let idBackUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == f.Id && m.Kind == kIdBack
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                let fingerUrl =
                    (from m in _mediaDal.QueryNoTracking()
                     where m.OwnerType == ownerType && m.OwnerId == f.Id && m.Kind == kFinger
                     orderby m.IsPrimary descending, m.Id descending
                     select m.StorageUrl).FirstOrDefault()

                // Balance sums (FamilyBalanceEntry)
                let sums =
                    (from b in _familyBalanceEntryDal.QueryNoTracking()
                     where b.FamilyMemberId == f.Id
                     group b by 1 into g
                     select new
                     {
                         TotalIn = g.Where(x => x.Direction == BalanceDirection.In).Sum(x => (decimal?)x.Amount) ?? 0m,
                         TotalOut = g.Where(x => x.Direction == BalanceDirection.Out).Sum(x => (decimal?)x.Amount) ?? 0m
                     }).FirstOrDefault()

                select new FamilyMemberProfileDto
                {
                    Id = f.Id.ToString(),
                    DisplayName = f.DisplayName,
                    FatherName = f.FatherName,
                    Gender = f.Gender,
                    NationalId = f.NationalId,

                    RelationTag = f.RelationTag ?? "",
                    IsActive = f.IsActive,
                    IsParent = f.IsParent,
                    IncludeInShare = f.IncludeInShare,

                    PhonePrimary = phonePrimary ?? "",
                    PhoneSecondary = phoneSecondary ?? "",

                    Province = addr != null ? (addr.Province ?? "") : "",
                    District = addr != null ? (addr.District ?? "") : "",
                    AddressLine = addr != null ? (addr.Line1 ?? "") : "",
                    AddressFull =
                        (addr == null)
                            ? ""
                            : (
                                (addr.Province ?? "") +
                                ((addr.Province != null && addr.District != null) ? ", " : "") +
                                (addr.District ?? "") +
                                (((addr.Province != null || addr.District != null) && addr.Line1 != null) ? " - " : "") +
                                (addr.Line1 ?? "")
                              ),

                    TotalIn = sums != null ? sums.TotalIn : 0m,
                    TotalOut = sums != null ? sums.TotalOut : 0m,

                    AvatarUrl = avatarUrl ?? "",
                    IdFrontUrl = idFrontUrl ?? "",
                    IdBackUrl = idBackUrl ?? "",
                    FingerBitmapUrl = fingerUrl ?? ""
                };

            var dto = q.FirstOrDefault();
            if (dto == null)
                return new ErrorDataResult<FamilyMemberProfileDto>(null, "یافت نشد.");

            // null-safety
            dto.DisplayName ??= "";
            dto.FatherName ??= "";
            dto.NationalId ??= "";
            dto.RelationTag ??= "";

            dto.PhonePrimary ??= "";
            dto.PhoneSecondary ??= "";

            dto.Province ??= "";
            dto.District ??= "";
            dto.AddressLine ??= "";
            dto.AddressFull ??= "";

            dto.AvatarUrl ??= "";
            dto.IdFrontUrl ??= "";
            dto.IdBackUrl ??= "";
            dto.FingerBitmapUrl ??= "";

            return new SuccessDataResult<FamilyMemberProfileDto>(dto);
        }

        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        public async Task<IResult> UpsertFingerprintAsync(Guid id, FamilyMemberFingerprintUpsertDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TemplateBase64))
                return new ErrorResult("TemplateBase64 zorunlu.");

            var exists = await _familyMemberDal.QueryNoTracking().AnyAsync(x => x.Id == id);
            if (!exists) return new ErrorResult("عضو خانواده یافت نشد.");

            var templateBytes = DecodeBase64(dto.TemplateBase64);

            // Deactivate existing active fingerprints
            var actives = await _biometricDal.Query()
                .Where(x => x.OwnerType == MediaOwnerType.FamilyMember
                         && x.OwnerId == id
                         && x.Type == BiometricType.Fingerprint
                         && x.IsActive)
                .ToListAsync();

            foreach (var a in actives)
            {
                a.IsActive = false;
                _biometricDal.Update(a);
            }

            var bio = new Biometric
            {
                Id = Guid.NewGuid(),
                OwnerType = MediaOwnerType.FamilyMember,
                OwnerId = id,
                Type = BiometricType.Fingerprint,
                Template = templateBytes,
                RelatedMediaAssetId = dto.RelatedMediaAssetId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _biometricDal.Add(bio);
            return new SuccessResult("اثر انگشت بروزرسانی شد.");
        }

        private static byte[] DecodeBase64(string input)
        {
            var s = (input ?? "").Trim();
            var idx = s.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) s = s.Substring(idx + "base64,".Length);
            return Convert.FromBase64String(s);
        }

        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        public IResult Add(FamilyMemberCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DisplayName))
                return new ErrorResult("نام عضو اجباری است.");

            string relationTag;
            if (dto.IsParent)
            {
                // Anne / Baba
                relationTag = dto.Gender == Gender.Male
                    ? "پدر"   // baba
                    : "مادر"; // anne
            }
            else
            {
                // Çocuk: erkek / kız
                relationTag = dto.Gender == Gender.Male
                    ? "پسر"   // erkek çocuk
                    : "دختر"; // kız çocuk
            }

            var entity = new FamilyMember
            {
                Id = dto.Id,
                DisplayName = dto.DisplayName.Trim(),
                FatherName = dto.FatherName?.Trim(),
                Gender = dto.Gender,
                NationalId = dto.NationalId?.Trim(),
                RelationTag = relationTag,
                IsParent = dto.IsParent,
                IncludeInShare = true,
                IsActive = true
            };

            _familyMemberDal.Add(entity);
            return new SuccessResult("عضو خانواده ثبت شد.");
        }

        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        public IResult Update(FamilyMemberUpdateDto dto)
        {
            var entity = _familyMemberDal.Get(x => x.Id == dto.Id);
            if (entity == null)
                return new ErrorResult("عضو خانواده یافت نشد.");

            if (string.IsNullOrWhiteSpace(dto.DisplayName))
                return new ErrorResult("نام عضو اجباری است.");

            entity.DisplayName = dto.DisplayName.Trim();
            entity.FatherName = dto.FatherName?.Trim();
            entity.Gender = dto.Gender;
            entity.NationalId = dto.NationalId?.Trim();
            entity.RelationTag = dto.RelationTag?.Trim();
            entity.IsParent = dto.IsParent;
            entity.IncludeInShare = dto.IncludeInShare;
            entity.IsActive = dto.IsActive;

            _familyMemberDal.Update(entity);
            return new SuccessResult("عضو خانواده به‌روزرسانی شد.");
        }

        [CacheRemoveAspect(".*FamilyMember.*Get.*")]
        public IResult Delete(Guid id)
        {
            var entity = _familyMemberDal.Get(x => x.Id == id);
            if (entity == null)
                return new ErrorResult("عضو خانواده یافت نشد.");

            // İstersen soft delete de yapabilirsin:
            // entity.IsActive = false; _familyMemberDal.Update(entity);
            _familyMemberDal.Delete(entity);
            return new SuccessResult("عضو خانواده حذف شد.");
        }



        private static FamilyMemberDto MapToDto(FamilyMember x)
        {
            return new FamilyMemberDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                FatherName = x.FatherName,
                Gender = x.Gender,
                NationalId = x.NationalId,
                RelationTag = x.RelationTag,
                IsParent = x.IsParent,
                IncludeInShare = x.IncludeInShare,
                IsActive = x.IsActive
            };
        }

        [CacheAspect(10)]
        public IDataResult<List<FamilyMemberViewDto>> GetAllFamilyView(bool? active = null)
        {
            // 1) FamilyMember çek
            var famQ = _familyMemberDal.QueryNoTracking();
            if (active.HasValue)
                famQ = famQ.Where(f => f.IsActive == active.Value);

            var members = famQ
                .Select(f => new { f.Id, f.DisplayName, f.RelationTag, f.IsActive })
                .OrderByDescending(f => f.IsActive)
                .ThenBy(f => f.DisplayName)
                .ToList();

            var memberIds = members.Select(x => x.Id).ToList();
            if (memberIds.Count == 0)
                return new SuccessDataResult<List<FamilyMemberViewDto>>(new());

            // 2) Avatar (primary) — MediaAsset
            var avatars = _mediaDal.QueryNoTracking()
                .Where(m => memberIds.Contains(m.OwnerId)
                            && m.OwnerType == MediaOwnerType.FamilyMember
                            && m.Kind == MediaKind.Avatar
                            && m.IsPrimary)
                .Select(m => new { m.OwnerId, m.StorageUrl })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().StorageUrl);

            // 3) Primary Phone
            var phones = _phoneDal.QueryNoTracking()
                .Where(p => memberIds.Contains(p.OwnerId)
                            && p.OwnerType == MediaOwnerType.FamilyMember
                            && p.IsPrimary)
                .Select(p => new { p.OwnerId, p.Number })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(g => g.Key, g => g.First().Number);

            // 4) Primary Address (+ Province, District)
            var addresses = _addrDal.QueryNoTracking()
                .Where(a => memberIds.Contains(a.OwnerId)
                            && a.OwnerType == MediaOwnerType.FamilyMember
                            && a.IsPrimary)
                .Include(a => a.Province)
                .Include(a => a.District)
                .Select(a => new
                {
                    a.OwnerId,
                    ProvinceName = a.Province != null ? a.Province.Name : null,
                    DistrictName = a.District != null ? a.District.Name : null,
                    a.Line1
                })
                .ToList()
                .GroupBy(x => x.OwnerId)
                .ToDictionary(
                    g => g.Key,
                    g => FormatAddress(
                        g.First().ProvinceName,
                        g.First().DistrictName,
                        g.First().Line1
                    )
                );

            // 5) Balance (FamilyBalanceEntry üzerinden)
            var balances = _familyBalanceEntryDal.QueryNoTracking()
                .Where(b => memberIds.Contains(b.FamilyMemberId))
                .Select(b => new
                {
                    b.FamilyMemberId,
                    AmountSigned = b.Direction == BalanceDirection.In ? b.Amount : -b.Amount
                })
                .GroupBy(x => x.FamilyMemberId)
                .Select(g => new
                {
                    FamilyMemberId = g.Key,
                    Balance = g.Sum(x => x.AmountSigned)
                })
                .ToList()
                .ToDictionary(x => x.FamilyMemberId, x => x.Balance);

            // 6) Birleştir
            var result = members.Select(f => new FamilyMemberViewDto
            {
                Id = f.Id,
                DisplayName = f.DisplayName,
                IsActive = f.IsActive,
                Avatar = avatars.TryGetValue(f.Id, out var url) ? url : null,
                PrimaryPhone = phones.TryGetValue(f.Id, out var ph) ? ph : null,
                RelationTag = f.RelationTag,
                AddressFull = addresses.TryGetValue(f.Id, out var addr) ? addr : null,
                Balance = balances.TryGetValue(f.Id, out var bal) ? bal : 0m
            }).ToList();

            return new SuccessDataResult<List<FamilyMemberViewDto>>(result);
        }


        private static string? FormatAddress(string? province, string? district, string? line1)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(province)) parts.Add(province);
            if (!string.IsNullOrWhiteSpace(district)) parts.Add(district);
            // if (!string.IsNullOrWhiteSpace(line1)) parts.Add(line1);
            return parts.Count == 0 ? null : string.Join(" - ", parts);
        }

    }
}
