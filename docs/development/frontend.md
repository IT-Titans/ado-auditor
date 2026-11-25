# Frontend Development

The frontend is a Nuxt 4 application with Nuxt UI 4 and Tailwind CSS 4. It renders on the
server and proxies every API call to the backend through its own server routes.

> **Prerequisites**: Node.js 22.22.3 or later (or 24.15 or later, or 26), which is what Nuxt 4 requires, and `pnpm` as the package manager.  
> To install `pnpm`, visit [pnpm Installation](https://pnpm.io/installation) and follow the instructions specific to your operating system. With `corepack enable`, Node.js picks the pnpm version pinned in `package.json` by itself.

## Layout

```
frontend/
  app/                      the Vue application
    pages/ layouts/         routes and their frames
    components/             components, auto-imported
    composables/ utils/     auto-imported logic
    middleware/             route middleware
    assets/ constants/ lib/ types/
    app.vue  error.vue  app.config.ts
  server/                   runs in Nitro only
    api/                    routes that forward to the backend
    middleware/  utils/
  shared/types/             types of the backend responses, auto-imported in app/ and server/
  public/                   files served as they are
  nuxt.config.ts  vitest.config.ts
```

`~` and `@` point to `app/`, `#shared` to `shared/`.

## Setup Steps

1. **Navigate to the Frontend Directory**
   After cloning the repository, open your terminal and move into the `frontend` folder:
   ```bash
   cd frontend
   ```
2. **Configure Environment Variables**
   Copy the `.env.sample` file to `.env`:
   ```bash
   cp .env.sample .env
   ```
   Edit the `.env` file and set the `NUXT_API_URL` based on your backend setup:
   - **Local API**:
     ```env
     NUXT_API_URL="http://localhost:5279"
     ```
   - **Dockerized API** (replace `<PORT>` with the actual Docker API port):
     ```env
     NUXT_API_URL="http://localhost:<PORT>"
     ```
3. **Install Dependencies**

   ```bash
   pnpm install
   ```
4. **Start Development Server**
   ```bash
   pnpm dev
   ```
   Open your browser at:
   ```
   http://localhost:3000
   ```

## Checks

The pipeline runs these three in this order, and a change has to pass all of them:

```bash
pnpm typecheck
pnpm lint
pnpm vitest run --coverage
```

`pnpm typecheck` checks `app/`, `server/` and `shared/` as separate TypeScript projects,
each against its own environment. Coverage is measured over the logic in
`app/composables`, `app/utils`, `app/middleware`, `app/lib` and `server`; below 90 % the
test run fails.

## Tests

The tests live next to the code in `__tests__` folders and run as two Vitest projects:

- **server** covers `server/` in the plain Node environment. That code never runs inside
  the Vue application. What Nitro auto-imports (`defineEventHandler`, `useRuntimeConfig`,
  `$fetch` and so on) is not there in a unit test, so a test provides it with
  `vi.stubGlobal`.
- **app** covers `app/` inside a running Nuxt application, which `@nuxt/test-utils` starts
  once per test file.

Three things to know when writing a test for `app/`:

- The application starts after the mocks of the file are registered. A `mockNuxtImport`
  for something Nuxt itself needs while starting (`useRouter`, `useState`,
  `useAsyncData`, `$fetch`) has to hand over to the original until a test replaces it:
  `mockNuxtImport("useRouter", (original) => mock.mockImplementation(original))`.
- `$fetch` in application code is an import, not a global. `vi.stubGlobal("$fetch", ...)`
  has no effect there; use `mockNuxtImport("$fetch", ...)`.
- Unmount what a test mounts and let its timers run out before the file ends. A toast
  (5 s), the debounce of `UForm` (300 ms) or an icon that is still loading otherwise fires
  after the environment is gone and fails the run with `window is not defined`, on the
  build agent more often than locally. `components/form/__tests__/mount-in-app.ts` does
  this for the forms.

## Forms

`UForm` hands the submit handler what the schema returns, not the state. A field that is
sent to the backend has to be part of the schema, even when nothing about it is
validated; otherwise it is dropped without a word. The start form and the contact form
each have such a field, and a test for it.
