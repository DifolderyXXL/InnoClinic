import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { BrowserRouter, Route, Routes } from "react-router";
import { DebugPage } from "./routes/pages/DebugPage.tsx";
import { AuthProvider } from "./services/states/userState.tsx";
import {LoginPage} from "./routes/pages/Identity/LoginPage.tsx";
import {ProfilePage} from "./routes/pages/Identity/ProfilePage.tsx";


createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<App />} />
          <Route path="/debug" element={<DebugPage />} />
          <Route path="/login" element={<LoginPage/>} />
          <Route path="/profile" element={<ProfilePage/>} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  </StrictMode>,
);
