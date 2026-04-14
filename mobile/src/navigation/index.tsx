import { NavigationContainer, DarkTheme, DefaultTheme } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Text, View, TouchableOpacity } from 'react-native';
import { useAuthStore } from '@/store/authStore';
import { useTheme } from '@/contexts/ThemeContext';
import LoginScreen from '@/screens/auth/LoginScreen';
import RegisterScreen from '@/screens/auth/RegisterScreen';
import ForgotPasswordScreen from '@/screens/auth/ForgotPasswordScreen';
import HomeScreen from '@/screens/main/HomeScreen';
import BrowseScreen from '@/screens/main/BrowseScreen';
import ProfileScreen from '@/screens/main/ProfileScreen';
import ItemDetailScreen from '@/screens/main/ItemDetailScreen';
import WalletScreen from '@/screens/main/WalletScreen';
import PaymentScreen from '@/screens/main/PaymentScreen';
import DashboardScreen from '@/screens/main/DashboardScreen';
import ShipmentDetailScreen from '@/screens/main/ShipmentDetailScreen';
import ChatListScreen from '@/screens/chat/ChatListScreen';
import ConversationScreen from '@/screens/chat/ConversationScreen';
import MyItemsScreen from '@/screens/main/MyItemsScreen';
import SettingsScreen from '@/screens/main/SettingsScreen';
import DonateScreen from '@/screens/main/DonateScreen';
import CreateItemScreen from '@/screens/main/CreateItemScreen';
import type { AuthStackParamList, ChatStackParamList, MainTabParamList, RootStackParamList } from './types';

export type { AuthStackParamList, ChatStackParamList, MainTabParamList, RootStackParamList };

const AuthStack = createNativeStackNavigator<AuthStackParamList>();
const MainTab   = createBottomTabNavigator<MainTabParamList>();
const RootStack = createNativeStackNavigator<RootStackParamList>();
const ChatStack = createNativeStackNavigator<ChatStackParamList>();

function ChatNavigator() {
  return (
    <ChatStack.Navigator>
      <ChatStack.Screen
        name="ChatList"
        component={ChatListScreen}
        options={{ title: 'Messages', headerTintColor: '#e91e8c' }}
      />
      <ChatStack.Screen
        name="Conversation"
        component={ConversationScreen}
        options={({ route }) => ({
          title: route.params.displayName,
          headerTintColor: '#e91e8c',
        })}
      />
    </ChatStack.Navigator>
  );
}

function MainTabs() {
  const { colors } = useTheme();
  return (
    <MainTab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: '#e91e8c',
        tabBarInactiveTintColor: colors.text2,
        tabBarStyle: { backgroundColor: colors.tabBar, borderTopColor: colors.tabBarBorder },
      }}
    >
      <MainTab.Screen
        name="Home"
        component={HomeScreen}
        options={{ tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>🏠</Text> }}
      />
      <MainTab.Screen
        name="Browse"
        component={BrowseScreen}
        options={{ tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>🔍</Text> }}
      />
      <MainTab.Screen
        name="ChatTab"
        component={ChatNavigator}
        options={{
          tabBarLabel: 'Chat',
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>💬</Text>,
        }}
      />
      <MainTab.Screen
        name="Profile"
        component={ProfileScreen}
        options={{ tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>👤</Text> }}
      />
      <MainTab.Screen
        name="NewListing"
        component={ProfileScreen}
        listeners={({ navigation: nav }) => ({
          tabPress: (e) => {
            e.preventDefault();
            nav.navigate('CreateItem');
          },
        })}
        options={{
          tabBarLabel: '',
          tabBarIcon: () => (
            <View style={{ width: 46, height: 46, borderRadius: 23, backgroundColor: '#e91e8c', alignItems: 'center', justifyContent: 'center', marginBottom: 6 }}>
              <Text style={{ color: '#fff', fontSize: 28, lineHeight: 32, fontWeight: '300' }}>+</Text>
            </View>
          ),
        }}
      />
    </MainTab.Navigator>
  );
}

function MainNavigator() {
  return (
    <RootStack.Navigator screenOptions={{ headerShown: false }}>
      <RootStack.Screen name="MainTabs" component={MainTabs} />
      <RootStack.Screen
        name="ItemDetail"
        component={ItemDetailScreen}
        options={{ headerShown: true, title: '', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="Wallet"
        component={WalletScreen}
        options={{ headerShown: true, title: 'Wallet', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="Payment"
        component={PaymentScreen}
        options={{ headerShown: true, title: 'Checkout', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="Dashboard"
        component={DashboardScreen}
        options={{ headerShown: true, title: 'My Orders', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="ShipmentDetail"
        component={ShipmentDetailScreen}
        options={{ headerShown: true, title: 'Shipment', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="MyItems"
        component={MyItemsScreen}
        options={{ headerShown: true, title: 'My Items', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="Settings"
        component={SettingsScreen}
        options={{ headerShown: true, title: 'Settings', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="Donate"
        component={DonateScreen}
        options={{ headerShown: true, title: 'Support MamVibe', headerTintColor: '#e91e8c' }}
      />
      <RootStack.Screen
        name="CreateItem"
        component={CreateItemScreen}
        options={{ headerShown: true, title: 'New Listing', headerTintColor: '#e91e8c' }}
      />
    </RootStack.Navigator>
  );
}

export default function RootNavigator() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const { isDark, colors } = useTheme();

  const navTheme = isDark
    ? { ...DarkTheme, colors: { ...DarkTheme.colors, background: colors.bg, card: colors.header } }
    : { ...DefaultTheme, colors: { ...DefaultTheme.colors, background: colors.bg, card: colors.header } };

  return (
    <NavigationContainer theme={navTheme}>
      {isAuthenticated ? (
        <MainNavigator />
      ) : (
        <AuthStack.Navigator screenOptions={{ headerShown: false }}>
          <AuthStack.Screen name="Login" component={LoginScreen} />
          <AuthStack.Screen name="Register" component={RegisterScreen} />
          <AuthStack.Screen name="ForgotPassword" component={ForgotPasswordScreen} />
        </AuthStack.Navigator>
      )}
    </NavigationContainer>
  );
}
