import mockSafeAreaContext from 'react-native-safe-area-context/jest/mock';

process.env.EXPO_PUBLIC_API_URL ??= 'http://localhost:5020';

jest.mock('expo-secure-store', () => ({
  getItemAsync: jest.fn(async () => null),
  setItemAsync: jest.fn(async () => undefined),
  deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock('expo-image-picker', () => ({
  requestMediaLibraryPermissionsAsync: jest.fn(async () => ({
    status: 'granted',
    granted: true,
    canAskAgain: true,
  })),
  getMediaLibraryPermissionsAsync: jest.fn(async () => ({
    status: 'granted',
    granted: true,
    canAskAgain: true,
  })),
  launchImageLibraryAsync: jest.fn(async () => ({
    canceled: false,
    assets: [
      {
        uri: 'https://picsum.photos/seed/mock-pick/800/800',
        width: 800,
        height: 800,
        type: 'image',
      },
    ],
  })),
  MediaTypeOptions: {
    All: 'All',
    Images: 'Images',
    Videos: 'Videos',
  },
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


