using GymBase.Base.Paging;
using GymBase.Entities.Biz;

namespace GymBase.DLFile.Contract;

public static class TepTinQueryColumns
{
    public static QueryColumns<tep_tin> Allowed { get; } = new QueryColumns<tep_tin>()
        .Add(e => e.ten_tep_goc)
        .Add(e => e.s3_key)
        .Add(e => e.s3_bucket)
        .Add(e => e.loai_mime)
        .Add(e => e.muc_dich)
        .Add(e => e.ngay_tao);
}
