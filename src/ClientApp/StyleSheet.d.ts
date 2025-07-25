export namespace StyleSheet {
  type NamedStyles<T> = {
    [P in keyof T]:
      | ViewStyle
      | TextStyle
      | ImageStyle
      | ((...props: unknown[]) => ViewStyle | TextStyle | ImageStyle);
  };
}
