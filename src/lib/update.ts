export const RELEASES_LATEST_URL =
  "https://github.com/nogiszd/xiaomi-note-exporter/releases/latest";

export function normalizeVersion(value: string): string {
  return value.trim().replace(/^v/i, "");
}

function parseVersionParts(version: string): number[] | null {
  const normalized = normalizeVersion(version);
  if (!normalized) {
    return null;
  }

  const parts = normalized.split(".");
  if (parts.length === 0) {
    return null;
  }

  const parsed = parts.map((part) => Number.parseInt(part, 10));
  if (parsed.some((part) => !Number.isFinite(part))) {
    return null;
  }

  return parsed;
}

export function compareVersions(left: string, right: string): number | null {
  const leftParts = parseVersionParts(left);
  const rightParts = parseVersionParts(right);
  if (!leftParts || !rightParts) {
    return null;
  }

  const maxLength = Math.max(leftParts.length, rightParts.length);

  for (let index = 0; index < maxLength; index += 1) {
    const leftValue = leftParts[index] ?? 0;
    const rightValue = rightParts[index] ?? 0;
    if (leftValue > rightValue) {
      return 1;
    }
    if (leftValue < rightValue) {
      return -1;
    }
  }

  return 0;
}
