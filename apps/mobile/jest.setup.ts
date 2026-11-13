import mockSafeAreaContext from 'react-native-safe-area-context/jest/mock';

// Đặt trước khi test import `@/core/config/env` (setupFilesAfterEnv chạy trước file test).
process.env.EXPO_PUBLIC_API_URL = 'http://localhost:5020';
process.env.EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID = 'test-web-client.apps.googleusercontent.com';

// Thư viện native của Google không chạy được trong Jest; test cụ thể tự mock lại khi cần kiểm cách gọi.
jest.mock('@react-native-google-signin/google-signin', () => ({
  GoogleSignin: {
    configure: jest.fn(),
    hasPlayServices: jest.fn(async () => true),
    signIn: jest.fn(),
    signOut: jest.fn(async () => null),
  },
  statusCodes: {
    SIGN_IN_CANCELLED: 'SIGN_IN_CANCELLED',
    PLAY_SERVICES_NOT_AVAILABLE: 'PLAY_SERVICES_NOT_AVAILABLE',
  },
  isErrorWithCode: (error: unknown) =>
    typeof error === 'object' && error !== null && 'code' in error,
}));

jest.mock('expo-secure-store', () => ({
  getItemAsync: jest.fn(async () => null),
  setItemAsync: jest.fn(async () => undefined),
  deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock('react-native-safe-area-context', () => mockSafeAreaContext);

// Jest không qua metro nên NativeWind không nhận cờ `darkMode: 'class'`; đặt tay để đổi theme được.
// eslint-disable-next-line @typescript-eslint/no-require-imports
require('react-native-css-interop').StyleSheet.registerCompiled({ flags: { darkMode: 'class' } });
