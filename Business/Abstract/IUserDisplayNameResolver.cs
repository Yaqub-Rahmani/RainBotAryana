namespace Business.Abstract;

public interface IUserDisplayNameResolver
{
    // CreatedByUserId -> "Ahmad"
    string GetDisplayName(int userId);
}
