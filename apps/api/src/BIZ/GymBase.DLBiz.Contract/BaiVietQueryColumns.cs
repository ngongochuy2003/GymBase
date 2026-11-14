using GymBase.Base.Paging;
using GymBase.Entities.Biz;

namespace GymBase.DLBiz.Contract;

/// <summary>Các cột của <c>bai_viet</c> mà client được phép sort và filter.</summary>
public static class BaiVietQueryColumns
{
    public static QueryColumns<bai_viet> Allowed { get; } = new QueryColumns<bai_viet>()
        .Add(p => p.tac_gia_id)
        .Add(p => p.loai_bai_viet)
        .Add(p => p.che_do_hien_thi)
        .Add(p => p.trang_thai)
        .Add(p => p.ngay_dang)
        .Add(p => p.ngay_tao)
        .Add(p => p.ngay_chinh_sua);
}
