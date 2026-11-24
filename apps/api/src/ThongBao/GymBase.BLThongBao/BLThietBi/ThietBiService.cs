using System.Text.RegularExpressions;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.BLThongBao.BLThietBi;

/// <summary>
/// Thiết bị của người dùng và push token FCM (ADR-2, quyết định 6 và 9). Đăng ký là upsert theo <c>(tai_khoan_id, ma_thiet_bi)</c>;
/// token đang nằm ở tài khoản khác thì gỡ khỏi đó trước, vì <c>push_token</c> unique.
/// </summary>
public sealed partial class ThietBiService(IThietBiRepository thietBis, TimeProvider timeProvider)
{
    private const int MaThietBiToiDa = 128;
    private const int PushTokenToiDa = 512;

    /// <summary>Tên múi giờ IANA như <c>Asia/Ho_Chi_Minh</c>, <c>Etc/GMT+7</c>.</summary>
    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_+\-]*(/[A-Za-z0-9_+\-]+){0,2}$")]
    private static partial Regex MuiGioHopLe();

    /// <exception cref="ValidationFailedException">Mã thiết bị trống hoặc quá dài, nền tảng lạ, token quá dài, múi giờ sai dạng.</exception>
    public async Task BLDangKyAsync(Guid taiKhoanId, DangKyThietBiDto dto, CancellationToken ct = default)
    {
        var nenTang = KiemTra(dto, out var maThietBi, out var token, out var muiGio);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var thietBi = await thietBis.DLFindByMaThietBiAsync(taiKhoanId, maThietBi, ct);
        var moi = thietBi == null;
        thietBi ??= new thiet_bi { tai_khoan_id = taiKhoanId, ma_thiet_bi = maThietBi };

        var doiToken = moi || thietBi.push_token != token;
        thietBi.nen_tang = nenTang;
        thietBi.push_token = token;
        thietBi.mui_gio = muiGio;
        thietBi.dang_hoat_dong = true;
        thietBi.ngay_hoat_dong_cuoi = now;
        if (moi)
        {
            thietBi.MarkCreated(null, now);
        }
        else if (doiToken)
        {
            thietBi.MarkUpdated(null, now);
        }

        await LuuAsync(thietBi, moi, token, ct);
    }

    /// <summary>
    /// Đăng xuất khỏi thiết bị: gỡ token và tắt, để người dùng kế tiếp trên máy này không nhận thông báo của người trước.
    /// Không có thiết bị thì coi như đã xong.
    /// </summary>
    public async Task BLGoAsync(Guid taiKhoanId, string maThietBi, CancellationToken ct = default)
    {
        var thietBi = await thietBis.DLFindByMaThietBiAsync(taiKhoanId, maThietBi, ct);
        if (thietBi != null)
        {
            await thietBis.DLClearPushTokenAsync(thietBi.Id, ct);
        }
    }

    /// <summary>
    /// Ghi thiết bị. Hai tình huống đua nhau ở unique index, mỗi bên được thử lại một lần:
    /// hai request cùng chuyển một token (va <c>push_token</c>) thì gỡ token lại rồi ghi tiếp; hai request cùng đăng ký một thiết bị
    /// mới (va <c>(tai_khoan_id, ma_thiet_bi)</c>, hay gặp khi app đồng bộ nhiều lần cùng lúc) thì bên thua cập nhật lên dòng bên thắng.
    /// </summary>
    private async Task LuuAsync(thiet_bi thietBi, bool moi, string? token, CancellationToken ct)
    {
        for (var lan = 0; ; lan++)
        {
            if (token != null)
            {
                await thietBis.DLReleasePushTokenAsync(token, thietBi.Id, ct);
            }

            try
            {
                if (moi)
                {
                    await thietBis.DLInsertAsync(thietBi, ct);
                }
                else
                {
                    await thietBis.DLUpdateAsync(thietBi, ct);
                }

                return;
            }
            catch (ConflictException) when (lan == 0)
            {
                if (moi && await thietBis.DLFindByMaThietBiAsync(thietBi.tai_khoan_id, thietBi.ma_thiet_bi, ct) is { } daCo)
                {
                    thietBi.Id = daCo.Id;
                    thietBi.ngay_tao = daCo.ngay_tao;
                    thietBi.nguoi_tao = daCo.nguoi_tao;
                    moi = false;
                }
            }
        }
    }

    private static NenTang KiemTra(DangKyThietBiDto dto, out string maThietBi, out string? token, out string? muiGio)
    {
        var loi = new Dictionary<string, string[]>();

        maThietBi = dto.MaThietBi?.Trim() ?? string.Empty;
        if (maThietBi.Length is 0 or > MaThietBiToiDa)
        {
            loi["ma_thiet_bi"] = [$"Mã thiết bị bắt buộc và tối đa {MaThietBiToiDa} ký tự."];
        }

        var nenTangHopLe = Enum.TryParse<NenTang>(dto.NenTang, ignoreCase: true, out var nenTang) && Enum.IsDefined(nenTang);
        if (!nenTangHopLe)
        {
            loi["nen_tang"] = ["Nền tảng phải là android hoặc ios."];
        }

        token = string.IsNullOrWhiteSpace(dto.PushToken) ? null : dto.PushToken.Trim();
        if (token is { Length: > PushTokenToiDa })
        {
            loi["push_token"] = [$"Push token tối đa {PushTokenToiDa} ký tự."];
        }

        muiGio = string.IsNullOrWhiteSpace(dto.MuiGio) ? null : dto.MuiGio.Trim();
        if (muiGio != null && (muiGio.Length > 64 || !MuiGioHopLe().IsMatch(muiGio)))
        {
            loi["mui_gio"] = ["Múi giờ phải là tên IANA, ví dụ Asia/Ho_Chi_Minh."];
        }

        if (loi.Count > 0)
        {
            throw new ValidationFailedException(loi);
        }

        return nenTang;
    }
}
