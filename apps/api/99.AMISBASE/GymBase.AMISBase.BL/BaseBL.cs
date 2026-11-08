using GymBase.AMISBase.DL.Contract;
using GymBase.AMISBase.DL.Contract.Entities;
using GymBase.AMISBase.DL.Contract.Paging;

namespace GymBase.AMISBase.BL;

/// <summary>
/// CRUD dùng chung: chuẩn hóa phân trang, map entity ↔ DTO, tự điền audit (giờ UTC) và xóa mềm.
/// BL của từng bảng chỉ cần viết 3 hàm map. Muốn thêm kiểm tra nghiệp vụ thì override
/// <see cref="CreateAsync"/>, <see cref="UpdateAsync"/> hoặc <see cref="DeleteAsync"/>.
/// </summary>
/// <typeparam name="TEntity">Entity ánh xạ tới bảng.</typeparam>
/// <typeparam name="TDto">Dữ liệu trả ra API.</typeparam>
/// <typeparam name="TCreateDto">Body khi thêm mới.</typeparam>
/// <typeparam name="TUpdateDto">Body khi sửa.</typeparam>
/// <param name="dl">DL của bảng, ví dụ <c>IUserDL</c>.</param>
/// <param name="timeProvider">Nguồn giờ UTC để ghi ngày tạo, ngày sửa; test truyền giờ cố định.</param>
public abstract class BaseBL<TEntity, TDto, TCreateDto, TUpdateDto>(IBaseDL<TEntity> dl, TimeProvider timeProvider)
    : ICrudBL<TDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
{
    protected IBaseDL<TEntity> DL { get; } = dl;

    protected TimeProvider TimeProvider { get; } = timeProvider;

    // TODO(CURRENT-USER): lấy tên người đang thao tác từ ICurrentUser (claims của JWT). Mục 10.
    /// <summary>Tên người đang thao tác, ghi vào nguoi_tao / nguoi_chinh_sua. Tạm null đến khi có ICurrentUser.</summary>
    protected virtual string? CurrentUserName => null;

    protected abstract TDto ToDto(TEntity entity);

    /// <summary>Tạo entity mới từ body. Không cần gán Id hay ngày tạo.</summary>
    protected abstract TEntity CreateEntity(TCreateDto dto);

    /// <summary>Chép các trường được phép sửa từ body vào entity.</summary>
    protected abstract void ApplyUpdate(TEntity entity, TUpdateDto dto);

    public virtual async Task<PagingResult<TDto>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken)
    {
        var page = await DL.GetPagingAsync(request.Normalize(), cancellationToken);
        return page.Map(ToDto);
    }

    public virtual async Task<TDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await DL.GetByIdAsync(id, cancellationToken);
        return entity is null ? default : ToDto(entity);
    }

    public virtual async Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken)
    {
        var entity = CreateEntity(dto);
        if (entity is BaseAuditableEntity auditable)
        {
            auditable.MarkCreated(CurrentUserName, UtcNow);
        }

        await DL.InsertAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public virtual async Task<TDto?> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await DL.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return default;
        }

        ApplyUpdate(entity, dto);
        MarkUpdated(entity);

        return await DL.UpdateAsync(entity, cancellationToken) ? ToDto(entity) : default;
    }

    /// <summary>Entity có <see cref="ISoftDelete"/> thì xóa mềm, còn lại xóa hẳn.</summary>
    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await DL.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        if (entity is not ISoftDelete softDelete)
        {
            return await DL.DeleteAsync(id, cancellationToken);
        }

        softDelete.MarkDeleted();
        MarkUpdated(entity);
        return await DL.UpdateAsync(entity, cancellationToken);
    }

    private DateTime UtcNow => TimeProvider.GetUtcNow().UtcDateTime;

    private void MarkUpdated(TEntity entity)
    {
        if (entity is BaseAuditableEntity auditable)
        {
            auditable.MarkUpdated(CurrentUserName, UtcNow);
        }
    }
}
