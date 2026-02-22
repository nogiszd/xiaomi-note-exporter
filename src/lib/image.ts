function isRemoteOrSpecialSource(value: string) {
  return (
    /^[a-zA-Z][a-zA-Z\d+\-.]*:/.test(value) ||
    value.startsWith("//") ||
    value.startsWith("#")
  );
}

function normalizeToForwardSlashes(value: string) {
  return value.replace(/\\/g, "/");
}

function toNativeFilePath(value: string) {
  if (/^[a-zA-Z]:\//.test(value)) {
    return value.replace(/\//g, "\\");
  }
  return value;
}

function dirname(value: string) {
  const normalized = normalizeToForwardSlashes(value);
  const lastSlash = normalized.lastIndexOf("/");
  if (lastSlash < 0) {
    return "";
  }
  return normalized.slice(0, lastSlash);
}

function resolveRelativePath(baseFilePath: string, relativePath: string) {
  const normalizedBase = dirname(baseFilePath);
  const rootMatch = normalizedBase.match(/^(?:[a-zA-Z]:|\/)/);
  const root = rootMatch?.[0] ?? "";

  const baseParts = normalizedBase
    .slice(root.length)
    .split("/")
    .filter((part) => part.length > 0);
  const relativeParts = normalizeToForwardSlashes(relativePath)
    .split("/")
    .filter((part) => part.length > 0);

  const parts = [...baseParts];
  for (const part of relativeParts) {
    if (part === ".") {
      continue;
    }
    if (part === "..") {
      if (parts.length > 0) {
        parts.pop();
      }
      continue;
    }
    parts.push(part);
  }

  const resolved = (() => {
    const suffix = parts.join("/");
    if (root === "/") {
      return suffix ? `/${suffix}` : "/";
    }
    if (root) {
      return suffix ? `${root}/${suffix}` : `${root}/`;
    }
    return suffix;
  })();
  return toNativeFilePath(resolved);
}

export {
  isRemoteOrSpecialSource,
  normalizeToForwardSlashes,
  toNativeFilePath,
  dirname,
  resolveRelativePath,
};
