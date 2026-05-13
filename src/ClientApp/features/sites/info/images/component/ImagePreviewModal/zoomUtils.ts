export const MIN_SCALE = 1;
export const DOUBLE_TAP_SCALE = 2;
export const MAX_SCALE = 4;

export const clamp = (value: number, min: number, max: number) => {
  "worklet";
  return Math.min(Math.max(value, min), max);
};

export const getMaxTranslate = (
  renderedSize: number,
  viewportSize: number,
  currentScale: number,
) => {
  "worklet";
  return Math.max(0, (renderedSize * currentScale - viewportSize) / 2);
};
