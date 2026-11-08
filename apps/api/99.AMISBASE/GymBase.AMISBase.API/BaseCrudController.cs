using GymBase.AMISBase.BL;
using GymBase.AMISBase.DL.Contract.Entities;
using GymBase.AMISBase.DL.Contract.Paging;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.AMISBase.API;

// TODO(ERROR): lỗi nghiệp vụ (trùng, không hợp lệ, không có quyền) ném exception và trả ProblemDetails
//   qua global exception handler, không try/catch trong từng action. Mục 7.
// TODO(AUTH): khi có xác thực, controller cần giới hạn quyền ghi thì override action và gắn [Authorize(Roles = ...)]. Mục 12.
/// <summary>
/// Controller CRUD dùng chung, đủ 5 action: GET danh sách, GET {id}, POST, PUT {id}, DELETE {id}.
/// Body sai kiểu hoặc vi phạm DataAnnotations của DTO bị [ApiController] trả 400 trước khi vào action.
/// Muốn đổi hành vi một action thì override (các action đều virtual).
/// </summary>
public abstract class BaseCrudController<TDto, TCreateDto, TUpdateDto>(ICrudBL<TDto, TCreateDto, TUpdateDto> bl)
    : BaseApiController
    where TDto : IHasId
{
    protected ICrudBL<TDto, TCreateDto, TUpdateDto> BL { get; } = bl;

    /// <summary>Danh sách có phân trang. Ví dụ: <c>?page=1&amp;pageSize=20&amp;search=abc</c>.</summary>
    [HttpGet]
    public virtual async Task<ActionResult<PagingResult<TDto>>> GetPaging(
        [FromQuery] PagingRequest request, CancellationToken cancellationToken) =>
        Ok(await BL.GetPagingAsync(request, cancellationToken));

    /// <summary>Chi tiết một bản ghi; 404 nếu không có hoặc đã xóa.</summary>
    [HttpGet("{id:guid}")]
    public virtual async Task<ActionResult<TDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await BL.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>Thêm mới; 201 kèm header Location trỏ tới bản ghi vừa tạo.</summary>
    [HttpPost]
    public virtual async Task<ActionResult<TDto>> Create([FromBody] TCreateDto dto, CancellationToken cancellationToken)
    {
        var created = await BL.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Sửa; 404 nếu không có hoặc đã xóa.</summary>
    [HttpPut("{id:guid}")]
    public virtual async Task<ActionResult<TDto>> Update(
        Guid id, [FromBody] TUpdateDto dto, CancellationToken cancellationToken)
    {
        var updated = await BL.UpdateAsync(id, dto, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Xóa (xóa mềm nếu entity có is_deleted); 204 nếu thành công, 404 nếu không có.</summary>
    [HttpDelete("{id:guid}")]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await BL.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
