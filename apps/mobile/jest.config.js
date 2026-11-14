module.exports = {
  preset: 'jest-expo',
  testMatch: ['**/__tests__/**/*.test.[jt]s?(x)'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
    '\\.css$': '<rootDir>/__tests__/style-stub.js',
    '^react$': '<rootDir>/node_modules/react',
    '^react/(.*)$': '<rootDir>/node_modules/react/$1',
  },
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  clearMocks: true,
  collectCoverageFrom: ['src/**/*.{ts,tsx}', '!src/app/**', '!src/**/index.ts'],
};
