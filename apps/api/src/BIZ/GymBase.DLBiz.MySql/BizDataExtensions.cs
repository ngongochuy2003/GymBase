using GymBase.DLBiz.Contract;
using GymBase.DLBiz.Contract.DLChiSoCoThe;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.DLBiz.Contract.DLKetNoiPt;
using GymBase.DLBiz.MySql.DLChiSoCoThe;
using GymBase.DLBiz.MySql.DLHoSoNguoiDung;
using GymBase.DLBiz.MySql.DLKetNoiPt;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.DLBiz.MySql;

public static class BizDataExtensions
{
    /// <summary>
    /// Đăng ký repository MySQL của nhóm Biz: bài viết và tương tác, hồ sơ, chỉ số cơ thể, kết nối PT. Thêm repository
    /// mới của nhóm thì thêm ở đây.
    /// </summary>
    public static IServiceCollection AddBizData(this IServiceCollection services)
    {
        services.AddScoped<IBaiVietRepository, BaiVietRepository>();
        services.AddScoped<ITuongTacBaiVietRepository, TuongTacBaiVietRepository>();
        services.AddScoped<IHoSoNguoiDungRepository, HoSoNguoiDungRepository>();
        services.AddScoped<IChiSoCoTheRepository, ChiSoCoTheRepository>();
        services.AddScoped<IKetNoiPtRepository, KetNoiPtRepository>();
        return services;
    }
}
