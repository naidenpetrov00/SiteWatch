import React, { ReactNode } from "react";
import { Pressable, PressableProps, StyleProp, ViewStyle } from "react-native";

import cardStyles from "./Card.styles";

type CardProps = {
  backgroundColor: string;
  borderColor: string;
  children: ReactNode;
  onPress?: PressableProps["onPress"];
  style?: StyleProp<ViewStyle>;
};

const Card: React.FC<CardProps> = ({
  backgroundColor,
  borderColor,
  children,
  onPress,
  style,
}) => {
  return (
    <Pressable
      onPress={onPress}
      style={({ pressed }) => [
        cardStyles.card,
        { backgroundColor, borderColor },
        style,
        pressed && cardStyles.pressed,
      ]}
    >
      {children}
    </Pressable>
  );
};

export default Card;
