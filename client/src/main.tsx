import React from "react";
import { BrowserRouter } from "react-router-dom";
import ReactDOM from "react-dom/client";
import "bootstrap";
import "bootstrap-icons/font/bootstrap-icons.css";
import "./assets/custom.scss";
import App from "./App.tsx";
import { QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "react-oidc-context";
import { WebStorageStateStore } from "oidc-client-ts";
import { getQueryClient } from "./services/queryClient.tsx";
import { AuthRequired } from "./AuthRequired.tsx";
import { SignalRConnectionProvider } from "./signalR/SignalRContext.tsx";

const queryClient = getQueryClient();

const oidcConfig = {
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  authority: "https://harnesskc.duckdns.org:25651/realms/asteroids",
  client_id: "asteroids",
  redirect_uri: window.location.origin,
  response_type: "code",
  scope: "openid profile email",
  loadUserInfo: true,
};

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <AuthProvider {...oidcConfig}>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AuthRequired>
              <SignalRConnectionProvider>
                <App />
              </SignalRConnectionProvider>
          </AuthRequired>
        </BrowserRouter>
      </QueryClientProvider>
    </AuthProvider>
  </React.StrictMode>
);
