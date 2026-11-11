using GymBase.Base.Auth;
using GymBase.Base.Exceptions;
using GymBase.DLBiz.Contract.DLKetNoiPt;
using GymBase.Entities.Biz.Enums;

namespace GymBase.BLBiz.BLTruyCap;

/// <summary>
/// Quyền trên từng bản ghi: một PT chỉ được động tới dữ liệu của học viên có kết nối với mình, đúng lĩnh vực. Mọi hàm nhận
/// id của người gọi và của học viên từ nơi tin được (token, bản ghi trong DB), không bao giờ từ body request.
/// Không đủ quyền thì ném <see cref="ForbiddenException"/> kèm mã ở <see cref="BizErrorCodes"/>; endpoint nào cần giấu việc
/// bản ghi có tồn tại thì đổi thành <see cref="NotFoundException"/>. Thiết kế: docs/api/phan-quyen.md.
/// </summary>
public sealed class TruyCapService(IQuyenHanProvider quyenHan, IKetNoiPtRepository ketNois)
{
    /// <exception cref="ForbiddenException"><see cref="BizErrorCodes.PtNotVerified"/>: chưa phải PT đã xác minh.</exception>
    public async Task BLKiemTraPtDaXacMinhAsync(Guid ptId, CancellationToken cancellationToken)
    {
        var quyen = await quyenHan.BLLayQuyenHanAsync(ptId, cancellationToken);
        if (!quyen.la_pt_da_xac_minh)
        {
            throw new ForbiddenException("PT chưa được xác minh nên chưa được quản lý học viên.", BizErrorCodes.PtNotVerified);
        }
    }

    /// <summary>
    /// Ghi hoặc sửa dữ liệu của học viên trong một lĩnh vực: người gọi phải là PT đã xác minh, có kết nối đang hoạt động
    /// với học viên và kết nối đó phụ trách lĩnh vực này.
    /// </summary>
    /// <exception cref="ForbiddenException">
    /// <see cref="BizErrorCodes.PtNotVerified"/>, <see cref="BizErrorCodes.ConnectionNotActive"/> hoặc
    /// <see cref="BizErrorCodes.ScopeNotAllowed"/>.
    /// </exception>
    public async Task BLKiemTraQuanLyHocVienAsync(Guid ptId, Guid hocVienId, PhamViPhuTrach phamVi, CancellationToken cancellationToken)
    {
        await BLKiemTraPtDaXacMinhAsync(ptId, cancellationToken);

        var ketNoi = await ketNois.DLFindDangHoatDongAsync(ptId, hocVienId, cancellationToken)
            ?? throw new ForbiddenException("Không có kết nối đang hoạt động với học viên này.", BizErrorCodes.ConnectionNotActive);

        if (!ketNoi.pham_vi_phu_trach.Contains(phamVi))
        {
            throw new ForbiddenException("Kết nối với học viên không phụ trách lĩnh vực này.", BizErrorCodes.ScopeNotAllowed);
        }
    }

    /// <summary>
    /// Đọc dữ liệu của học viên. Chủ dữ liệu luôn đọc được. PT chỉ đọc được dữ liệu gắn với kết nối của chính mình với học
    /// viên đó (<paramref name="ketNoiPtId"/> là cột <c>ket_noi_pt_id</c> của bản ghi): kết nối đang hoạt động thì PT phải còn
    /// là PT đã xác minh, kết nối đã kết thúc thì PT chỉ đọc lại lịch sử của kết nối đó. Dữ liệu không gắn kết nối
    /// (<paramref name="ketNoiPtId"/> null) là dữ liệu riêng, chỉ chủ đọc được.
    /// </summary>
    /// <exception cref="ForbiddenException"><see cref="BizErrorCodes.DataNotShared"/> hoặc <see cref="BizErrorCodes.PtNotVerified"/>.</exception>
    public async Task BLKiemTraDocDuLieuHocVienAsync(
        Guid nguoiGoiId, Guid hocVienId, Guid? ketNoiPtId, CancellationToken cancellationToken)
    {
        if (nguoiGoiId == hocVienId)
        {
            return;
        }

        var ketNoi = ketNoiPtId is { } id ? await ketNois.DLGetByIdAsync(id, cancellationToken) : null;
        if (ketNoi is null || ketNoi.pt_id != nguoiGoiId || ketNoi.hoc_vien_id != hocVienId)
        {
            throw new ForbiddenException("Dữ liệu này chưa được chia sẻ cho bạn.", BizErrorCodes.DataNotShared);
        }

        switch (ketNoi.trang_thai)
        {
            case TrangThaiKetNoiPt.dang_hoat_dong:
                await BLKiemTraPtDaXacMinhAsync(nguoiGoiId, cancellationToken);
                break;
            case TrangThaiKetNoiPt.da_ket_thuc:
                break;
            default:
                throw new ForbiddenException("Dữ liệu này chưa được chia sẻ cho bạn.", BizErrorCodes.DataNotShared);
        }
    }
}
