export const isStaging = import.meta.env.VITE_IS_STAGING === 'true';
export const showTestFeatures = import.meta.env.DEV || isStaging;
