export function isEmptyObject(value: unknown) {
  if (!isObject(value)) {
    return false;
  }
  return Object.keys(value).length === 0;
}

