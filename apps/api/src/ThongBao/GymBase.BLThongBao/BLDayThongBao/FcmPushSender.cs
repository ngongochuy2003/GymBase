using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>
/// Gửi push qua FCM HTTP v1 trực tiếp (ADR-2, quyết định 2 và 5). Kết quả của FCM được quy về bốn nhóm để worker quyết định:
/// thành công, lỗi tạm (thử lại), token chết (gỡ token), lỗi cấu hình (dừng, cần người xử lý). Không ném lỗi của FCM ra ngoài.
/// </summary>
/// <remarks>
/// Tin gồm <c>notification</c> (chỉ tiêu đề, không có nội dung) kèm <c>data</c> để app mở đúng màn hình. <c>channel_id</c> của Android
/// bằng <c>loai_thong_bao</c>, khớp kênh app tạo.
/// </remarks>
public sealed class FcmPushSender(
    HttpClient http,
    IFcmAccessTokenProvider tokens,
    IOptions<ThongBaoOptions> options,
    TimeProvider timeProvider,
    ILogger<FcmPushSender> logger) : IPushSender
{
    private readonly string _projectId = options.Value.Fcm.ProjectId;

    public async Task<KetQuaPush> GuiAsync(TinPush tin, CancellationToken ct = default)
    {
        string accessToken;
        try
        {
            accessToken = await tokens.GetAccessTokenAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Không lấy được access token: mạng tới Google lỗi thì thử lại được; còn lại (khóa sai, hết hạn, bị thu hồi) là lỗi cấu hình.
            logger.LogError(ex, "Không lấy được access token FCM.");
            return new KetQuaPush(ex is HttpRequestException ? KetQuaDay.LoiTam : KetQuaDay.LoiCauHinh, Loi: $"ACCESS_TOKEN: {ex.GetType().Name}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1/projects/{_projectId}/messages:send")
        {
            Content = JsonContent.Create(DungTin(tin)),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await http.SendAsync(request, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            return response.IsSuccessStatusCode ? DocThanhCong(body) : PhanLoaiLoi(response.StatusCode, body, tin.PushToken);
        }
        catch (Exception ex) when (ex is HttpRequestException || (ex is OperationCanceledException && !ct.IsCancellationRequested))
        {
            // Mất mạng hoặc hết thời gian chờ (HttpClient.Timeout): lỗi tạm. Người gọi hủy thì ném tiếp.
            return new KetQuaPush(KetQuaDay.LoiTam, Loi: ex is HttpRequestException ? "NETWORK" : "TIMEOUT");
        }
    }

    private object DungTin(TinPush tin)
    {
        var cao = tin.UuTien == UuTienPush.Cao;
        var data = new Dictionary<string, string>
        {
            ["thong_bao_id"] = tin.ThongBaoId.ToString(),
            ["loai_thong_bao"] = tin.LoaiThongBao,
        };
        if (tin.LoaiDoiTuong != null)
        {
            data["loai_doi_tuong"] = tin.LoaiDoiTuong;
        }

        if (tin.DoiTuongId != null)
        {
            data["doi_tuong_id"] = tin.DoiTuongId.Value.ToString();
        }

        var hetHan = new DateTimeOffset(timeProvider.GetUtcNow().UtcDateTime + tin.Ttl).ToUnixTimeSeconds();
        return new
        {
            message = new Dictionary<string, object>
            {
                ["token"] = tin.PushToken,
                ["notification"] = new Dictionary<string, string> { ["title"] = tin.TieuDe },
                ["data"] = data,
                ["android"] = new Dictionary<string, object>
                {
                    ["priority"] = cao ? "HIGH" : "NORMAL",
                    ["ttl"] = $"{(long)tin.Ttl.TotalSeconds}s",
                    ["notification"] = new Dictionary<string, string>
                    {
                        ["channel_id"] = tin.LoaiThongBao,
                        ["visibility"] = "PUBLIC",
                    },
                },
                ["apns"] = new Dictionary<string, object>
                {
                    ["headers"] = new Dictionary<string, string>
                    {
                        ["apns-priority"] = cao ? "10" : "5",
                        ["apns-push-type"] = "alert",
                        ["apns-expiration"] = hetHan.ToString(),
                    },
                    ["payload"] = new Dictionary<string, object>
                    {
                        ["aps"] = new Dictionary<string, object> { ["sound"] = "default" },
                    },
                },
            },
        };
    }

    private static KetQuaPush DocThanhCong(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            return new KetQuaPush(KetQuaDay.ThanhCong, doc.RootElement.TryGetProperty("name", out var name) ? name.GetString() : null);
        }
        catch (JsonException)
        {
            return new KetQuaPush(KetQuaDay.ThanhCong);
        }
    }

    /// <summary>
    /// Quy lỗi của FCM về bốn nhóm. <c>INVALID_ARGUMENT</c> dùng chung cho token sai và payload sai: chỉ coi là token chết khi
    /// thông điệp nói về registration token, còn lại là lỗi của ta (không thử lại vô hạn).
    /// </summary>
    private static KetQuaPush PhanLoaiLoi(HttpStatusCode httpStatus, string body, string pushToken)
    {
        var (status, errorCode, message) = DocLoi(body);
        var moTa = $"{(int)httpStatus} {status} {errorCode} {message}".Replace(pushToken, "***").Trim();
        moTa = moTa.Length > 300 ? moTa[..300] : moTa;

        var ma = errorCode ?? status;
        var ket = ma switch
        {
            "UNREGISTERED" or "NOT_FOUND" => KetQuaDay.TokenChet,
            "INVALID_ARGUMENT" when message.Contains("registration token", StringComparison.OrdinalIgnoreCase) => KetQuaDay.TokenChet,
            "INVALID_ARGUMENT" or "SENDER_ID_MISMATCH" or "THIRD_PARTY_AUTH_ERROR" or "PERMISSION_DENIED" or "UNAUTHENTICATED" => KetQuaDay.LoiCauHinh,
            "QUOTA_EXCEEDED" or "UNAVAILABLE" or "INTERNAL" or "RESOURCE_EXHAUSTED" => KetQuaDay.LoiTam,
            _ => (int)httpStatus >= 500 || httpStatus == HttpStatusCode.TooManyRequests ? KetQuaDay.LoiTam
                : httpStatus is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden ? KetQuaDay.LoiCauHinh
                : httpStatus == HttpStatusCode.NotFound ? KetQuaDay.TokenChet
                : KetQuaDay.LoiCauHinh,
        };
        return new KetQuaPush(ket, Loi: moTa);
    }

    /// <summary>Đọc <c>error.status</c>, mã lỗi riêng của FCM trong <c>details</c> và thông điệp; body không phải JSON (proxy, 502) thì rỗng.</summary>
    private static (string Status, string? ErrorCode, string Message) DocLoi(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("error", out var error))
            {
                return (string.Empty, null, string.Empty);
            }

            var status = error.TryGetProperty("status", out var s) ? s.GetString() ?? string.Empty : string.Empty;
            var message = error.TryGetProperty("message", out var m) ? m.GetString() ?? string.Empty : string.Empty;
            string? errorCode = null;
            if (error.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
            {
                foreach (var d in details.EnumerateArray())
                {
                    if (d.TryGetProperty("errorCode", out var c) && c.GetString() is { Length: > 0 } code)
                    {
                        errorCode = code;
                        break;
                    }
                }
            }

            return (status, errorCode, message);
        }
        catch (JsonException)
        {
            return (string.Empty, null, string.Empty);
        }
    }
}
