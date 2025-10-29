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
import { SafeAreaView } from "react-native-safe-area-context";
import VerifyEmailForm from "@/features/auth/components/VerifyEmailForm/VerifyEmailForm";
import signUpStyles from "@/features/auth/components/SignUp.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useLocalSearchParams } from "expo-router";

const VerifyEmail = () => {
  const colorPalette = useColorPalette();
  const {email} = useLocalSearchParams<{email:string}>();

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
                title={"Enter Code"}
                description={`Check ${email}. Resend again if there is no soul`}
              />

              {/* Form */}
              <VerifyEmailForm email={email} />
            </ScrollView>
          </View>
        </TouchableWithoutFeedback>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
};

export default VerifyEmail;
