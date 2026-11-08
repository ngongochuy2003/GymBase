using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.AMISBase.API.Errors;

// TODO(HOST): gộp vào AddAppApi()/UseAppApi() khi làm mục 11.
/// <summary>Đăng ký xử lý lỗi dùng chung cho mọi host: lỗi nào cũng trả ProblemDetails theo một dạng.</summary>
public static class ErrorHandlingExtensions
{
    /// <summary>ProblemDetails có code, trace_id; global exception handler cho exception chưa bắt.</summary>
    public static IServiceCollection AddAppErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsCustomizer.Customize);
        services.AddExceptionHandler<AppExceptionHandler>();
        return services;
    }

    /// <summary>
    /// Đặt sớm trong pipeline (trước UseHttpsRedirection). Exception chưa bắt thành ProblemDetails;
    /// response lỗi không có body (404 sai đường dẫn, 401 từ middleware xác thực, 405…) cũng được
    /// điền ProblemDetails.
    /// </summary>
    public static IApplicationBuilder UseAppErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        return app;
    }
}
