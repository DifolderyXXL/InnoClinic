import {appointmentsApi} from "../../../services/api/AppointmentApi.ts";
import {AppointmentCard} from "../common/Appointment/AppointmentCard.tsx";
import {ItemDetails} from "../Shared/Layouts/ItemDetails.tsx";

export function ClinicAppointmentDetails() {
    return (
        <ItemDetails
            provider={(id) => appointmentsApi.getAppointmentById(id)}
            extractor={(res) => res.value}
        >
            {(appointment) => <AppointmentCard appointment={appointment} showResultLink={false} />}
        </ItemDetails>
    )
}

