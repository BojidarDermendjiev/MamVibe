import { useEffect } from 'react';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { StripeProvider } from '@stripe/stripe-react-native';
import { SignalRProvider } from './src/contexts/SignalRContext';
import { ThemeProvider, useTheme } from './src/contexts/ThemeContext';
import { useAuthStore } from './src/store/authStore';
import RootNavigator from './src/navigation';
import {
  registerForPushNotifications,
  sendPushTokenToServer,
} from './src/services/pushNotificationService';

const STRIPE_KEY = process.env.EXPO_PUBLIC_STRIPE_KEY ?? '';

function AppInner() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const { isDark } = useTheme();

  useEffect(() => {
    if (!isAuthenticated) return;
    registerForPushNotifications()
      .then((token) => { if (token) return sendPushTokenToServer(token); })
      .catch(() => {});
  }, [isAuthenticated]);

  return (
    <>
      <StatusBar style={isDark ? 'light' : 'dark'} />
      <RootNavigator />
    </>
  );
}

export default function App() {
  return (
    <SafeAreaProvider>
      <ThemeProvider>
        <StripeProvider publishableKey={STRIPE_KEY}>
          <SignalRProvider>
            <AppInner />
          </SignalRProvider>
        </StripeProvider>
      </ThemeProvider>
    </SafeAreaProvider>
  );
}
