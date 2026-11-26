module.exports = {
  preset: 'jest-expo',
  testMatch: ['**/__tests__/**/*.test.[jt]s?(x)'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
    '\\.css$': '<rootDir>/__tests__/style-stub.js',
    '^react-native-reanimated/mock$': '<rootDir>/__tests__/reanimated-mock.js',
    '\.css$': '<rootDir>/__tests__/style-stub.js',
  },
  // @rn-primitives phát hành mã còn JSX nên phải cho Babel biên dịch, thêm vào danh sách mặc định của jest-expo.
  transformIgnorePatterns: [
    '/node_modules/(?!(.pnpm|react-native|@react-native|@react-native-community|expo|@expo|@expo-google-fonts|react-navigation|@react-navigation|@sentry/react-native|native-base|standard-navigation|@rn-primitives))',
    '/node_modules/react-native-reanimated/plugin/',
    '/node_modules/@react-native/babel-preset/',
  ],
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  clearMocks: true,
  collectCoverageFrom: ['src/**/*.{ts,tsx}', '!src/app/**', '!src/**/index.ts'],
};
