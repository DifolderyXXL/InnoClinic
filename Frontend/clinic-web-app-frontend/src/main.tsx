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
import {MakeAppointmentForm} from "./routes/pages/actionable/MakeAppointmentForm.tsx";
import {ClientAppointments} from "./routes/pages/specific/appointments/ClientAppointments.tsx";
import {DoctorPage} from "./routes/pages/specific/doctors/DoctorPage.tsx";
import {OfficePage, OfficesPage} from "./routes/pages/specific/offices/OfficePage.tsx";
import {ClientAppointment} from "./routes/pages/specific/appointments/ClientAppointment.tsx";
import {DoctorScheduledAppointment, MyDoctorSchedule} from "./routes/pages/DoctorPages/MyDoctorSchedule.tsx";


createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route path="/" element={<App />} />
            <Route path="/debug" element={<DebugPage />} />
            
            <Route path="/login-patient" element={<LoginPage role="client"/>} />
            <Route path="/login-doctor" element={<LoginPage role="doctor"/>} />
            <Route path="/login-receptionist" element={<LoginPage role="receptionist"/>} />
            
            <Route path="/profile" element={<ProfilePage/>} />
            
            <Route path="/doctors" element={<DoctorsPage/>} />
            <Route path="/doctors/details" element={<DoctorPage/>} />

            <Route path="/view-offices" element={<OfficesPage/>} />
            <Route path="/view-offices/details" element={<OfficePage/>} />
            
            <Route path="/view-services" element={<ServicesPage/>} />
            <Route path="/make-appointment" element={<MakeAppointmentForm/>} />
            <Route path="/my-appointments" element={<ClientAppointments/>} />
            <Route path="/my-appointments/details" element={<ClientAppointment/>} />
            
            
            <Route path="/my-schedule" element={<MyDoctorSchedule/>} />
            <Route path="/my-schedule/details" element={<DoctorScheduledAppointment/>} />
            
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  </StrictMode>,
);

