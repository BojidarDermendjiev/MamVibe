import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { useAuthStore } from "../store/authStore";

export interface ListingViewDelta {
  listingId: string;
  newViewCount: number;
}

export interface ListingLikeDelta {
  listingId: string;
  newLikeCount: number;
  actorLiked: boolean;
}

export interface ListingCommentBroadcast {
  listingId: string;
  comment: {
    id: string;
    listingId: string;
    userId: string;
    authorDisplayName: string;
    authorAvatarUrl: string | null;
    body: string;
    parentCommentId: string | null;
    isHidden: boolean;
    hiddenReason: string | null;
    createdAt: string;
  };
  deleted: boolean;
}

export interface SubscriptionStatusBroadcast {
  businessProfileId: string;
  status: number;
  planCode: string;
}

type Handler<T> = (payload: T) => void;

interface BusinessHubContextValue {
  isConnected: boolean;
  onListingViewed: (h: Handler<ListingViewDelta>) => () => void;
  onListingLiked: (h: Handler<ListingLikeDelta>) => () => void;
  onListingCommented: (h: Handler<ListingCommentBroadcast>) => () => void;
  onSubscriptionStatusChanged: (h: Handler<SubscriptionStatusBroadcast>) => () => void;
}

const Ctx = createContext<BusinessHubContextValue>({
  isConnected: false,
  onListingViewed: () => () => {},
  onListingLiked: () => () => {},
  onListingCommented: () => () => {},
  onSubscriptionStatusChanged: () => () => {},
});

/**
 * Connects to <c>/hubs/business</c> when the user is authenticated. Exposes
 * subscribable streams of view / like / comment / subscription deltas for the
 * dashboard to consume — all handlers return an unsubscribe function.
 */
export function BusinessHubProvider({ children }: { children: ReactNode }) {
  const { accessToken, isAuthenticated } = useAuthStore();
  const connectionRef = useRef<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  const viewHandlers = useRef(new Set<Handler<ListingViewDelta>>());
  const likeHandlers = useRef(new Set<Handler<ListingLikeDelta>>());
  const commentHandlers = useRef(new Set<Handler<ListingCommentBroadcast>>());
  const subscriptionHandlers = useRef(new Set<Handler<SubscriptionStatusBroadcast>>());

  useEffect(() => {
    if (!isAuthenticated || !accessToken) return;

    const connection = new HubConnectionBuilder()
      .withUrl("/hubs/business", { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("ListingViewed", (delta: ListingViewDelta) => {
      viewHandlers.current.forEach((h) => h(delta));
    });
    connection.on("ListingLiked", (delta: ListingLikeDelta) => {
      likeHandlers.current.forEach((h) => h(delta));
    });
    connection.on("ListingCommented", (broadcast: ListingCommentBroadcast) => {
      commentHandlers.current.forEach((h) => h(broadcast));
    });
    connection.on("SubscriptionStatusChanged", (broadcast: SubscriptionStatusBroadcast) => {
      subscriptionHandlers.current.forEach((h) => h(broadcast));
    });

    connection
      .start()
      .then(() => setIsConnected(true))
      .catch((err) => {
        if (import.meta.env.DEV) {
          console.warn("BusinessHub connect failed", err);
        }
      });

    connectionRef.current = connection;

    return () => {
      setIsConnected(false);
      if (connection.state !== HubConnectionState.Disconnected) {
        void connection.stop();
      }
      connectionRef.current = null;
    };
  }, [accessToken, isAuthenticated]);

  const value = useMemo<BusinessHubContextValue>(
    () => ({
      isConnected,
      onListingViewed: (h) => {
        viewHandlers.current.add(h);
        return () => viewHandlers.current.delete(h);
      },
      onListingLiked: (h) => {
        likeHandlers.current.add(h);
        return () => likeHandlers.current.delete(h);
      },
      onListingCommented: (h) => {
        commentHandlers.current.add(h);
        return () => commentHandlers.current.delete(h);
      },
      onSubscriptionStatusChanged: (h) => {
        subscriptionHandlers.current.add(h);
        return () => subscriptionHandlers.current.delete(h);
      },
    }),
    [isConnected],
  );

  return <Ctx.Provider value={value}>{children}</Ctx.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useBusinessHub() {
  return useContext(Ctx);
}
