using GymBase.BLFile.BLStorage;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;

namespace GymBase.BLFile.BLFiles;

public enum LoaiFile
{
    Anh,
    Video,
    TaiLieu,
}

/// <summary>Luật upload của một <c>muc_dich</c>: cho loại file nào và mặc định công khai hay riêng tư.</summary>
public sealed record FileMucDichRule(string MucDich, IReadOnlySet<LoaiFile> Loai, CheDoTruyCap CheDoMacDinh);

/// <summary>
/// Một nơi duy nhất quyết định mục đích upload hợp lệ, MIME được phép, dung lượng tối đa và magic bytes. Quyền xem thật do
/// <see cref="IFileTruyCapPolicy"/> của đối tượng đích quyết định; <c>CheDoMacDinh</c> chỉ là gợi ý lưu trong <c>tep_tin</c>.
/// </summary>
public static class FilePolicy
{
    private static readonly HashSet<LoaiFile> ChiAnh = [LoaiFile.Anh];
    private static readonly HashSet<LoaiFile> ChiTaiLieu = [LoaiFile.TaiLieu];
    private static readonly HashSet<LoaiFile> AnhVaVideo = [LoaiFile.Anh, LoaiFile.Video];

    private static readonly Dictionary<string, FileMucDichRule> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["avatar"] = new("avatar", ChiAnh, CheDoTruyCap.cong_khai),
        ["post"] = new("post", AnhVaVideo, CheDoTruyCap.cong_khai),
        ["gym_photo"] = new("gym_photo", ChiAnh, CheDoTruyCap.rieng_tu),
        ["progress"] = new("progress", ChiAnh, CheDoTruyCap.rieng_tu),
        ["check_in"] = new("check_in", AnhVaVideo, CheDoTruyCap.rieng_tu),
        [IChungChiPtLienKet.MucDich] = new(IChungChiPtLienKet.MucDich, ChiTaiLieu, CheDoTruyCap.rieng_tu),
    };

    private static readonly Dictionary<string, LoaiFile> MimeLoai = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = LoaiFile.Anh,
        ["image/png"] = LoaiFile.Anh,
        ["image/webp"] = LoaiFile.Anh,
        ["image/heic"] = LoaiFile.Anh,
        ["image/heif"] = LoaiFile.Anh,
        ["video/mp4"] = LoaiFile.Video,
        ["video/quicktime"] = LoaiFile.Video,
        ["application/pdf"] = LoaiFile.TaiLieu,
    };

    public static IEnumerable<string> CacMucDich => Rules.Keys;

    /// <summary>
    /// Trạng thái xử lý nền ngay sau khi xác nhận: ảnh JPEG, PNG, WebP cần bỏ EXIF và tính blurhash. HEIC và HEIF chưa giải mã được nên giữ
    /// nguyên (app đã đổi sang JPEG khi nén); video và PDF không cần.
    /// </summary>
    public static TrangThaiXuLyTepTin TrangThaiXuLyBanDau(string mime) =>
        mime is "image/jpeg" or "image/png" or "image/webp" ? TrangThaiXuLyTepTin.cho_xu_ly : TrangThaiXuLyTepTin.khong_can;

    /// <summary>Key tạm app POST vào. Sau confirm file được copy sang <c>tep_tin.s3_key</c> nên form upload còn hạn không đè được file đã kiểm.</summary>
    public static string KeyTam(Guid fileId) => $"tmp/{fileId}";

    /// <summary>Đuôi tệp theo MIME (có dấu chấm), để key và URL có đuôi: app đoán ảnh hay video theo đuôi URL.</summary>
    public static string DuoiTep(string mime) => mime switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/heic" => ".heic",
        "image/heif" => ".heif",
        "video/mp4" => ".mp4",
        "video/quicktime" => ".mov",
        "application/pdf" => ".pdf",
        _ => string.Empty,
    };

    public static bool TryGetRule(string? mucDich, out FileMucDichRule rule)
    {
        rule = null!;
        return !string.IsNullOrWhiteSpace(mucDich) && Rules.TryGetValue(mucDich.Trim(), out rule!);
    }

    /// <summary>Bỏ tham số (<c>; charset=...</c>) và chữ hoa của MIME; trả null nếu trống.</summary>
    public static string? ChuanHoaMime(string? mime)
    {
        if (string.IsNullOrWhiteSpace(mime))
        {
            return null;
        }

        var semicolon = mime.IndexOf(';');
        var core = semicolon >= 0 ? mime[..semicolon] : mime;
        return core.Trim().ToLowerInvariant();
    }

    public static LoaiFile? PhanLoai(string? mime) =>
        mime != null && MimeLoai.TryGetValue(mime, out var loai) ? loai : null;

    public static long DungLuongToiDa(LoaiFile loai, StorageOptions options) => loai switch
    {
        LoaiFile.Anh => options.MaxImageBytes,
        LoaiFile.Video => options.MaxVideoBytes,
        _ => options.MaxDocumentBytes,
    };

    /// <summary>Số byte đầu cần đọc để kiểm <see cref="KhopMagicBytes"/>.</summary>
    public const int SoByteKiemMagic = 16;

    /// <summary>Nội dung thật có đúng là loại đã khai báo không (không tin MIME của client).</summary>
    public static bool KhopMagicBytes(string mime, ReadOnlySpan<byte> head)
    {
        if (head.Length < 12)
        {
            return false;
        }

        return mime switch
        {
            "image/jpeg" => head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF,
            "image/png" => head[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/webp" => head[..4].SequenceEqual("RIFF"u8) && head[8..12].SequenceEqual("WEBP"u8),
            "image/heic" or "image/heif" => CoFtyp(head) && DauHeic(head[8..12]),
            "video/mp4" or "video/quicktime" => CoFtyp(head),
            "application/pdf" => head[..4].SequenceEqual("%PDF"u8),
            _ => false,
        };
    }

    private static bool CoFtyp(ReadOnlySpan<byte> head) => head[4..8].SequenceEqual("ftyp"u8);

    private static bool DauHeic(ReadOnlySpan<byte> brand) =>
        brand.StartsWith("hei"u8) || brand.StartsWith("hev"u8) || brand.SequenceEqual("mif1"u8) || brand.SequenceEqual("msf1"u8);
}
