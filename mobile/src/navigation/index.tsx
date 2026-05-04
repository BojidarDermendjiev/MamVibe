import { NavigationContainer, DarkTheme, DefaultTheme } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Text, View, TouchableOpacity } from 'react-native';
import { useTranslation } from 'react-i18next';
import { useAuthStore } from '@/store/authStore';
import { useTheme } from '@/contexts/ThemeContext';
import LoginScreen from '@/screens/auth/LoginScreen';
import RegisterScreen from '@/screens/auth/RegisterScreen';
import ForgotPasswordScreen from '@/screens/auth/ForgotPasswordScreen';
import HomeScreen from '@/screens/main/HomeScreen';
import BrowseScreen from '@/screens/main/BrowseScreen';
import ProfileScreen from '@/screens/main/ProfileScreen';
import ItemDetailScreen from '@/screens/main/ItemDetailScreen';
import PaymentScreen from '@/screens/main/PaymentScreen';
import DashboardScreen from '@/screens/main/DashboardScreen';
import ShipmentDetailScreen from '@/screens/main/ShipmentDetailScreen';
import ChatListScreen from '@/screens/chat/ChatListScreen';
import ConversationScreen from '@/screens/chat/ConversationScreen';
import MyItemsScreen from '@/screens/main/MyItemsScreen';
import SettingsScreen from '@/screens/main/SettingsScreen';
import DonateScreen from '@/screens/main/DonateScreen';
import CreateItemScreen from '@/screens/main/CreateItemScreen';
import LeaveReviewScreen from '@/screens/main/LeaveReviewScreen';
import DoctorReviewsScreen from '@/screens/main/DoctorReviewsScreen';
import ChildFriendlyPlacesScreen from '@/screens/main/ChildFriendlyPlacesScreen';
import AdminCommunityScreen from '@/screens/main/AdminCommunityScreen';
import type { AuthStackParamList, ChatStackParamList, MainTabParamList, RootStackParamList } from './types';

export type { AuthStackParamList, ChatStackParamList, MainTabParamList, RootStackParamList };

const AuthStack = createNativeStackNavigator<AuthStackParamList>();
const MainTab   = createBottomTabNavigator<MainTabParamList>();
const RootStack = createNativeStackNavigator<RootStackParamList>();
const ChatStack = createNativeStackNavigator<ChatStackParamList>();

function ChatNavigator() {
  const { t } = useTranslation();
  return (
    <ChatStack.Navigator>
      <ChatStack.Screen
        name="ChatList"
        component={ChatListScreen}
        options={{ title: t('nav.messages'), headerTintColor: '#d4938f' }}
      />
      <ChatStack.Screen
        name="Conversation"
        component={ConversationScreen}
        options={({ route }) => ({
          title: route.params.displayName,
          headerTintColor: '#d4938f',
        })}
      />
    </ChatStack.Navigator>
  );
}

function MainTabs() {
  const { t } = useTranslation();
  const { colors } = useTheme();
  return (
    <MainTab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: '#d4938f',
        tabBarInactiveTintColor: colors.text2,
        tabBarStyle: { backgroundColor: colors.tabBar, borderTopColor: colors.tabBarBorder },
      }}
    >
      <MainTab.Screen
        name="Home"
        component={HomeScreen}
        options={{ tabBarLabel: t('nav.home'), tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>🏠</Text> }}
      />
      <MainTab.Screen
        name="Browse"
        component={BrowseScreen}
        options={{ tabBarLabel: t('nav.browse'), tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>🔍</Text> }}
      />
      <MainTab.Screen
        name="ChatTab"
        component={ChatNavigator}
        options={{
          tabBarLabel: t('nav.chat'),
          tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>💬</Text>,
        }}
      />
      <MainTab.Screen
        name="Profile"
        component={ProfileScreen}
        options={{ tabBarLabel: t('nav.profile'), tabBarIcon: ({ color }) => <Text style={{ fontSize: 20, color }}>👤</Text> }}
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
            <View style={{ width: 46, height: 46, borderRadius: 23, backgroundColor: '#d4938f', alignItems: 'center', justifyContent: 'center', marginBottom: 6 }}>
              <Text style={{ color: '#fff', fontSize: 28, lineHeight: 32, fontWeight: '300' }}>+</Text>
            </View>
          ),
        }}
      />
    </MainTab.Navigator>
  );
}

function MainNavigator() {
  const { t } = useTranslation();
  return (
    <RootStack.Navigator screenOptions={{ headerShown: false }}>
      <RootStack.Screen name="MainTabs" component={MainTabs} />
      <RootStack.Screen
        name="ItemDetail"
        component={ItemDetailScreen}
        options={{ headerShown: true, title: '', headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="Payment"
        component={PaymentScreen}
        options={{ headerShown: true, title: 'Checkout', headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="Dashboard"
        component={DashboardScreen}
        options={{ headerShown: true, title: t('profile.myOrders'), headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="ShipmentDetail"
        component={ShipmentDetailScreen}
        options={{ headerShown: true, title: 'Shipment', headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="MyItems"
        component={MyItemsScreen}
        options={{ headerShown: true, title: t('profile.myItems'), headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="Settings"
        component={SettingsScreen}
        options={{ headerShown: true, title: t('profile.settings'), headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="Donate"
        component={DonateScreen}
        options={{ headerShown: true, title: t('donate.title'), headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="CreateItem"
        component={CreateItemScreen}
        options={{ headerShown: true, title: 'New Listing', headerTintColor: '#d4938f' }}
      />
      <RootStack.Screen
        name="LeaveReview"
        component={LeaveReviewScreen}
        options={({ route }) => ({
          headerShown: true,
          title: 'Rate your experience',
          headerTintColor: '#d4938f',
          headerSubtitle: `With ${route.params.sellerName}`,
        })}
      />
      <RootStack.Screen
        name="DoctorReviews"
        component={DoctorReviewsScreen}
        options={{ headerShown: false }}
      />
      <RootStack.Screen
        name="ChildFriendlyPlaces"
        component={ChildFriendlyPlacesScreen}
        options={{ headerShown: false }}
      />
      <RootStack.Screen
        name="AdminCommunity"
        component={AdminCommunityScreen}
        options={{ headerShown: false }}
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
