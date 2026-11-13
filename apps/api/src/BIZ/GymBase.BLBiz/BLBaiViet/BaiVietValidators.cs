using FluentValidation;
using GymBase.Base.Validation;
using GymBase.Entities.Biz.Enums;

namespace GymBase.BLBiz.BaiViet;

public sealed class BaiVietCreateValidator : AppValidator<BaiVietCreateDto>
{
    public const int MaxMediaCount = 10;
    public const int MaxContentLength = 5000;

    private static readonly HashSet<string> ValidVisibilities = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(CheDoHienThi.cong_khai),
        nameof(CheDoHienThi.nguoi_theo_doi),
        nameof(CheDoHienThi.rieng_tu),
        "public",
        "followers",
        "private",
    };

    private static readonly HashSet<string> ValidPostTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(LoaiBaiViet.van_ban),
        nameof(LoaiBaiViet.hinh_anh),
        nameof(LoaiBaiViet.video),
        nameof(LoaiBaiViet.hon_hop),
    };

    public BaiVietCreateValidator()
    {
        RuleFor(x => x.noi_dung)
            .MaximumLength(MaxContentLength)
            .WithName("Nội dung");

        RuleFor(x => x)
            .Must(HasContentOrMedia)
            .WithMessage("Bài viết phải có nội dung văn bản hoặc ít nhất một ảnh/video.")
            .OverridePropertyName(nameof(BaiVietCreateDto.noi_dung));

        RuleFor(x => x.che_do_hien_thi)
            .NotEmpty()
            .Must(v => ValidVisibilities.Contains(v.Trim()))
            .WithMessage("Chế độ hiển thị không hợp lệ.")
            .WithName("Chế độ hiển thị");

        RuleFor(x => x.loai_bai_viet)
            .Must(v => string.IsNullOrWhiteSpace(v) || ValidPostTypes.Contains(v.Trim()))
            .WithMessage("Loại bài viết không hợp lệ.")
            .WithName("Loại bài viết");

        RuleFor(x => x.chu_de)
            .MaximumLength(32)
            .WithName("Chủ đề");

        RuleFor(x => x.hinh_anh_urls)
            .Must(list => list is null || list.Count <= MaxMediaCount)
            .WithMessage($"Chỉ được đính kèm tối đa {MaxMediaCount} ảnh/video.")
            .WithName("Ảnh/video đính kèm");
    }

    private static bool HasContentOrMedia(BaiVietCreateDto dto)
    {
        var hasText = !string.IsNullOrWhiteSpace(dto.noi_dung);
        var hasMedia = dto.hinh_anh_urls is { Count: > 0 } && dto.hinh_anh_urls.Any(u => !string.IsNullOrWhiteSpace(u));
        return hasText || hasMedia;
    }
}

public sealed class BaiVietUpdateValidator : AppValidator<BaiVietUpdateDto>
{
    private static readonly HashSet<string> ValidVisibilities = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(CheDoHienThi.cong_khai),
        nameof(CheDoHienThi.nguoi_theo_doi),
        nameof(CheDoHienThi.rieng_tu),
        "public",
        "followers",
        "private",
    };

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(TrangThaiBaiViet.nhap),
        nameof(TrangThaiBaiViet.da_dang),
        nameof(TrangThaiBaiViet.an),
        nameof(TrangThaiBaiViet.go_bo),
    };

    public BaiVietUpdateValidator()
    {
        RuleFor(x => x.noi_dung)
            .MaximumLength(BaiVietCreateValidator.MaxContentLength)
            .WithName("Nội dung");

        RuleFor(x => x.che_do_hien_thi)
            .NotEmpty()
            .Must(v => ValidVisibilities.Contains(v.Trim()))
            .WithMessage("Chế độ hiển thị không hợp lệ.")
            .WithName("Chế độ hiển thị");

        RuleFor(x => x.trang_thai)
            .NotEmpty()
            .Must(v => ValidStatuses.Contains(v.Trim()))
            .WithMessage("Trạng thái bài viết không hợp lệ.")
            .WithName("Trạng thái");
    }
}

public sealed class BinhLuanCreateValidator : AppValidator<BinhLuanCreateDto>
{
    public BinhLuanCreateValidator()
    {
        RuleFor(x => x.noi_dung)
            .NotEmpty()
            .MaximumLength(2000)
            .WithName("Nội dung bình luận");
    }
}
