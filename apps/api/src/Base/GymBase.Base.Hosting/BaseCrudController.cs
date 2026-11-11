using GymBase.Base.Crud;
using GymBase.Base.Entities;
using GymBase.Base.Paging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.Base.Hosting;

/// <summary>
/// Controller CRUD dùng chung, đủ 5 action: GET danh sách, GET {id}, POST, PUT {id}, DELETE {id}.
/// Action không try/catch: BL ném exception nghiệp vụ (không tìm thấy, trùng…), global exception
/// handler đổi thành ProblemDetails (xem <c>Errors/AppExceptionHandler</c>). Body sai kiểu hoặc vi
/// phạm DataAnnotations của DTO bị [ApiController] trả 400 trước khi vào action.
/// Muốn đổi hành vi một action thì override (các action đều virtual).
/// </summary>
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
public abstract class BaseCrudController<TDto, TCreateDto, TUpdateDto>(ICrudService<TDto, TCreateDto, TUpdateDto> service)
    : BaseApiController
    where TDto : IHasId
{
    protected ICrudService<TDto, TCreateDto, TUpdateDto> Service { get; } = service;

    /// <summary>Danh sách có phân trang. Ví dụ: <c>?page=1&amp;page_size=20&amp;search=abc</c>, trả <c>{ data, meta }</c>.</summary>
    [HttpGet]
    public virtual async Task<ActionResult<PagingResult<TDto>>> GetPaging(
        [FromQuery] PagingRequest request, CancellationToken cancellationToken) =>
        Ok(await Service.BLGetPagingAsync(request, cancellationToken));

    /// <summary>Chi tiết một bản ghi; 404 nếu không có hoặc đã xóa.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public virtual async Task<ActionResult<TDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await Service.BLGetByIdAsync(id, cancellationToken));

    /// <summary>Thêm mới; 201 kèm header Location trỏ tới bản ghi vừa tạo.</summary>
    [HttpPost]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public virtual async Task<ActionResult<TDto>> Create([FromBody] TCreateDto dto, CancellationToken cancellationToken)
    {
        var created = await Service.BLCreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Sửa; 404 nếu không có hoặc đã xóa.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public virtual async Task<ActionResult<TDto>> Update(
        Guid id, [FromBody] TUpdateDto dto, CancellationToken cancellationToken) =>
        Ok(await Service.BLUpdateAsync(id, dto, cancellationToken));

    /// <summary>Xóa (xóa mềm nếu entity có is_deleted); 204 nếu thành công, 404 nếu không có.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public virtual async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Service.BLDeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
