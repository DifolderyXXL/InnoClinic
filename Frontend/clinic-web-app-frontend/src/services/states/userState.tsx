import React, { createContext, useContext, useEffect, useState } from "react";
import bffFetch from "../bffFetch";
import { logoutUrl, userUrl } from "../bffEndpoints";
import { silentLogin } from "../silentLoginSelector";
import {type AuthState, type ClaimItem, User} from "./states.ts";

export type AuthContextType = {
  state: AuthState;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Превращает массив клеймов [ {type: "name", value: "Alex"} ] в объект { name: "Alex" }
const simplify = (data: ClaimItem[]): Record<string, any> => {
  return data.reduce((acc, item) => {
    if (item.type && item.value !== undefined) {
      acc[item.type] = item.value;
    }
    return acc;
  }, {} as Record<string, any>);
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [state, setState] = useState<AuthState>({ status: "loading" });

  const setAsLoading = () => {
    setState({ status: "loading" });
  };

  const setAsAuthorized = (claims: ClaimItem[]) => {
    const profile = simplify(claims);

    const logoutUrlClaim = claims.find((c) => c.type === "bff:logout_url");
    const logoutUrlValue = logoutUrlClaim ? String(logoutUrlClaim.value) : undefined;

    const userInstance = new User(claims, profile, logoutUrlValue);
    setState({
      status: "authorized",
      data: userInstance
    });
  };

  const setAsUnauthorized = (error?: Error) => {
    setState({ status: "unauthorized", error });
  };
  
  useEffect(() => {
    async function checkAuth() {
      try {
        setAsLoading();
        const resp = await bffFetch(userUrl);

        if (resp.ok) {
          const claims: ClaimItem[] = await resp.json();
          
          setAsAuthorized(claims)
          return;
        } 
        
        if (resp.status === 401) {
          const silentLoginResult = await silentLogin();

          if (silentLoginResult) {
            window.location.reload();
          }
          else{
            setAsUnauthorized();
          }
          
          return;
        }

        setAsUnauthorized();
      } catch (e) {
        const err = e instanceof Error ? e : new Error(String(e));
        setAsUnauthorized(err);
        console.error(e);
      }
    }
    checkAuth();
  }, []);

  const logout = async () => {
    const targetUrl = state.status === "authorized" && state.data.logoutUrl
        ? state.data.logoutUrl
        : logoutUrl;

    setAsUnauthorized();

    window.location.href = targetUrl;
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
