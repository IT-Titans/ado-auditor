import { afterEach, vi } from "vitest";
import { mountSuspended } from "@nuxt/test-utils/runtime";
import { h, type Component } from "vue";
import { UApp } from "#components";

type Mounted = Awaited<ReturnType<typeof mountSuspended>>;
const mounted: Mounted[] = [];

/**
 * Mounts a component inside UApp, which supplies the tooltip and toast providers the forms need.
 *
 * The icon component is stubbed: the real one asks the icon API for each icon and keeps retry
 * timers alive that outlive the test file. Every wrapper is unmounted after its test, which
 * also stops the toast timer a successful submit starts.
 */
export async function mountInApp(component: Component): Promise<Mounted> {
  const wrapper = await mountSuspended(
    { render: () => h(UApp, {}, { default: () => h(component) }) },
    { attachTo: document.body, global: { stubs: { UIcon: true } } }
  );
  mounted.push(wrapper);
  return wrapper;
}

// UForm validates on input with a debounce of 300 ms (vueuse). Let it run out while the form is still
// mounted; otherwise it fires after the test environment is gone and fails the whole run.
const FORM_DEBOUNCE_MS = 350;

afterEach(async () => {
  if (mounted.length) await new Promise((resolve) => setTimeout(resolve, FORM_DEBOUNCE_MS));
  for (const wrapper of mounted.splice(0)) wrapper.unmount();
  document.body.innerHTML = "";
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});
