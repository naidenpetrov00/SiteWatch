import {
  Keyboard,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  TouchableWithoutFeedback,
  View,
} from "react-native";

import AuthPageTitle from "@/features/auth/components/AuthPageTitle/AuthPageTitle";
import Logo from "@/features/auth/components/Logo/Logo";
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import SignUpForm from "@/features/auth/components/SignUpForm/SignUpForm";
import signUpStyles from "../features/auth/components/SignUp.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const SignUp = () => {
  const colorPalette = useColorPalette();

  return (
    <SafeAreaView
      style={[signUpStyles.safe, { backgroundColor: colorPalette.background }]}
    >
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        style={{ flex: 1 }}
      >
        <TouchableWithoutFeedback onPress={Keyboard.dismiss} accessible={false}>
          <View
            style={[
              signUpStyles.container,
              { backgroundColor: colorPalette.background },
            ]}
          >
            {/* Logo badge */}
            <Logo />
            <ScrollView
              contentContainerStyle={{ flexGrow: 1 }}
              keyboardShouldPersistTaps="handled"
              keyboardDismissMode="interactive"
            >
              {/* Title + subtitle */}
              <AuthPageTitle
                title={"Create new\n Account"}
                description="Already Registered? Log in here"
                href={"/SignIn"}
              />

              {/* Form */} 
              <SignUpForm />
            </ScrollView>
          </View>
        </TouchableWithoutFeedback>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

export default SignUp;
