import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useSearchParams } from "react-router";
import type { AppointmentDto } from "../../../services/api/AppointmentApi.ts";
import {
  appointmentsApi,
  AppointmentState,
} from "../../../services/api/AppointmentApi.ts";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";
import { AppointmentCard } from "../common/Appointment/AppointmentCard.tsx";
import { TitledCard } from "../common/TitledCard.tsx";
import { PaginatedListView } from "../common/PaginatedListView.tsx";
import { useUpdateUrlParams } from "../specific/doctors/DoctorsPage.tsx";
import {PAGE_SIZE, PatientCard, type PatientDto} from "./types.tsx";

interface PatientCardByIdProps {
  id: string;
}

export function PatientCardById({ id }: PatientCardByIdProps) {
  const [patient, setPatient] = useState<PatientDto | null>(null);

  useEffect(() => {
    profilesApi.getPatient(id).then((result) => {
      if (result.type === "ok") setPatient(result.value);
    });
  }, []);

  if (patient == null) return <></>;

  return (
    <TitledCard title="Patient">
      <PatientCard patient={patient} />
      <PatientRecentAppointments id={patient.accountId} />
    </TitledCard>
  );
}

export function DoctorScheduledAppointment() {
  const [searchParams] = useSearchParams();
  const [appointment, setAppointment] = useState<AppointmentDto>();

  const targetId = searchParams.get("id") || null;

  useEffect(() => {
    if (targetId == null) return;

    appointmentsApi.getMyDoctorAppointmentById(targetId).then((result) => {
      if (result.type === "ok") setAppointment(result.value);
    });
  }, [targetId]);

  return (
    <div>
      {appointment && (
        <>
          <AppointmentCard appointment={appointment} showResultLink={true}/>
          <PatientCardById id={appointment.patientAccountId} />
        </>
      )}
    </div>
  );
}

interface PatientRecentAppointmentsProps {
  id: string;
}

function PatientRecentAppointments({ id }: PatientRecentAppointmentsProps) {
  const { searchParams, updateUrlParams } = useUpdateUrlParams();
  const currentPage = Number(searchParams.get("page")) || 1;

  const fetchAppointments = async (page: number) => {
    const result = await appointmentsApi.getAppointments(
      AppointmentState.Confirmed,
      id,
      page,
      PAGE_SIZE,
    );

    if (result.type === "ok") {
      return { items: result.value.items, total: result.value.totalCount };
    }

    return {
      items: [],
      total: 0,
      error: result.error?.title || "Failed to load",
    };
  };

  return (
    <PaginatedListView
      currentPage={currentPage}
      pageSize={PAGE_SIZE}
      onPageChange={(page) => updateUrlParams({ page: String(page) })}
      fetchRequest={fetchAppointments}
      dependencies={[searchParams]}
      renderItems={(appointments) => (
        <div
          style={{
            flex: 1,
            overflowY: "auto",
            display: "flex",
            flexWrap: "wrap",
            gap: "10px",
            justifyContent: "flex-start",
            alignContent: "flex-start",
            padding: "10px",
          }}
        >
          {appointments.map((appointment: AppointmentDto) => (
            <Link
              key={appointment.id}
              to={`/my-schedule/details?id=${appointment.id}`}
              style={{ textDecoration: "none" }}
            >
              <AppointmentCard appointment={appointment} />
            </Link>
          ))}
        </div>
      )}
    />
  );
}
