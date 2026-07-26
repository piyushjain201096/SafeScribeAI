import { invoke } from '@tauri-apps/api/core';
import { listen, EventCallback, EventName, Options, UnlistenFn } from '@tauri-apps/api/event';

/**
 * Helper utility to check if the application is running inside a Tauri Desktop WebView
 * or inside a standard browser environment (e.g. Chrome/Edge dev server).
 */
export function isTauri(): boolean {
  return typeof window !== 'undefined' && (
    '__TAURI__' in window ||
    '__TAURI_INTERNALS__' in window
  );
}

/**
 * Safe wrapper for Tauri's listen function.
 * In a standard browser environment (where window.__TAURI_INTERNALS__ is undefined),
 * this returns a dummy unlisten function instead of throwing a TypeError.
 */
export async function safeListen<T>(
  event: EventName,
  handler: EventCallback<T>,
  options?: Options
): Promise<UnlistenFn> {
  if (!isTauri()) {
    return () => {};
  }
  try {
    return await listen<T>(event, handler, options);
  } catch (err) {
    console.warn(`[safeListen] Skipped event listener for "${String(event)}":`, err);
    return () => {};
  }
}

/**
 * Safe wrapper for Tauri's invoke function.
 * In a standard browser environment (where window.__TAURI_INTERNALS__ is undefined),
 * this returns fallbackValue instead of throwing a TypeError.
 */
export async function safeInvoke<T>(
  cmd: string,
  args?: Record<string, unknown>,
  fallbackValue?: T
): Promise<T> {
  if (!isTauri()) {
    return fallbackValue as T;
  }
  try {
    return await invoke<T>(cmd, args);
  } catch (err) {
    console.warn(`[safeInvoke] Command "${cmd}" skipped or failed:`, err);
    return fallbackValue as T;
  }
}
