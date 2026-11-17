import { defineVitestConfig } from "@nuxt/test-utils/config";

export default defineVitestConfig({
  test: {
    environment: "nuxt",
    reporters: ["default", "junit"],
    outputFile: {
      junit: "./test-results/junit.xml",
    },
    coverage: {
      provider: "v8",
      reporter: ["text-summary", "text", "cobertura", "html"],
      reportsDirectory: "./coverage",
      // Logic only: Vue single-file components are excluded on purpose,
      // most of their body is template rendering rather than logic.
      include: [
        "composables/**/*.ts",
        "utils/**/*.ts",
        "middleware/**/*.ts",
        "server/**/*.ts",
        "lib/**/*.ts",
      ],
      exclude: [
        "**/*.vue",
        "**/*.d.ts",
        "**/__tests__/**",
        "**/*.test.ts",
        "**/*.spec.ts",
      ],
      all: true,
      thresholds: {
        lines: 90,
        statements: 90,
        functions: 90,
        branches: 90,
      },
    },
  },
});
