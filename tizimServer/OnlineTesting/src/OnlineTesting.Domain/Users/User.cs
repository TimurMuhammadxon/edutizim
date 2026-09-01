using OnlineTesting.Domain.Common;


namespace OnlineTesting.Domain.Users;

public class User : Entity
{
    private readonly List<RefreshToken> _refreshTokens = new();
    private readonly List<ExternalLogin> _externalLogins = new();

    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? PasswordHash { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public Role Role { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public IReadOnlyCollection<ExternalLogin> ExternalLogins => _externalLogins.AsReadOnly();

    private User() { } // EF

    private User(
        Guid id,
        string email,
        string? passwordHash,
        bool emailConfirmed,
        Role role) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        EmailConfirmed = emailConfirmed;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    private User(
        Guid id,
        Guid organizationId,
        Role role,
        string phone,
        string passwordHash) : base(id)
    {
        OrganizationId = organizationId;
        Role = role;
        Phone = phone;
        PasswordHash = passwordHash;
        EmailConfirmed = false;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создание юзера через email + пароль. Email требует подтверждения.
    /// </summary>
    public static User CreateWithEmail(string email, string passwordHash, Role role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User(
            Guid.NewGuid(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            emailConfirmed: false,
            role);
    }

    /// <summary>
    /// Создание юзера через external провайдера (Telegram и т.д.).
    /// Пароля нет, email — placeholder, EmailConfirmed = true (provider уже верифицировал identity).
    /// </summary>
    public static User CreateFromExternal(string placeholderEmail, Role role,
        string? firstName = null, string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(placeholderEmail))
            throw new ArgumentException("Placeholder email is required.", nameof(placeholderEmail));

        var user = new User(
            Guid.NewGuid(),
            placeholderEmail.Trim().ToLowerInvariant(),
            passwordHash: null,
            emailConfirmed: true,
            role);

        user.FirstName = firstName?.Trim();
        user.LastName = lastName?.Trim();
        return user;
    }

    /// <summary>
    /// Создание нового OrgAdmin — владельца новой организации (самостоятельная регистрация).
    /// </summary>
    public static User CreateOrgAdmin(string email, string passwordHash, Guid organizationId)
    {
        var user = CreateWithEmail(email, passwordHash, Role.OrgAdmin);
        user.OrganizationId = organizationId;
        return user;
    }

    /// <summary>
    /// Создание OrgAdmin через external провайдера — используется при первом входе через Google/Telegram.
    /// </summary>
    public static User CreateOrgAdminFromExternal(string placeholderEmail, Guid organizationId,
        string? firstName = null, string? lastName = null)
    {
        var user = CreateFromExternal(placeholderEmail, Role.OrgAdmin, firstName, lastName);
        user.OrganizationId = organizationId;
        return user;
    }

    /// <summary>
    /// Создание сотрудника организации (Staff/Teacher/Student) напрямую вышестоящим
    /// пользователем — логин по номеру телефона, без email.
    /// </summary>
    public static User CreateOrgMember(
        Guid organizationId, Role role, string phone, string passwordHash,
        string? firstName = null, string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone is required.", nameof(phone));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        var user = new User(Guid.NewGuid(), organizationId, role, phone.Trim(), passwordHash);
        user.FirstName = firstName?.Trim();
        user.LastName = lastName?.Trim();
        return user;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void ConfirmEmail() => EmailConfirmed = true;
    public void SetRole(Role role) => Role = role;
    public void SetName(string? firstName, string? lastName)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
    }

    public void SetCredentials(string email, string passwordHash)
    {
        Email = email;
        PasswordHash = passwordHash;
        EmailConfirmed = true;
    }
}