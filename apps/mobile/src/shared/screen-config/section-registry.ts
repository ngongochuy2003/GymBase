import type { ComponentType } from 'react';
import { FilterChips } from '../ui-kit';

type SectionComponent = ComponentType<Record<string, unknown>>;

const registry = new Map<string, SectionComponent>();

/** Đăng ký section mới (hoặc ghi đè): `registerSection('composer', ComposerCard)`. */
export function registerSection<P extends object>(type: string, component: ComponentType<P>): void {
  registry.set(type, component as unknown as SectionComponent);
}

export function getSection(type: string): SectionComponent | undefined {
  return registry.get(type);
}

// Section dùng chung có sẵn.
registerSection('filterChips', FilterChips);
