using GymBase.DLDI.Contract.DLDanhMuc;

namespace GymBase.BLDI.BLDanhMuc;

/// <summary>
/// Nghiệp vụ danh mục: trả các mục đang dùng, nhóm theo loại. Chỉ đọc; sửa danh mục hiện làm trực tiếp trong DB.
/// </summary>
public sealed class DanhMucService(IDanhMucRepository danhMucRepository)
{
    /// <summary>
    /// Danh mục đang dùng, nhóm theo <c>ma_dinh_danh</c>. Loại không có mục dùng được thì không xuất hiện trong kết quả.
    /// </summary>
    public async Task<IReadOnlyList<DanhMucNhomDto>> BLLayDanhMucAsync(
        IReadOnlyCollection<string>? maDinhDanhs, CancellationToken cancellationToken)
    {
        var cacMuc = await danhMucRepository.DLFindDangDungAsync(maDinhDanhs, cancellationToken);

        return cacMuc
            .GroupBy(e => e.ma_dinh_danh)
            .Select(nhom => new DanhMucNhomDto(
                nhom.Key,
                nhom.Select(e => new DanhMucItemDto(e.ma, e.ten, e.mo_ta, e.cap, e.danh_muc_cap_tren_id)).ToList()))
            .ToList();
    }
}
