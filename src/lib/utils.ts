import type { ClassValue } from "clsx";
import { clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function firstLetterToUpperCase(str: string) {
  if (!str) return str;
  return str.charAt(0).toUpperCase() + str.slice(1);
}
