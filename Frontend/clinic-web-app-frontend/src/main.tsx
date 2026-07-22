import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { BrowserRouter, Route, Routes } from "react-router";
import { DebugPage } from "./routes/pages/DebugPage.tsx";
import { AuthProvider } from "./services/states/userState.tsx";
import {LoginPage} from "./routes/pages/Identity/LoginPage.tsx";
import {ProfilePage} from "./routes/pages/Identity/ProfilePage.tsx";
import {Layout} from "./components/Layout.tsx";
import {DoctorsPage} from "./routes/pages/specific/doctors/DoctorsPage.tsx";
import {ServicesPage} from "./routes/pages/specific/services/ServicesPage.tsx";


createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route path="/" element={<App />} />
            <Route path="/debug" element={<DebugPage />} />
            <Route path="/login" element={<LoginPage/>} />
            <Route path="/profile" element={<ProfilePage/>} />
            <Route path="/doctors" element={<DoctorsPage/>} />
            <Route path="/view-services" element={<ServicesPage/>} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  </StrictMode>,
);

