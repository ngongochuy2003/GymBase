using GymBase.Base.Paging;
using GymBase.Entities.Auth;

namespace GymBase.DLAuth.Contract.DLTaiKhoan;

/// <summary>Các cột của <c>tai_khoan</c> mà client được sort và filter. Không có mat_khau_hash, is_deleted.</summary>
public static class TaiKhoanQueryColumns
{
    public static QueryColumns<tai_khoan> Allowed { get; } = new QueryColumns<tai_khoan>()
        .Add(u => u.ten_dang_nhap)
        .Add(u => u.ho_ten)
        .Add(u => u.email)
        .Add(u => u.so_dien_thoai)
        .Add(u => u.vai_tro)
        .Add(u => u.dang_hoat_dong)
        .Add(u => u.ngay_tao)
        .Add(u => u.ngay_chinh_sua);
}
