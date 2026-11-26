namespace GymBase.BLFile.BLProcessing;

/// <summary>Ảnh sau xử lý: đã xoay đúng chiều và bỏ metadata (EXIF, GPS), cùng định dạng với ảnh gốc.</summary>
public sealed record ProcessedImage(byte[] Data, int Width, int Height, string Blurhash);

/// <summary>Ảnh vượt giới hạn kích thước điểm ảnh (chống ảnh nén bom làm cạn bộ nhớ khi giải mã).</summary>
public sealed class ImageTooLargeException(string message) : Exception(message);

public interface IImageProcessor
{
    /// <summary>Giải mã ảnh JPEG, PNG hoặc WebP, xoay đúng chiều theo EXIF, mã hóa lại không kèm metadata và tính blurhash.</summary>
    /// <exception cref="ImageTooLargeException">Ảnh vượt giới hạn kích thước điểm ảnh; kiểm trước khi giải mã.</exception>
    /// <exception cref="InvalidDataException">Dữ liệu không giải mã được hoặc định dạng không hỗ trợ.</exception>
    Task<ProcessedImage> XuLyAsync(Stream source, string contentType, CancellationToken ct = default);
}
