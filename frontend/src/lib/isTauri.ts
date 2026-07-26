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
