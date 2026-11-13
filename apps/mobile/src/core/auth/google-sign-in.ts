import { env } from '../config/env';

type Reason =
  'NOT_CONFIGURED' | 'NATIVE_MODULE_MISSING' | 'PLAY_SERVICES_UNAVAILABLE' | 'NO_ID_TOKEN';

/** Đăng nhập Google không dùng được trên bản này hoặc thiết bị này; `reason` để giao diện nói rõ lý do. */
export class GoogleSignInUnavailableError extends Error {
  constructor(public readonly reason: Reason) {
    super(reason);
    this.name = 'GoogleSignInUnavailableError';
  }
}

type Library = typeof import('@react-native-google-signin/google-signin');

/**
 * Nạp thư viện native khi cần. Expo Go không có module native này, và import thẳng sẽ làm cả app lỗi ngay lúc mở;
 * nạp lười để chỉ nút Google báo "cần bản dev build".
 */
function loadLibrary(): Library {
  try {
    // eslint-disable-next-line @typescript-eslint/no-require-imports
    return require('@react-native-google-signin/google-signin') as Library;
  } catch {
    throw new GoogleSignInUnavailableError('NATIVE_MODULE_MISSING');
  }
}

// TODO(IOS): mới làm Android. iOS cần client ID iOS, plugin `iosUrlScheme` trong app.json (plugin báo lỗi nếu thiếu nên
//   chưa khai báo) và theo quy định App Store thường phải có thêm Sign in with Apple.
/**
 * Ranh giới với thư viện Google Sign-In (chỉ Android, cần dev build). `getIdToken` mở hộp chọn tài khoản Google và trả
 * ID token để gửi cho `POST /auth/google`; trả null khi người dùng hủy. Thiết kế: docs/api/xac-thuc.md mục 8.
 */
export const googleSignIn = {
  /**
   * @returns ID token, hoặc null khi người dùng hủy hộp chọn tài khoản.
   * @throws GoogleSignInUnavailableError chưa cấu hình client ID, thiếu module native, thiếu Google Play Services, hoặc Google không trả ID token.
   */
  async getIdToken(): Promise<string | null> {
    const webClientId = env.googleWebClientId;
    if (!webClientId) throw new GoogleSignInUnavailableError('NOT_CONFIGURED');

    const { GoogleSignin, statusCodes, isErrorWithCode } = loadLibrary();
    try {
      // ID token cấp cho web client ID, cũng là `aud` mà backend (Google:ClientIds) kiểm tra.
      GoogleSignin.configure({ webClientId });
      await GoogleSignin.hasPlayServices();
      const response = await GoogleSignin.signIn();
      if (response.type !== 'success') return null;
      if (!response.data.idToken) throw new GoogleSignInUnavailableError('NO_ID_TOKEN');
      return response.data.idToken;
    } catch (error) {
      if (error instanceof GoogleSignInUnavailableError) throw error;
      if (isErrorWithCode(error)) {
        if (error.code === statusCodes.SIGN_IN_CANCELLED) return null;
        if (error.code === statusCodes.PLAY_SERVICES_NOT_AVAILABLE) {
          throw new GoogleSignInUnavailableError('PLAY_SERVICES_UNAVAILABLE');
        }
      }
      throw error;
    }
  },

  /** Đăng xuất tài khoản Google để lần sau hiện lại hộp chọn tài khoản; không bao giờ ném lỗi. */
  async signOut(): Promise<void> {
    try {
      await loadLibrary().GoogleSignin.signOut();
    } catch {
      /* Không có module native hoặc chưa đăng nhập Google: không có gì để đăng xuất. */
    }
  },
};
