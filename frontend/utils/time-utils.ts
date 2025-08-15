/**
 * Convert the passed minutes into a human friendly duration string.
 *
 * @param minutes - The minutes that are to be converted into the duration.
 * @returns - The duration string in a human friendly format.
 */
export function formatDuration(minutes: number): string {
  const hrs = Math.floor(minutes / 60);
  const mins = minutes % 60;

  const hrsLabel = hrs === 1 ? "hour" : "hours";
  const minsLabel = mins === 1 ? "minute" : "minutes";

  if (hrs > 0 && mins > 0) return `${hrs} ${hrsLabel} and ${mins} ${minsLabel}`;
  if (hrs > 0) return `${hrs} ${hrsLabel}`;
  return `${mins} ${minsLabel}`;
}
