using GymBase.Base.Entities;

namespace GymBase.BLBiz.BaiViet;

/// <summary>Thông tin tác giả hiển thị kèm bài viết trên bảng tin.</summary>
public sealed record TacGiaBaiVietDto(
    Guid id,
    string ten_hien_thi,
    string? anh_dai_dien_url);

/// <summary>Dữ liệu bài viết trả ra API cho cả bảng tin và chi tiết bài viết.</summary>
public sealed record BaiVietDto(
    Guid Id,
    Guid tac_gia_id,
    TacGiaBaiVietDto tac_gia,
    string? noi_dung,
    string loai_bai_viet,
    string che_do_hien_thi,
    string trang_thai,
    string? chu_de,
    IReadOnlyList<string> hinh_anh_urls,
    int so_luot_thich,
    int so_binh_luan,
    bool da_thich,
    DateTime? ngay_dang,
    DateTime ngay_tao,
    DateTime? ngay_chinh_sua) : IHasId;

/// <summary>Body khi tạo bài viết mới từ web hoặc mobile.</summary>
public sealed class BaiVietCreateDto
{
    public string? noi_dung { get; init; }

    /// <summary>Tùy chọn: <c>van_ban</c>, <c>hinh_anh</c>, <c>video</c>, <c>hon_hop</c>. Bỏ trống sẽ tự suy ra từ nội dung và tệp đính kèm.</summary>
    public string? loai_bai_viet { get; init; }

    /// <summary><c>cong_khai</c> (<c>public</c>), <c>nguoi_theo_doi</c> (<c>followers</c>) hoặc <c>rieng_tu</c> (<c>private</c>).</summary>
    public string che_do_hien_thi { get; init; } = "cong_khai";

    /// <summary>Mã chủ đề bài viết (ví dụ: <c>workout</c>, <c>meal-prep</c>, <c>progress</c>, <c>tips</c>).</summary>
    public string? chu_de { get; init; }

    /// <summary>Danh sách đường dẫn ảnh/video đính kèm (tối đa 10).</summary>
    public List<string>? hinh_anh_urls { get; init; }

    /// <summary>Tên tác giả hiển thị (tùy chọn khi tạo từ phiên trải nghiệm).</summary>
    public string? ten_tac_gia { get; init; }
}

/// <summary>Body khi cập nhật bài viết.</summary>
public sealed class BaiVietUpdateDto
{
    public string? noi_dung { get; init; }

    public string che_do_hien_thi { get; init; } = "cong_khai";

    public string trang_thai { get; init; } = "da_dang";
}

/// <summary>Kết quả sau khi bật/tắt lượt thích trên bài viết.</summary>
public sealed record KetQuaThichBaiVietDto(
    Guid bai_viet_id,
    bool da_thich,
    int so_luot_thich);

/// <summary>Body khi thêm bình luận vào bài viết.</summary>
public sealed class BinhLuanCreateDto
{
    public string noi_dung { get; init; } = string.Empty;

    public Guid? tuong_tac_cha_id { get; init; }

    public string? ten_nguoi_dung { get; init; }
}

/// <summary>Thông tin một bình luận trả ra API.</summary>
public sealed record BinhLuanDto(
    Guid Id,
    Guid bai_viet_id,
    Guid tai_khoan_id,
    string ten_nguoi_dung,
    string? anh_dai_dien_url,
    string noi_dung,
    Guid? tuong_tac_cha_id,
    DateTime ngay_tao) : IHasId;
