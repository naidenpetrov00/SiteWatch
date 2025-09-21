import { Colors } from "@/config/constants/Colors";
import { NavigationContainer } from "@react-navigation/native";
import React from "react";
import SignIn from "../../app/SignIn";
import SignUp from "../../app/SignUp";
import { createNativeStackNavigator } from "@react-navigation/native-stack";

const Stack = createNativeStackNavigator();

const AuthStack = () => {
  return (
    <Stack.Navigator
      screenOptions={{
        headerStyle: { backgroundColor: Colors.light.secondary },
        headerTintColor: "white",
        contentStyle: { backgroundColor: Colors.light.background },
      }}
    >
      <Stack.Screen name="Login" component={SignIn} />
      <Stack.Screen name="Signup" component={SignUp} />
    </Stack.Navigator>
  );
};

// const AuthenticatedStack = () => {
//   return (
//     <Stack.Navigator
//       screenOptions={{
//         headerStyle: { backgroundColor: Colors.primary500 },
//         headerTintColor: "white",
//         contentStyle: { backgroundColor: Colors.primary100 },
//       }}
//     >
//       <Stack.Screen name="Welcome" component={WelcomeScreen} />
//     </Stack.Navigator>
//   );
// };

const Navigation = () => {
  return (
    <NavigationContainer>
      <AuthStack />
    </NavigationContainer>
  );
};

export default Navigation;
