using System.Text.Json.Serialization;

namespace GymBase.BLThongBao.BLThongBao;

/// <summary>
/// Một dòng hộp thư. Chỉ có phần app cần; trạng thái hàng đợi (số lần thử, mã chống trùng) là chuyện nội bộ của server.
/// <c>loai_doi_tuong</c> cùng <c>doi_tuong_id</c> để app mở đúng màn hình.
/// </summary>
public sealed record ThongBaoDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("loai_thong_bao")] string LoaiThongBao,
    [property: JsonPropertyName("tieu_de")] string TieuDe,
    [property: JsonPropertyName("noi_dung")] string? NoiDung,
    [property: JsonPropertyName("loai_doi_tuong")] string? LoaiDoiTuong,
    [property: JsonPropertyName("doi_tuong_id")] Guid? DoiTuongId,
    [property: JsonPropertyName("thoi_diem")] DateTime ThoiDiem,
    [property: JsonPropertyName("da_doc")] bool DaDoc);

public sealed record HopThuDto(
    [property: JsonPropertyName("data")] IReadOnlyList<ThongBaoDto> Data,
    [property: JsonPropertyName("con_nua")] bool ConNua);

public sealed record SoChuaDocDto([property: JsonPropertyName("so_chua_doc")] int SoChuaDoc);
