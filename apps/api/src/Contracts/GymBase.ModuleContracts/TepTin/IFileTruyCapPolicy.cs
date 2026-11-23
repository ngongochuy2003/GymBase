namespace GymBase.ModuleContracts.TepTin;

/// <summary>Thông tin tối thiểu của một file đã gắn vào đối tượng, đủ để policy của đối tượng đó quyết định ai được xem.</summary>
public sealed record TepTinTruyCap(Guid Id, Guid NguoiTaiLenId, Guid? DoiTuongId, string? MucDich);

/// <summary>
/// Luật ai được xem file đã gắn vào một loại đối tượng (bài viết, hồ sơ, check-in...). Mỗi loại đối tượng đích tự cài ở BL của nhóm
/// đó; BLFile gom file theo <see cref="LoaiDoiTuong"/> rồi hỏi policy tương ứng. Chủ file luôn xem được nên policy không cần xét.
/// </summary>
public interface IFileTruyCapPolicy
{
    /// <summary>Giá trị <c>tep_tin.loai_doi_tuong</c> (snake_case) mà policy này phụ trách.</summary>
    string LoaiDoiTuong { get; }

    /// <summary>
    /// Kiểm theo lô để danh sách không kiểm từng file. Trả tập <c>Id</c> của các file <paramref name="nguoiXemId"/> được xem.
    /// </summary>
    Task<IReadOnlySet<Guid>> BLKiemTruyCapAsync(
        Guid nguoiXemId, IReadOnlyCollection<TepTinTruyCap> cacTep, CancellationToken ct = default);
}
