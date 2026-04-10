export type AuthStackParamList = {
  Login: undefined;
  Register: undefined;
  ForgotPassword: undefined;
};

export type ChatStackParamList = {
  ChatList: undefined;
  Conversation: {
    userId: string;
    displayName: string;
    avatarUrl: string | null;
  };
};

export type MainTabParamList = {
  Home: undefined;
  Browse: undefined;
  ChatTab: undefined;
  Profile: undefined;
};

export type RootStackParamList = {
  MainTabs: undefined;
  ItemDetail: { itemId: string };
  Wallet: undefined;
  Payment: { itemId: string };
  Dashboard: undefined;
  ShipmentDetail: { shipmentId: string };
};
