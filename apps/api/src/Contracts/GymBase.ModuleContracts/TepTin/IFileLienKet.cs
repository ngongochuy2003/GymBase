namespace GymBase.ModuleContracts.TepTin;

/// <summary>
/// Gắn các file đã upload (qua <c>/files/upload-url</c> và <c>/files/{id}/confirm</c>) vào một đối tượng (bài viết, ...). Nhóm Biz dùng
/// hợp đồng này để không phải tham chiếu BLFile; hiện thực nằm ở BLFile và được host đăng ký.
/// </summary>
public interface IFileLienKet
{
    /// <summary>
    /// Gắn <paramref name="fileIds"/> (theo đúng thứ tự, đánh <c>thu_tu_hien_thi</c> từ 1) vào đối tượng <paramref name="doiTuongId"/>.
    /// Gọi trước khi lưu đối tượng để file sai thì không tạo đối tượng dở.
    /// </summary>
    /// <param name="loaiDoiTuong">Giá trị <c>tep_tin.loai_doi_tuong</c> (snake_case), ví dụ <c>bai_viet</c>.</param>
    /// <param name="mucDichChoPhep">Các <c>muc_dich</c> file được mang; file khác mục đích bị từ chối.</param>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">
    /// Có file không tồn tại, không phải của <paramref name="nguoiDungId"/>, chưa upload xong, đã gắn nơi khác, sai mục đích, hoặc bị trùng.
    /// </exception>
    Task BLGanAsync(
        Guid nguoiDungId,
        IReadOnlyCollection<Guid> fileIds,
        string loaiDoiTuong,
        Guid doiTuongId,
        IReadOnlyCollection<string> mucDichChoPhep,
        CancellationToken ct = default);
}
