using GymBase.Base.Crud;
using GymBase.Base.Exceptions;
using GymBase.Base.Paging;
using GymBase.BLThongBao.BLThietBi;
using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Xunit;

namespace GymBase.UnitTests.ThongBao;

/// <summary>
/// Hai request đăng ký cùng một thiết bị mới chạy song song: cả hai thấy chưa có dòng, một bên insert trước, bên kia va unique index
/// <c>(tai_khoan_id, ma_thiet_bi)</c>. Bên thua phải cập nhật dòng của bên thắng, không được báo lỗi.
/// </summary>
public sealed class ThietBiRaceTests
{
    private static readonly DateTime Now = ThongBaoKit.Now;

    /// <summary>Lần insert đầu tiên: chèn dòng của "request thắng" rồi ném trùng khóa như MySQL.</summary>
    private sealed class RacingRepository(IThietBiRepository inner, Func<Task> request) : IThietBiRepository
    {
        public int SoLanInsert { get; private set; }

        public async Task DLInsertAsync(thiet_bi entity, CancellationToken ct)
        {
            if (SoLanInsert++ == 0)
            {
                await request();
            }

            await inner.DLInsertAsync(entity, ct);
        }

        public Task<PagingResult<thiet_bi>> DLGetPagingAsync(PagingRequest request, CancellationToken ct) => inner.DLGetPagingAsync(request, ct);
        public Task<thiet_bi?> DLGetByIdAsync(Guid id, CancellationToken ct) => inner.DLGetByIdAsync(id, ct);
        public Task<bool> DLUpdateAsync(thiet_bi entity, CancellationToken ct) => inner.DLUpdateAsync(entity, ct);
        public Task<bool> DLDeleteAsync(Guid id, CancellationToken ct) => inner.DLDeleteAsync(id, ct);
        public Task<thiet_bi?> DLFindByMaThietBiAsync(Guid taiKhoanId, string maThietBi, CancellationToken ct = default) => inner.DLFindByMaThietBiAsync(taiKhoanId, maThietBi, ct);
        public Task<IReadOnlyList<thiet_bi>> DLFindActiveWithTokenAsync(Guid taiKhoanId, CancellationToken ct = default) => inner.DLFindActiveWithTokenAsync(taiKhoanId, ct);
        public Task DLClearPushTokenAsync(Guid thietBiId, CancellationToken ct = default) => inner.DLClearPushTokenAsync(thietBiId, ct);
        public Task DLReleasePushTokenAsync(string pushToken, Guid tru, CancellationToken ct = default) => inner.DLReleasePushTokenAsync(pushToken, tru, ct);
    }

    [Fact]
    public async Task LosingTheInsertRace_UpdatesTheWinnersRow_InsteadOfFailing()
    {
        var kit = new ThongBaoKit();
        var tk = Guid.NewGuid();
        var thang = new thiet_bi
        {
            tai_khoan_id = tk, ma_thiet_bi = "may-1", nen_tang = NenTang.android, push_token = null,
            dang_hoat_dong = true, ngay_hoat_dong_cuoi = Now, ngay_tao = Now.AddSeconds(-1),
        };
        var repo = new RacingRepository(kit.ThietBis, async () =>
        {
            await kit.ThietBis.DLInsertAsync(thang, default);
            throw new ConflictException();
        });

        await new ThietBiService(repo, kit.Time).BLDangKyAsync(tk, new DangKyThietBiDto("may-1", "android", "tok-thua", "Asia/Bangkok"));

        var tb = Assert.Single(await kit.ThietBis.DLFindActiveWithTokenAsync(tk, default));
        Assert.Equal(thang.Id, tb.Id);
        Assert.Equal("tok-thua", tb.push_token);
        Assert.Equal("Asia/Bangkok", tb.mui_gio);
        Assert.Equal(thang.ngay_tao, tb.ngay_tao);
        Assert.Equal(1, repo.SoLanInsert);
    }
}
