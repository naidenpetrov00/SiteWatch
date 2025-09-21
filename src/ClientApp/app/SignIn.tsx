import {
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
  useColorScheme,
} from "react-native";

import { Colors } from "@/config/constants/Colors";
import React from "react";
import { SafeAreaView } from "react-native-safe-area-context";
import { router } from "expo-router";
import signInStyles from "../features/auth/components/SignIn.styles";

export default function SignUp() {
  const colorScheme = useColorScheme();
  const colorPalette = colorScheme === "dark" ? Colors.dark : Colors.light;
  return (
    <SafeAreaView
      style={[signInStyles.safe, { backgroundColor: colorPalette.background }]}
    >
      <View
        style={[
          signInStyles.container,
          { backgroundColor: colorPalette.background },
        ]}
      >
        {/* Logo badge */}
        <View
          style={[
            signInStyles.logoBadge,
            { backgroundColor: colorPalette.primary },
          ]}
        >
          <Text
            style={[signInStyles.logoText, { color: colorPalette.background }]}
          >
            Logo
          </Text>
        </View>

        {/* Title + subtitle */}
        <View style={signInStyles.headerWrap}>
          <Text style={[signInStyles.title, { color: colorPalette.text }]}>
            Create new{"\n"}Account
          </Text>
          <Text
            style={[signInStyles.subtitle, { color: colorPalette.secondary }]}
          >
            Already Registered? Log in here.
          </Text>
        </View>

        {/* Form */}
        <View style={signInStyles.form}>
          <Text style={[signInStyles.label, { color: colorPalette.secondary }]}>
            NAME
          </Text>
          <TextInput
            placeholder="Jiara Martins"
            placeholderTextColor={colorPalette.icon}
            style={[
              signInStyles.input,
              {
                backgroundColor: colorPalette.secondary,
                color: colorPalette.text,
              },
            ]}
          />

          <Text
            style={[
              signInStyles.label,
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
              signInStyles.input,
              {
                backgroundColor: colorPalette.secondary,
                color: colorPalette.text,
              },
            ]}
          />

          <Text style={[signInStyles.label, { color: colorPalette.secondary }]}>
            PASSWORD
          </Text>
          <TextInput
            placeholder="•••••"
            placeholderTextColor={colorPalette.icon}
            secureTextEntry
            style={[
              signInStyles.input,
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
              signInStyles.cta,
              {
                backgroundColor: colorPalette.primary,
              },
              pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] },
            ]}
          >
            <Text
              style={[signInStyles.ctaText, { color: colorPalette.background }]}
            >
              Sign up
            </Text>
          </Pressable>
        </View>

        {/* Footer link */}
        <View style={signInStyles.footer}>
          <Text
            style={[
              signInStyles.footerMuted,
              { color: colorPalette.secondary },
            ]}
          >
            Already Have Account?
          </Text>
          <Pressable onPress={() => router.replace("/SignUp")}>
            <Text
              style={[signInStyles.footerLink, { color: colorPalette.text }]}
            >
              {"  "}Login !
            </Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  );
}
