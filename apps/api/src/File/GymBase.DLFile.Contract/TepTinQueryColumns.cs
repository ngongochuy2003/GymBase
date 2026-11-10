using GymBase.Base.Paging;
using GymBase.Entities.File;

namespace GymBase.DLFile.Contract;

public static class TepTinQueryColumns
{
    public static QueryColumns<tep_tin> Allowed { get; } = new QueryColumns<tep_tin>()
        .Add(e => e.ten_tep_goc)
        .Add(e => e.khoa_luu_tru)
        .Add(e => e.nha_cung_cap)
        .Add(e => e.loai_mime)
        .Add(e => e.muc_dich)
        .Add(e => e.trang_thai)
        .Add(e => e.ngay_tao);
}
