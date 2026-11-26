using System.Text.Json.Serialization;
using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.Base.Hosting;
using GymBase.BLThongBao.BLThongBao;
using GymBase.ModuleContracts.ThongBao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Mob.Controllers;

/// <summary>
/// Hộp thông báo trên kênh mobile: <c>/api/v1/mob/thong-bao</c>. <c>thong_bao</c> là nguồn sự thật; push chỉ là cái chuông,
/// nên app gọi lấy hộp thư mỗi lần mở app hoặc quay lại foreground (ADR-2). Xem docs/adr/0002-thong-bao-day-bang-fcm-truc-tiep-outbox.md.
/// </summary>
[Authorize]
[Route("thong-bao")]
public sealed class ThongBaoController(
    ThongBaoService thongBaoService,
    IThongBaoTao thongBaoTao,
    ICurrentUser currentUser,
    IWebHostEnvironment environment) : BaseApiController
{
    private Guid TaiKhoanId => currentUser.UserId ?? throw new UnauthorizedException();

    /// <summary>Hộp thư, mới nhất trước. <c>bo_qua</c> và <c>so_luong</c> để phân trang (tối đa 50); <c>chi_chua_doc</c> lọc chưa đọc.</summary>
    [HttpGet]
    public async Task<IActionResult> LayHopThu(
        [FromQuery(Name = "chi_chua_doc")] bool chiChuaDoc,
        [FromQuery(Name = "bo_qua")] int boQua,
        [FromQuery(Name = "so_luong")] int soLuong = 20,
        CancellationToken ct = default) =>
        Ok(await thongBaoService.BLLayHopThuAsync(TaiKhoanId, chiChuaDoc, boQua, soLuong, ct));

    /// <summary>Số thông báo chưa đọc, cho huy hiệu trên chuông.</summary>
    [HttpGet("so-chua-doc")]
    public async Task<IActionResult> DemChuaDoc(CancellationToken ct) =>
        Ok(new SoChuaDocDto(await thongBaoService.BLDemChuaDocAsync(TaiKhoanId, ct)));

    [HttpPost("{id:guid}/da-doc")]
    public async Task<IActionResult> DanhDauDaDoc(Guid id, CancellationToken ct)
    {
        await thongBaoService.BLDanhDauDaDocAsync(TaiKhoanId, id, ct);
        return NoContent();
    }

    [HttpPost("da-doc-tat-ca")]
    public async Task<IActionResult> DanhDauTatCaDaDoc(CancellationToken ct) =>
        Ok(new { so_dong = await thongBaoService.BLDanhDauTatCaDaDocAsync(TaiKhoanId, ct) });

    /// <summary>
    /// Chỉ môi trường Development: tạo một thông báo cho chính người gọi để thử hộp thư và push (worker gửi tới thiết bị đã đăng ký).
    /// <c>tre_giay</c> (0 đến 300) hẹn giờ gửi, để kịp tắt app rồi xem push tới khi app đã đóng; dòng chưa tới giờ thì chưa hiện trong hộp thư.
    /// </summary>
    // TODO(THONGBAO): bỏ endpoint thử này (và nút thử ở màn tin nhắn của app) khi tin nhắn thật đã gọi IThongBaoTao.
    [HttpPost("thu-nghiem")]
    public async Task<IActionResult> ThuNghiem([FromBody] ThuNghiemDto? dto, CancellationToken ct)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var tre = Math.Clamp(dto?.TreGiay ?? 0, 0, 300);
        var id = await thongBaoTao.BLTaoAsync(
            new TaoThongBaoRequest(
                TaiKhoanId,
                dto?.LoaiThongBao ?? "tin_nhan",
                dto?.TieuDe ?? "Thông báo thử",
                "Đây là thông báo thử từ server.",
                "thu_nghiem",
                null,
                $"thu_nghiem:{Guid.NewGuid():N}",
                tre > 0 ? DateTime.UtcNow.AddSeconds(tre) : null),
            ct);
        return Ok(new { id });
    }
}

public sealed record ThuNghiemDto(
    string? LoaiThongBao,
    string? TieuDe,
    [property: JsonPropertyName("tre_giay")] int? TreGiay);
