import { createContext, useContext } from 'react';
import type { NativeScrollEvent, NativeSyntheticEvent } from 'react-native';

export interface CollapsibleHeaderContextValue {
  headerHeight: number;
  onScroll?: (event: NativeSyntheticEvent<NativeScrollEvent>) => void;
  isCollapsible: boolean;
}

export const CollapsibleHeaderContext = createContext<CollapsibleHeaderContextValue>({
  headerHeight: 0,
  onScroll: undefined,
  isCollapsible: false,
});

export function useCollapsibleHeaderContext(): CollapsibleHeaderContextValue {
  return useContext(CollapsibleHeaderContext);
}
