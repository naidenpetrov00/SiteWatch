import {
  Pressable,
  SafeAreaView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";

import { Colors } from "@/config/constants/Colors";
// app/(identity)/SignUp.tsx
import React from "react";
import { router } from "expo-router";

export default function SignUp() {
  const theme = Colors.dark; 
  return (
    <SafeAreaView style={[styles.safe, { backgroundColor: theme.background }]}>
      <View style={[styles.container, { backgroundColor: theme.background }]}>
        {/* Logo badge */}
        <View style={[styles.logoBadge, { backgroundColor: theme.primary }]}>
          <Text style={[styles.logoText, { color: theme.background }]}>
            Logo
          </Text>
        </View>

        {/* Title + subtitle */}
        <View style={styles.headerWrap}>
          <Text style={[styles.title, { color: theme.text }]}>
            Create new{"\n"}Account
          </Text>
          <Text style={[styles.subtitle, { color: theme.secondary }]}>
            Already Registered? Log in here.
          </Text>
        </View>

        {/* Form */}
        <View style={styles.form}>
          <Text style={[styles.label, { color: theme.secondary }]}>NAME</Text>
          <TextInput
            placeholder="Jiara Martins"
            placeholderTextColor={theme.icon}
            style={[
              styles.input,
              { backgroundColor: theme.secondary, color: theme.text },
            ]}
          />

          <Text
            style={[styles.label, { marginTop: 16, color: theme.secondary }]}
          >
            EMAIL
          </Text>
          <TextInput
            placeholder="hello@reallygreatsite.com"
            placeholderTextColor={theme.icon}
            keyboardType="email-address"
            autoCapitalize="none"
            style={[
              styles.input,
              { backgroundColor: theme.secondary, color: theme.text },
            ]}
          />

          <Text
            style={[styles.label, { marginTop: 16, color: theme.secondary }]}
          >
            PASSWORD
          </Text>
          <TextInput
            placeholder="•••••"
            placeholderTextColor={theme.icon}
            secureTextEntry
            style={[
              styles.input,
              { backgroundColor: theme.secondary, color: theme.text },
            ]}
          />

          <Pressable
            onPress={() => {
              /* handle sign up */
            }}
            style={({ pressed }) => [
              styles.cta,
              {
                backgroundColor: theme.primary,
              },
              pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] },
            ]}
          >
            <Text style={[styles.ctaText, { color: theme.background }]}>
              Sign up
            </Text>
          </Pressable>
        </View>

        {/* Footer link */}
        <View style={styles.footer}>
          <Text style={[styles.footerMuted, { color: theme.secondary }]}>
            Already Have Account?
          </Text>
          <Pressable onPress={() => router.replace("/SignUp")}>
            <Text style={[styles.footerLink, { color: theme.text }]}>
              {"  "}Login !
            </Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  );
}

const RADIUS = 14;

const styles = StyleSheet.create({
  safe: {
    flex: 1,
  },
  container: {
    flex: 1,
    paddingHorizontal: 24,
  },
  logoBadge: {
    width: 62,
    height: 62,
    borderRadius: 16,
    alignItems: "center",
    justifyContent: "center",
    marginTop: 8,
  },
  logoText: {
    fontWeight: "700",
  },
  headerWrap: {
    marginTop: 24,
    alignItems: "center",
  },
  title: {
    fontSize: 32,
    lineHeight: 38,
    fontWeight: "800",
    textAlign: "center",
  },
  subtitle: {
    marginTop: 6,
    fontSize: 13,
    textAlign: "center",
  },
  form: {
    marginTop: 28,
  },
  label: {
    fontSize: 12,
    letterSpacing: 1,
    marginBottom: 8,
  },
  input: {
    paddingHorizontal: 16,
    paddingVertical: 14,
    borderRadius: RADIUS,
    fontSize: 16,
  },
  cta: {
    marginTop: 22,
    borderRadius: RADIUS,
    alignItems: "center",
    justifyContent: "center",
    paddingVertical: 14,
  },
  ctaText: {
    fontWeight: "700",
    fontSize: 16,
  },
  footer: {
    marginTop: 22,
    flexDirection: "row",
    justifyContent: "center",
    alignItems: "center",
  },
  footerMuted: {
    fontSize: 13,
  },
  footerLink: {
    fontSize: 13,
    fontWeight: "700",
  },
});
