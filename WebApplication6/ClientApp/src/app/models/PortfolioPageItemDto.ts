export const PAGE_LAYOUT_PRESETS = ['default', 'cozy', 'spooky'] as const;
export type PageLayoutPreset = (typeof PAGE_LAYOUT_PRESETS)[number];
