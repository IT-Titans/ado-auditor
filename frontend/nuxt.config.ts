// https://nuxt.com/docs/api/configuration/nuxt-config
import tailwindcss from "@tailwindcss/vite";

export default defineNuxtConfig({
  compatibilityDate: "2025-05-15",
  modules: [
    "@nuxt/eslint",
    "@nuxt/icon",
    "@nuxt/test-utils",
    "@nuxt/ui",
    "@compodium/nuxt",
    "@vueuse/nuxt",
    "@vee-validate/nuxt",
    "@nuxt/test-utils/module",
    "nuxt-lottie",
    "nuxt-feature-flags",
  ],
  devtools: { enabled: true },
  imports: {
    dirs: ["./types"],
  },
  app: {
    head: {
      title: "ADO Auditor",
      meta: [
        {
          name: "description",
          content: "Lights up the dark about your Azure DevOp's security.",
        },
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'icon', type: 'image/png', sizes: '16x16', href: '/favicon-16x16.png' },
        { rel: 'icon', type: 'image/png', sizes: '32x32', href: '/favicon-32x32.png' },
        { rel: 'apple-touch-icon', sizes: '180x180', href: '/apple-touch-icon.png' },
        { rel: 'icon', type: 'image/png', sizes: '192x192', href: '/android-chrome-192x192.png' },
        { rel: 'icon', type: 'image/png', sizes: '512x512', href: '/android-chrome-512x512.png' },
        { rel: 'manifest', href: '/site.webmanifest' }
      ],
    },
  },
  css: ["~/assets/css/main.css"],
  featureFlags: {
    config: "./feature-flags.config.ts",
  },
  runtimeConfig: {
    apiUrl: "",
    proxyKey: "",
    shouldRedirect: false,
    redirectToUrl: '',
    public: {
      auditExpireTime: "",
    },
  },
  vite: {
    plugins: [tailwindcss()],
  },
  colorMode: {
    preference: "light",
  },
});
