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
  NewListing: undefined;
};

export type RootStackParamList = {
  MainTabs: undefined;
  ItemDetail: { itemId: string };
  Payment: { itemId: string };
  Dashboard: undefined;
  ShipmentDetail: { shipmentId: string };
  MyItems: undefined;
  Settings: undefined;
  Donate: undefined;
  CreateItem: undefined;
  LeaveReview: {
    paymentId: string;
    sellerName: string;
    sellerAvatarUrl: string | null;
    itemTitle: string;
    itemPrice: number | null;
  };
  DoctorReviews: undefined;
  ChildFriendlyPlaces: undefined;
  AdminCommunity: undefined;
};
