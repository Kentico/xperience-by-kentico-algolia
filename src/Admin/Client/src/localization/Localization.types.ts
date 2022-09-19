import * as localization from './localization.json';

export type LocalizationType = typeof localization;

export type LocalizationContextType = {
    localization: LocalizationType,
}

export interface LocalizationProviderProps {
    readonly children: React.ReactNode,
}