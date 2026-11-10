import mockSafeAreaContext from 'react-native-safe-area-context/jest/mock';

jest.mock('expo-secure-store', () => ({
  getItemAsync: jest.fn(async () => null),
  setItemAsync: jest.fn(async () => undefined),
  deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock('react-native-safe-area-context', () => mockSafeAreaContext);

// Polyfill FormData cho Jest test environment (chống lỗi winter runtime trong node test)
class MockFormData {
  private _parts: { name: string; value: unknown; fileName?: string }[] = [];
  append(name: string, value: unknown, fileName?: string) {
    this._parts.push({ name, value, fileName });
  }
}
(globalThis as unknown as { FormData: unknown }).FormData = MockFormData;
