import { defineConfig } from "vitest/config";
import { defineVitestProject } from "@nuxt/test-utils/config";

export default defineConfig({
  test: {
    projects: [
      // The server routes, middleware and utilities run in Nitro and never inside the Vue
      // application, so their tests need no Nuxt runtime and no DOM.
      {
        test: {
          name: "server",
          include: ["server/**/*.test.ts"],
          environment: "node",
        },
      },
      // Everything under app/ is tested inside a running Nuxt application.
      await defineVitestProject({
        test: {
          name: "app",
          include: ["app/**/*.test.ts"],
          environment: "nuxt",
          // @nuxt/test-utils starts the application in a beforeAll hook of every test file.
          // That takes longer than the default of 10 s on a cold cache or a small CI agent.
          hookTimeout: 120_000,
        },
      }),
    ],
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
        "app/composables/**/*.ts",
        "app/utils/**/*.ts",
        "app/middleware/**/*.ts",
        "app/lib/**/*.ts",
        "server/**/*.ts",
      ],
      exclude: [
        "**/*.vue",
        "**/*.d.ts",
        "**/__tests__/**",
        "**/*.test.ts",
        "**/*.spec.ts",
      ],
      thresholds: {
        lines: 90,
        statements: 90,
        functions: 90,
        branches: 90,
      },
    },
  },
});
