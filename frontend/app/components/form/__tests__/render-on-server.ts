import { createSSRApp, h, type Component } from "vue";
import { renderToString } from "vue/server-renderer";
import { UApp, SidebarProvider } from "#components";

/** Renders a component the way the server does for the first response: no hydration, no mount. */
export async function renderOnServer(component: Component): Promise<string> {
  const app = createSSRApp({ render: () => h(UApp, {}, { default: () => h(SidebarProvider, {}, { default: () => h(component) }) }) });
  return renderToString(app);
}
