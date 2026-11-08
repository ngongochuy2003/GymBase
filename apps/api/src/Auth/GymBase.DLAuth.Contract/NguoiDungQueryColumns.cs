using GymBase.Base.Paging;
using GymBase.DLAuth.Contract.Entities;

namespace GymBase.DLAuth.Contract;

/// <summary>Các cột của <c>nguoi_dung</c> mà client được sort và filter. Không có mat_khau_hash, is_deleted.</summary>
public static class NguoiDungQueryColumns
{
    public static QueryColumns<nguoi_dung> Allowed { get; } = new QueryColumns<nguoi_dung>()
        .Add(u => u.tai_khoan)
        .Add(u => u.ho_ten)
        .Add(u => u.email)
        .Add(u => u.so_dien_thoai)
        .Add(u => u.vai_tro)
        .Add(u => u.dang_hoat_dong)
        .Add(u => u.ngay_tao)
        .Add(u => u.ngay_chinh_sua);
}
