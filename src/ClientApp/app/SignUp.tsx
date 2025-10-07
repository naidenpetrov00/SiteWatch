import {
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
  useColorScheme,
} from "react-native";

import AuthPageTitle from "@/features/auth/components/AuthPageTitle/AuthPageTitle";
import { Colors } from "@/config/constants/Colors";
import Logo from "@/features/auth/components/Logo/Logo";
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import { router } from "expo-router";
import signUpStyles from "../features/auth/components/SignIn.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function SignUp() {
  const colorPalette = useColorPalette();
  return (
    <SafeAreaView
      style={[signUpStyles.safe, { backgroundColor: colorPalette.background }]}
    >
      <View
        style={[
          signUpStyles.container,
          { backgroundColor: colorPalette.background },
        ]}
      >
        {/* Logo badge */}
        <Logo/>
        
        {/* Title + subtitle */}
        <AuthPageTitle title={"Create new\n Account"} description="Already Registered? Log in"/>

        {/* Form */}
        <View style={signUpStyles.form}>
          <Text style={[signUpStyles.label, { color: colorPalette.secondary }]}>
            NAME
          </Text>
          <TextInput
            placeholder="Jiara Martins"
            placeholderTextColor={colorPalette.icon}
            style={[
              signUpStyles.input,
              {
                backgroundColor: colorPalette.secondary,
                color: colorPalette.text,
              },
            ]}
          />

          <Text
            style={[
              signUpStyles.label,
              { marginTop: 16, color: colorPalette.secondary },
            ]}
          >
            EMAIL
          </Text>
          <TextInput
            placeholder="hello@reallygreatsite.com"
            placeholderTextColor={colorPalette.icon}
            keyboardType="email-address"
            autoCapitalize="none"
            style={[
              signUpStyles.input,
              {
                backgroundColor: colorPalette.secondary,
                color: colorPalette.text,
              },
            ]}
          />

          <Text style={[signUpStyles.label, { color: colorPalette.secondary }]}>
            PASSWORD
          </Text>
          <TextInput
            placeholder="•••••"
            placeholderTextColor={colorPalette.icon}
            secureTextEntry
            style={[
              signUpStyles.input,
              {
                backgroundColor: colorPalette.secondary,
                color: colorPalette.text,
              },
            ]}
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
              style={[signUpStyles.ctaText, { color: colorPalette.background }]}
            >
              Sign up
            </Text>
          </Pressable>
        </View>

        {/* Footer link */}
        <View style={signUpStyles.footer}>
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
        </View>
      </View>
    </SafeAreaView>
  );
}
