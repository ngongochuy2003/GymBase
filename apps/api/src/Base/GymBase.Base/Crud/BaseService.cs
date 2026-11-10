using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Entities;
using GymBase.Base.Paging;
using GymBase.Base.Validation;
using FluentValidation;

namespace GymBase.Base.Crud;

/// <summary>
/// CRUD dùng chung: chuẩn hóa phân trang, map entity ↔ DTO, tự điền audit (giờ UTC) và xóa mềm.
/// BL của từng bảng chỉ cần viết 3 hàm map. Muốn thêm kiểm tra nghiệp vụ thì override
/// <see cref="CreateAsync"/>, <see cref="UpdateAsync"/> hoặc <see cref="DeleteAsync"/> và ném
/// exception trong <see cref="Exceptions"/> (trùng → <see cref="ConflictException"/>, vi phạm quy tắc →
/// <see cref="BusinessRuleException"/>…). Không có bản ghi thì tự ném <see cref="NotFoundException"/>.
/// </summary>
/// <typeparam name="TEntity">Entity ánh xạ tới bảng.</typeparam>
/// <typeparam name="TDto">Dữ liệu trả ra API.</typeparam>
/// <typeparam name="TCreateDto">Body khi thêm mới.</typeparam>
/// <typeparam name="TUpdateDto">Body khi sửa.</typeparam>
/// <param name="repository">DL của bảng, ví dụ <c>IUserRepository</c>.</param>
/// <param name="timeProvider">Nguồn giờ UTC để ghi ngày tạo, ngày sửa; test truyền giờ cố định.</param>
/// <param name="currentUser">Người đang thao tác, ghi vào người tạo, người sửa.</param>
/// <param name="createValidator">Kiểm tra body khi thêm, chạy trước mọi thao tác khác.</param>
/// <param name="updateValidator">Kiểm tra body khi sửa, chạy trước mọi thao tác khác.</param>
public abstract class BaseService<TEntity, TDto, TCreateDto, TUpdateDto>(
    IBaseRepository<TEntity> repository,
    TimeProvider timeProvider,
    ICurrentUser currentUser,
    IValidator<TCreateDto> createValidator,
    IValidator<TUpdateDto> updateValidator)
    : ICrudService<TDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
{
    protected IBaseRepository<TEntity> Repository { get; } = repository;

    protected TimeProvider TimeProvider { get; } = timeProvider;

    /// <summary>Người đang thao tác; null khi chưa đăng nhập.</summary>
    protected ICurrentUser CurrentUser { get; } = currentUser;

    protected abstract TDto ToDto(TEntity entity);

    /// <summary>Tạo entity mới từ body. Không cần gán Id hay ngày tạo.</summary>
    protected abstract TEntity CreateEntity(TCreateDto dto);

    /// <summary>Chép các trường được phép sửa từ body vào entity.</summary>
    protected abstract void ApplyUpdate(TEntity entity, TUpdateDto dto);

    /// <summary>Kiểm tra nghiệp vụ trước khi thêm, ví dụ chặn trùng mã. Chạy sau validator nên dto đã hợp lệ.</summary>
    protected virtual Task BeforeCreateAsync(TCreateDto dto, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual async Task<PagingResult<TDto>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken)
    {
        var page = await Repository.GetPagingAsync(request.Normalize(), cancellationToken);
        return page.Map(ToDto);
    }

    public virtual async Task<TDto> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        ToDto(await GetEntityOrThrowAsync(id, cancellationToken));

    public virtual async Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken)
    {
        await createValidator.ValidateOrThrowAsync(dto, cancellationToken);
        await BeforeCreateAsync(dto, cancellationToken);

        var entity = CreateEntity(dto);
        if (entity is BaseAuditableEntity auditable)
        {
            auditable.MarkCreated(CurrentUser.UserName, UtcNow);
        }

        await Repository.InsertAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public virtual async Task<TDto> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var entity = await GetEntityOrThrowAsync(id, cancellationToken);

        ApplyUpdate(entity, dto);
        MarkUpdated(entity);

        // Bản ghi bị xóa giữa lúc đọc và lúc ghi.
        if (!await Repository.UpdateAsync(entity, cancellationToken))
        {
            throw NotFound();
        }

        return ToDto(entity);
    }

    /// <summary>Entity có <see cref="ISoftDelete"/> thì xóa mềm, còn lại xóa hẳn.</summary>
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEntityOrThrowAsync(id, cancellationToken);

        bool deleted;
        if (entity is ISoftDelete softDelete)
        {
            softDelete.MarkDeleted();
            MarkUpdated(entity);
            deleted = await Repository.UpdateAsync(entity, cancellationToken);
        }
        else
        {
            deleted = await Repository.DeleteAsync(id, cancellationToken);
        }

        if (!deleted)
        {
            throw NotFound();
        }
    }

    /// <summary>Tên đối tượng trong câu báo lỗi, ví dụ "người dùng" → "Không tìm thấy người dùng…".</summary>
    protected virtual string EntityDisplayName => "dữ liệu";

    /// <summary>Đọc bản ghi còn hiệu lực; không có hoặc đã xóa thì ném <see cref="NotFoundException"/>.</summary>
    protected async Task<TEntity> GetEntityOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await Repository.GetByIdAsync(id, cancellationToken) ?? throw NotFound();

    protected NotFoundException NotFound() =>
        new($"Không tìm thấy {EntityDisplayName} hoặc {EntityDisplayName} đã bị xóa.");

    private DateTime UtcNow => TimeProvider.GetUtcNow().UtcDateTime;


    private void MarkUpdated(TEntity entity)
    {
        if (entity is BaseAuditableEntity auditable)
        {
            auditable.MarkUpdated(CurrentUser.UserName, UtcNow);
        }
    }
}
