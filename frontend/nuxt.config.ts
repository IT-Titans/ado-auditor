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
    // Declared here and not in a file of their own. nuxt-feature-flags copies its options
    // into the runtime configuration before it reads a `config` file. Version 1.1.1 still
    // imported that file on the server; 1.1.7 answers from the runtime configuration only,
    // so flags from such a file never arrive and every flag reads as off without an error.
    flags: {
      pats: true,
      users: true,
      projects: false,
      groups: false,
      experimentalFeature: true,
    },
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
  icon: {
    // Nuxt UI asks for the icon styles in the CSS layer "base", where a width or height
    // utility overrides the default of 1em. This application was laid out without that:
    // the icon styles sat outside every layer, so an icon is as large as the font size
    // around it and classes like size-6 on it have no effect. The empty name keeps it
    // that way. Removing this line makes those classes apply and resizes many icons.
    cssLayer: "",
  },
});
