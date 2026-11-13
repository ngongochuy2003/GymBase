import mockSafeAreaContext from 'react-native-safe-area-context/jest/mock';

// Đặt trước khi test import `@/core/config/env` (setupFilesAfterEnv chạy trước file test).
process.env.EXPO_PUBLIC_API_URL = 'http://localhost:5020';

jest.mock('expo-secure-store', () => ({
  getItemAsync: jest.fn(async () => null),
  setItemAsync: jest.fn(async () => undefined),
  deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock('react-native-safe-area-context', () => mockSafeAreaContext);

// Jest không qua metro nên NativeWind không nhận cờ `darkMode: 'class'`; đặt tay để đổi theme được.
// eslint-disable-next-line @typescript-eslint/no-require-imports
require('react-native-css-interop').StyleSheet.registerCompiled({ flags: { darkMode: 'class' } });
