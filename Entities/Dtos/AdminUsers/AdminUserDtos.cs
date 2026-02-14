using Core.Entities;

namespace Entities.Dtos.AdminUsers
{
    /// <summary>
    /// Admin ekranı için: kartlarda gösterilecek alanlar (Name / Number / Role)
    /// </summary>
    public class AdminUserListItemDto : IDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "admin";
        public bool Status { get; set; } = true;
    }

    /// <summary>
    /// Detay + düzenleme için
    /// </summary>
    public class AdminUserDetailDto : IDto
    {
        public int Id { get; set; }

        // Kart başlığı
        public string Name { get; set; } = "";

        // Düzenleme alanları
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }

        // UserOperationClaim -> OperationClaim.Name
        public string Role { get; set; } = "admin";

        // User.Status
        public bool Status { get; set; } = true;
    }

    public class AdminUserCreateDto : IDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }

        // Create için zorunlu
        public string Password { get; set; } = "";

        // tek rol seçiyoruz (admin/superadmin/accountant/viewer vs.)
        public string Role { get; set; } = "admin";

        public bool Status { get; set; } = true;
    }

    public class AdminUserUpdateDto : IDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }

        // Şifre değiştirmek istersen doldur (opsiyonel)
        public string? Password { get; set; }

        public string Role { get; set; } = "admin";
        public bool Status { get; set; } = true;
    }
}
