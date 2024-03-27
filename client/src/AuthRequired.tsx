import { User } from "oidc-client-ts";
import { FC, ReactNode, useEffect } from "react";
import { useAuth } from "react-oidc-context";

export const AuthRequired: FC<{ children: ReactNode }> = ({ children }) => {
  const auth = useAuth();

  const setCookie = (user: User) => {
    const token = user?.access_token;
    const expiresDate = new Date(0);
    expiresDate.setUTCSeconds(user.expires_at ?? 0);
    document.cookie = `jwt=${token}; expires=${expiresDate.toUTCString()}; samesite=Strict; path=/api;`;
  };
  if (auth.user) setCookie(auth.user);

  useEffect(() => {
    if (
      !auth.activeNavigator &&
      !auth.isLoading &&
      !auth.error &&
      !auth.isAuthenticated
    ) {
      auth.signinRedirect();
    }
  });

  if (auth.error && auth.error.message === "Failed to fetch") {
    auth.signinRedirect();
  }

  switch (auth.activeNavigator) {
    case "signinSilent":
      return <div>Signing you in...</div>;
    case "signoutRedirect":
      return <div>Signing you out...</div>;
  }

  if (auth.isLoading) {
    return <div>Loading...</div>;
  }

  if (auth.error) {
    return <div>Oops... {auth.error.message}</div>;
  }

  if (auth.isAuthenticated) {
    return <>{children}</>;
  }

  return <button onClick={() => void auth.signinRedirect()}>Log in</button>;
}

