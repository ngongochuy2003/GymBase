namespace GymBase.BLDI.BLDanhMuc;

/// <summary>Một mục danh mục trả cho app. App hiển thị <see cref="ten"/> nhưng gửi lại <see cref="ma"/> khi lưu hồ sơ.</summary>
public sealed record DanhMucItemDto(
    string ma,
    string ten,
    string? mo_ta,
    int? cap,
    Guid? danh_muc_cap_tren_id);

/// <summary>Các mục của một loại danh mục, app tách theo <see cref="ma_dinh_danh"/> để đổ vào từng dropdown.</summary>
public sealed record DanhMucNhomDto(string ma_dinh_danh, IReadOnlyList<DanhMucItemDto> items);
