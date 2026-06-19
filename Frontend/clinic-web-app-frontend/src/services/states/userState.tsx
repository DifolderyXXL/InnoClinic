import { createContext, useContext, useEffect, useState } from "react";
import bffFetch from "../bffFetch";
import { logoutUrl, userUrl } from "../bffEndpoints";
import { silentLogin } from "../silentLoginSelector";

export interface ClaimItem {
  type: string;
  value: string | object;
  valueType?: string | null;
}

export type User = {
  claims: ClaimItem[];
  logoutUrl: string | undefined;
};

type LoadingState = {
  status: "loading";
};

type AuthorizedState = {
  status: "authorized";
  data: User;
};

type UnauthorizedState = {
  status: "unauthorized";
  error?: Error;
};

export type AuthState = LoadingState | AuthorizedState | UnauthorizedState;

type AuthContextType = {
  state: AuthState;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [state, setState] = useState<AuthState>({ status: "loading" });

  useEffect(() => {
    async function checkAuth() {
      try {
        setState({ status: "loading" });
        const resp = await bffFetch(userUrl);

        if (resp.ok) {
          const claims: ClaimItem[] = await resp.json();

          // Ищем динамический logoutUrl в клеймах
          const logoutUrlClaim = claims.find(
            (claim) => claim.type === "bff:logout_url",
          );
          const logoutUrlValue =
            logoutUrlClaim == undefined
              ? undefined
              : String(logoutUrlClaim.value);

          setState({
            status: "authorized",
            data: { claims: claims, logoutUrl: logoutUrlValue },
          });
        } else if (resp.status === 401) {
          const silentLoginResult = await silentLogin();

          if (silentLoginResult) {
            window.location.reload();
          }
        }
      } catch (e) {
        let err: Error | undefined;
        if (e instanceof Error) {
          err = e;
        }
        setState({ status: "unauthorized", error: err });
        console.error(e);
      }
    }
    checkAuth();
  }, []);

  const logout = async () => {
    if (state.status == "authorized" && state.data.logoutUrl) {
      await bffFetch(state.data.logoutUrl);
    } else {
      await bffFetch(logoutUrl);
    }

    setState({ status: "unauthorized" });
  };

  return (
    <AuthContext.Provider value={{ state, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
