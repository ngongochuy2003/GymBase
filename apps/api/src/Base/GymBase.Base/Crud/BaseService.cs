using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Entities;
using GymBase.Base.Paging;
using GymBase.Base.Validation;
using FluentValidation;

namespace GymBase.Base.Crud;

/// <summary>
/// CRUD dùng chung: chuẩn hóa phân trang, map entity ↔ DTO, tự điền audit (giờ UTC).
/// BL của từng bảng chỉ cần viết 3 hàm map. Muốn thêm kiểm tra nghiệp vụ thì override
/// <see cref="BLCreateAsync"/>, <see cref="BLUpdateAsync"/> hoặc <see cref="BLDeleteAsync"/> và ném
/// exception trong <see cref="Exceptions"/> (trùng → <see cref="ConflictException"/>, vi phạm quy tắc →
/// <see cref="BusinessRuleException"/>…). Không có bản ghi thì tự ném <see cref="NotFoundException"/>.
/// </summary>
/// <typeparam name="TEntity">Entity ánh xạ tới bảng.</typeparam>
/// <typeparam name="TDto">Dữ liệu trả ra API.</typeparam>
/// <typeparam name="TCreateDto">Body khi thêm mới.</typeparam>
/// <typeparam name="TUpdateDto">Body khi sửa.</typeparam>
/// <param name="repository">DL của bảng, ví dụ <c>ITaiKhoanRepository</c>.</param>
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

    protected abstract TDto BLToDto(TEntity entity);

    /// <summary>Tạo entity mới từ body. Không cần gán Id hay ngày tạo.</summary>
    protected abstract TEntity BLCreateEntity(TCreateDto dto);

    /// <summary>Chép các trường được phép sửa từ body vào entity.</summary>
    protected abstract void BLApplyUpdate(TEntity entity, TUpdateDto dto);

    /// <summary>Kiểm tra nghiệp vụ trước khi thêm, ví dụ chặn trùng mã. Chạy sau validator nên dto đã hợp lệ.</summary>
    protected virtual Task BLBeforeCreateAsync(TCreateDto dto, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Kiểm tra nghiệp vụ trước khi sửa, ví dụ chặn đổi sang giá trị đã có người dùng. Chạy sau validator.</summary>
    protected virtual Task BLBeforeUpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual async Task<PagingResult<TDto>> BLGetPagingAsync(PagingRequest request, CancellationToken cancellationToken)
    {
        var page = await Repository.DLGetPagingAsync(request.Normalize(), cancellationToken);
        return page.Map(BLToDto);
    }

    public virtual async Task<TDto> BLGetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        BLToDto(await BLGetEntityOrThrowAsync(id, cancellationToken));

    public virtual async Task<TDto> BLCreateAsync(TCreateDto dto, CancellationToken cancellationToken)
    {
        await createValidator.ValidateOrThrowAsync(dto, cancellationToken);
        await BLBeforeCreateAsync(dto, cancellationToken);

        var entity = BLCreateEntity(dto);
        if (entity is BaseAuditableEntity auditable)
        {
            auditable.MarkCreated(CurrentUser.UserName, UtcNow);
        }

        await Repository.DLInsertAsync(entity, cancellationToken);
        return BLToDto(entity);
    }

    public virtual async Task<TDto> BLUpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateOrThrowAsync(dto, cancellationToken);
        await BLBeforeUpdateAsync(id, dto, cancellationToken);

        var entity = await BLGetEntityOrThrowAsync(id, cancellationToken);

        BLApplyUpdate(entity, dto);
        MarkUpdated(entity);

        // Bản ghi bị xóa giữa lúc đọc và lúc ghi.
        if (!await Repository.DLUpdateAsync(entity, cancellationToken))
        {
            throw BLNotFound();
        }

        return BLToDto(entity);
    }

    /// <summary>
    /// Xóa hẳn bản ghi; không có thì 404. Bản ghi đang được bảng khác tham chiếu thì DL ném
    /// <see cref="ConflictException"/> (<see cref="ErrorCodes.InUse"/>).
    /// </summary>
    public virtual async Task BLDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await Repository.DLDeleteAsync(id, cancellationToken))
        {
            throw BLNotFound();
        }
    }

    /// <summary>Tên đối tượng trong câu báo lỗi, ví dụ "người dùng" → "Không tìm thấy người dùng…".</summary>
    protected virtual string EntityDisplayName => "dữ liệu";

    /// <summary>Đọc bản ghi; không có thì ném <see cref="NotFoundException"/>.</summary>
    protected async Task<TEntity> BLGetEntityOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await Repository.DLGetByIdAsync(id, cancellationToken) ?? throw BLNotFound();

    protected NotFoundException BLNotFound() =>
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
