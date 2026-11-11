using GymBase.Base.Auth;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.Entities.Biz.Enums;

namespace GymBase.BLBiz.BLQuyenHan;

/// <summary>
/// Tính quyền của tài khoản từ hồ sơ và trạng thái tài khoản. Luật: chỉ PT có hồ sơ <c>da_xac_minh</c> và tài khoản
/// không bị khóa mới được quản lý học viên; <c>loai_nguoi_dung</c> chỉ cho biết nhóm, không tự cấp quyền.
/// Dùng chung cho policy và endpoint <c>GET /auth/quyen-han</c>. Thiết kế: docs/api/phan-quyen.md.
/// </summary>
public sealed class QuyenHanService(IHoSoNguoiDungRepository hoSos, ITaiKhoanTrangThaiProvider taiKhoans) : IQuyenHanProvider
{
    public async Task<QuyenHanDto> BLLayQuyenHanAsync(Guid taiKhoanId, CancellationToken cancellationToken)
    {
        var hoSo = await hoSos.DLFindByTaiKhoanIdAsync(taiKhoanId, cancellationToken);
        if (hoSo is null)
        {
            return new QuyenHanDto(false, null, null, false, false, []);
        }

        var laPt = hoSo.loai_nguoi_dung is LoaiNguoiDung.pt or LoaiNguoiDung.ca_hai;
        var laPtDaXacMinh = laPt
            && hoSo.trang_thai_xac_minh == TrangThaiXacMinh.da_xac_minh
            && await taiKhoans.BLKiemTraHoatDongAsync(taiKhoanId, cancellationToken);

        return new QuyenHanDto(
            true,
            hoSo.loai_nguoi_dung.ToString(),
            hoSo.trang_thai_xac_minh.ToString(),
            laPt,
            laPtDaXacMinh,
            laPtDaXacMinh ? [NangLuc.QuanLyHocVien] : []);
    }
}
