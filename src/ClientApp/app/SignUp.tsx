import {
  Keyboard,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableWithoutFeedback,
  View,
  useColorScheme,
} from "react-native";

import AuthPageTitle from "@/features/auth/components/AuthPageTitle/AuthPageTitle";
import FormField from "@/components/ui/FormField/FormField";
import Logo from "@/features/auth/components/Logo/Logo";
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import { router } from "expo-router";
import signUpStyles from "../features/auth/components/SignUp.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function SignUp() {
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

              <View style={signUpStyles.form}>
                <FormField label="name" placeholder="Jiara Martins" />
                <FormField
                  label="email"
                  placeholder="whataGreatApp@email.com"
                  keyboardType="email-address"
                />
                <FormField
                  label="password"
                  placeholder="******"
                  secureTextEntry={true}
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
            {/* Footer link */}
            {/* <View style={signUpStyles.footer}>
            <Text
            style={[
              signUpStyles.footerMuted,
              { color: colorPalette.secondary },
              ]}
              >
              Already Have Account?
              </Text>
              <Pressable onPress={() => router.push("/SignIn")}>
              <Text
              style={[signUpStyles.footerLink, { color: colorPalette.text }]}
              >
                {"  "}Login !
                </Text>
                </Pressable>
                </View> */}
          </View>
        </TouchableWithoutFeedback>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}
