import type { ReactNode } from 'react';
import type { AppHeaderProps, SubHeaderConfig, TabItem } from '../ui-kit';

export interface SectionConfig {
  id: string;
  /** Khóa trong SectionRegistry. */
  type: string;
  props?: Record<string, unknown>;
  /** `false` để ẩn mà không xóa khỏi config. */
  visible?: boolean;
}

export type HeaderConfig = AppHeaderProps;

export interface ScreenConfig {
  id: string;
  header?: HeaderConfig;
  /** Thanh phụ (back + tiêu đề + hành động) dưới header. */
  subHeader?: SubHeaderConfig;
  sections: SectionConfig[];
  bottomTabs: TabItem[];
  activeTabId?: string;
  /** Cho phép header tự động ẩn khi vuốt xuống và từ từ hiện ra khi vuốt lên (mặc định: true khi có header). */
  collapsibleHeader?: boolean;
}

export interface ScreenRendererProps {
  config: ScreenConfig;
  onTabPress?(tab: TabItem): void;
  /** Lớp phủ như toast. */
  overlay?: ReactNode;
}
