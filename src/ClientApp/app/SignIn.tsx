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
import {SafeAreaView} from "react-native-safe-area-context";
import SignInForm from "@/features/auth/components/SignInForm/SignInForm";
import signUpStyles from "../features/auth/components/SignUp.styles";
import {useColorPalette} from "@/hooks/useColorPalette";

const SignIn = () => {
    const colorPalette = useColorPalette();
    const op = 4;
    return (
        <SafeAreaView
            style={[signUpStyles.safe, {backgroundColor: colorPalette.background}]}
        >
            <KeyboardAvoidingView
                behavior={Platform.OS === "ios" ? "padding" : "height"}
                style={{flex: 1}}
            >
                <TouchableWithoutFeedback onPress={Keyboard.dismiss} accessible={false}>
                    <View
                        style={[
                            signUpStyles.container,
                            {backgroundColor: colorPalette.background},
                        ]}
                    >
                        {/* Logo badge */}
                        <Logo/>
                        <ScrollView
                            contentContainerStyle={{flexGrow: 1}}
                            keyboardShouldPersistTaps="handled"
                            keyboardDismissMode="interactive"
                        >
                            {/* Title + subtitle */}
                            <AuthPageTitle
                                title={"Login"}
                                description="No Account no Problem? Register here"
                                href={"/SignUp"}
                            />

                            {/* Form */}
                            <SignInForm/>
                        </ScrollView>
                    </View>
                </TouchableWithoutFeedback>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
};

export default SignIn;
