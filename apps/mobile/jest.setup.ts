import mockSafeAreaContext from 'react-native-safe-area-context/jest/mock';

jest.mock('expo-secure-store', () => ({
  getItemAsync: jest.fn(async () => null),
  setItemAsync: jest.fn(async () => undefined),
  deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock('react-native-safe-area-context', () => mockSafeAreaContext);
