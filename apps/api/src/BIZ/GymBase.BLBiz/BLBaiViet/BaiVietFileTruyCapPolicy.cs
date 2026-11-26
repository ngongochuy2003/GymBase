using GymBase.DLBiz.Contract;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;

namespace GymBase.BLBiz.BaiViet;

/// <summary>
/// Ai được xem ảnh/video đính kèm bài viết: giống quyền xem bài viết. Bài công khai (và dành cho người theo dõi, bảng tin đang coi như
/// công khai) thì mọi tài khoản xem được; bài riêng tư chỉ tác giả. Bài đã gỡ hoặc ẩn chỉ tác giả xem.
/// </summary>
public sealed class BaiVietFileTruyCapPolicy(IBaiVietRepository baiViets) : IFileTruyCapPolicy
{
    public string LoaiDoiTuong => nameof(LoaiDoiTuongTepTin.bai_viet);

    public async Task<IReadOnlySet<Guid>> BLKiemTruyCapAsync(
        Guid nguoiXemId, IReadOnlyCollection<TepTinTruyCap> cacTep, CancellationToken ct = default)
    {
        var idBaiViet = cacTep.Where(t => t.DoiTuongId.HasValue).Select(t => t.DoiTuongId!.Value).Distinct().ToList();
        var cacBai = await baiViets.DLFindByIdsAsync(idBaiViet, ct);

        var xemDuoc = cacBai
            .Where(b => b.tac_gia_id == nguoiXemId
                        || (b.trang_thai == TrangThaiBaiViet.da_dang && b.che_do_hien_thi != CheDoHienThi.rieng_tu))
            .Select(b => b.Id)
            .ToHashSet();

        return cacTep
            .Where(t => t.DoiTuongId is { } id && xemDuoc.Contains(id))
            .Select(t => t.Id)
            .ToHashSet();
    }
}
