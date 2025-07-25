import { Alert, StyleSheet, View, useColorScheme } from "react-native";
import { DefaultTheme, Theme } from "@react-navigation/native";

import AuthForm from "./AuthForm";
import { Colors } from "@/config/constants/Colors";
import FlatButton from "../ui/FlatButton";
import { useCustomTheme } from "@/hooks/useCustomTheme";
import { useNavigation } from "@react-navigation/native";
import { useState } from "react";

type AuthContentProps = {
  isLogin: boolean;
  onAuthenticate: (credentials: { email: string; password: string }) => void;
};
const AuthContent = ({ isLogin, onAuthenticate }: AuthContentProps) => {
  const theme = useCustomTheme();
  const [credentialsInvalid, setCredentialsInvalid] = useState({
    email: false,
    password: false,
    confirmEmail: false,
    confirmPassword: false,
  });

  function switchAuthModeHandler() {
    // Todo
  }

  function submitHandler(credentials: {
    email: string;
    password: string;
    confirmEmail?: string;
    confirmPassword?: string;
  }) {
    let { email, confirmEmail, password, confirmPassword } = credentials;

    email = email.trim();
    password = password.trim();

    const emailIsValid = email.includes("@");
    const passwordIsValid = password.length > 6;
    const emailsAreEqual = email === confirmEmail;
    const passwordsAreEqual = password === confirmPassword;

    if (
      !emailIsValid ||
      !passwordIsValid ||
      (!isLogin && (!emailsAreEqual || !passwordsAreEqual))
    ) {
      Alert.alert("Invalid input", "Please check your entered credentials.");
      setCredentialsInvalid({
        email: !emailIsValid,
        confirmEmail: !emailIsValid || !emailsAreEqual,
        password: !passwordIsValid,
        confirmPassword: !passwordIsValid || !passwordsAreEqual,
      });
      return;
    }
    onAuthenticate({ email, password });
  }

  return (
    <View style={[styles.authContent, dynamicStyles(theme).authContent]}>
      <AuthForm
        isLogin={isLogin}
        onSubmit={submitHandler}
        credentialsInvalid={credentialsInvalid}
      />
      <View style={styles.buttons}>
        <FlatButton onPress={switchAuthModeHandler}>
          {isLogin ? "Create a new user" : "Log in instead"}
        </FlatButton>
      </View>
    </View>
  );
};

export default AuthContent;

const styles = StyleSheet.create({
  authContent: {
    marginTop: 64,
    marginHorizontal: 32,
    padding: 16,
    borderRadius: 8,
    elevation: 2,
    shadowColor: "black",
    shadowOffset: { width: 1, height: 1 },
    shadowOpacity: 0.35,
    shadowRadius: 4,
  },
  buttons: {
    marginTop: 8,
  },
});

const dynamicStyles = (theme: Theme) =>
  StyleSheet.create({
    authContent: {
      backgroundColor: theme.colors.background,
    },
  });
