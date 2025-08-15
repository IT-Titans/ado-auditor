export function usePageMeta() {
  const route = useRoute();

  return route.meta;
}
