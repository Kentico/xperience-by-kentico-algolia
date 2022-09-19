import React from 'react';
import { LocalizationContextType } from './Localization.types';

export const LocalizationContext = React.createContext<LocalizationContextType>({} as LocalizationContextType);
LocalizationContext.displayName = 'LocalizationContext';

export const useLocalization = () => React.useContext(LocalizationContext);