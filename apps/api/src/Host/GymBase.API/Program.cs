using GymBase.API;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Logging;
using GymBase.BLAuth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.BLBiz;
using GymBase.BLBiz.BaiViet;
using GymBase.BLDI;
using GymBase.BLFile;
using GymBase.BLFile.BLFiles;
using GymBase.BLThongBao;
using Microsoft.AspNetCore.Authorization;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.MySql;
using GymBase.DLDI.MySql;
using GymBase.DLFile.MySql;
using GymBase.DLThongBao.MySql;
using GymBase.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Host duy nhất cho cả web và mobile. Controller trong Controllers/ có tiền tố /api/v1/web,
// controller trong Mob/Controllers/ có tiền tố /api/v1/mob.
// Mỗi BL có validator thì thêm assembly của nó vào đây.
builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly, typeof(BaiVietService).Assembly);

// Policy RequireProfile không đạt thì trả mã PROFILE_REQUIRED (mã của nhóm Biz nên đặt ở host).
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProfileRequiredResultHandler>();

// Log kỹ thuật ra console và file, cấu hình ở mục "Serilog" của appsettings.
builder.Services.AddAppLogging(builder.Configuration);

// Một AppDbContext cho cả DB.
builder.Services.AddAppPersistence(builder.Configuration);

// Mỗi nhóm đăng ký hai phần: nghiệp vụ (AddXxxBusiness, ở BL) và repository MySQL (AddXxxData, ở DL.MySql).
// BL không tham chiếu DL.MySql nên hai phần phải gọi riêng ở host.

// Auth: người dùng, đăng nhập
builder.Services.AddAuthBusiness();
builder.Services.AddAuthData();

// Biz: mạng xã hội (bài viết, tương tác thích/bình luận), hồ sơ, quyền theo hồ sơ và quyền trên dữ liệu học viên
// (phân quyền: docs/api/phan-quyen.md)
builder.Services.AddBizBusiness();
builder.Services.AddBizData();

// File: lưu trữ tệp tin (Local/S3)
builder.Services.AddFileBusiness(builder.Configuration);
builder.Services.AddFileData();

// ThongBao: hộp thư thông báo, thiết bị và push token, worker đẩy push (ADR-2)
builder.Services.AddThongBaoBusiness(builder.Configuration);
builder.Services.AddThongBaoData();

// DI (Danh mục): dữ liệu tham chiếu cho dropdown (docs/api/danh-muc.md)
builder.Services.AddDIBusiness();
builder.Services.AddDIData();

var app = builder.Build();

app.UseAppApi();

app.Run();
return 0;
