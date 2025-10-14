import {
  Keyboard,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  TouchableWithoutFeedback,
  View,
} from "react-native";
import React, { useRef } from "react";

import AuthPageTitle from "@/features/auth/components/AuthPageTitle/AuthPageTitle";
import FormField from "@/components/ui/FormField/FormField";
import Logo from "@/features/auth/components/Logo/Logo";
import { SafeAreaView } from "react-native-safe-area-context";
import signUpStyles from "../features/auth/components/SignUp.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

const SignUp = () => {
  const colorPalette = useColorPalette();

  const nameRef = useRef<TextInput>(null);
  const emailRef = useRef<TextInput>(null);
  const passRef = useRef<TextInput>(null);

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

              <View style={signUpStyles.form}>
                <FormField
                  ref={nameRef}
                  label="name"
                  placeholder="Jiara Martins"
                  returnKeyType="next"
                  submitBehavior="submit"
                  onSubmitEditing={() => emailRef.current?.focus()}
                />
                <FormField
                  ref={emailRef}
                  label="email"
                  placeholder="whataGreatApp@email.com"
                  keyboardType="email-address"
                  returnKeyType="next"
                  submitBehavior="submit"
                  onSubmitEditing={() => passRef.current?.focus()}
                />
                <FormField
                  ref={passRef}
                  label="password"
                  placeholder="******"
                  secureTextEntry={true}
                  returnKeyType="done"
                  submitBehavior="blurAndSubmit"
                />

                <Pressable
                  onPress={() => {
                    /* handle sign up */
                  }}
                  style={({ pressed }) => [
                    signUpStyles.cta,
                    {
                      backgroundColor: colorPalette.primary,
                    },
                    pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] },
                  ]}
                >
                  <Text
                    style={[
                      signUpStyles.ctaText,
                      { color: colorPalette.background },
                    ]}
                  >
                    Sign up
                  </Text>
                </Pressable>
              </View>
            </ScrollView>
          </View>
        </TouchableWithoutFeedback>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

export default SignUp;
