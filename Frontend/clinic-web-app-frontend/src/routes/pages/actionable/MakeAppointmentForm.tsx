import { useEffect, useMemo, useState } from "react";
import {
    type AvailablePositionsOnDay,
    DateOnly, dateToDateOnly,
    type ServiceDto,
    servicesApi,
    type SpecializationDto
} from "../../../services/api/ServicesApi.ts";
import { type OfficeDto } from "../../../services/api/OfficesApi.ts";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";

import Select from 'react-select';

import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { appointmentsApi } from "../../../services/api/AppointmentApi.ts";
import { TimeSlotPicker } from "./TimeSlotPicker.tsx";
import {OfficeInputFilter, PatientInputFilter, SpecializationInputFilter} from "../Shared/Inputs/OfficeInputFilter.tsx";
import { useUpdateUrlParams } from "../specific/doctors/DoctorsPage.tsx";
import "./MakeAppointmentForm.css";

export interface DoctorProfileDto {
    accountId: string;
    accountFirstName: string;
    accountLastName: string;
    accountMiddleName: string | null;
    accountPhotoId: string | null;
    photoUrl: string | null;
    dateOfBirth: DateOnly;
    specializationId: number;
    specializationSpecializationName: string;
    officeId: number;
    careerStartYear: number;
}

export interface PatientProfileDto {
    id?: string;
    accountId?: string;
    firstName?: string;
    lastName?: string;
    middleName?: string | null;
    email?: string;
    accountFirstName?: string;
    accountLastName?: string;
    accountMiddleName?: string | null;
    accountEmail?: string;
}

export interface MakeAppointmentFormProps {
    isAdmin?: boolean;
    initialPatientId?: string;
    onSuccess?: () => void;
}

function serviceToString(s: ServiceDto) {
    return `${s.serviceName}`;
}

function doctorToString(doctor: DoctorProfileDto) {
    return `${doctor.accountFirstName} ${doctor.accountLastName} ${doctor.accountMiddleName || ''}`.trim();
}

export function MakeAppointmentForm({ isAdmin = false, initialPatientId, onSuccess }: MakeAppointmentFormProps) {
    const { searchParams, updateUrlParams } = useUpdateUrlParams();

    const urlOfficeId = searchParams.get("officeId");
    const urlSpecId = searchParams.get("specId") ? Number(searchParams.get("specId")) : null;
    const urlServiceId = searchParams.get("serviceId") ? Number(searchParams.get("serviceId")) : null;
    const urlDoctorId = searchParams.get("doctorId");
    const urlUserId = searchParams.get("userId") || initialPatientId;

    const [bookingStatus, setBookingStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const [services, setServices] = useState<ServiceDto[]>([]);
    const [doctors, setDoctors] = useState<DoctorProfileDto[]>([]);
    const [timeSlots, setTimeSlots] = useState<AvailablePositionsOnDay | null>(null);

    const [, setSpecialization] = useState<SpecializationDto | null>(null);
    const [service, setService] = useState<ServiceDto | null>(null);
    const [, setOffice] = useState<OfficeDto | null>(null);
    const [doctor, setDoctor] = useState<DoctorProfileDto | null>(null);
    const [patient, setPatient] = useState<PatientProfileDto | null>(null);

    const [date, setDate] = useState<Date | null>(null);
    const [timeSlot, setTimeSlot] = useState<number | null>(null);

    useEffect(() => {
        if (!urlServiceId) { setService(null); return; }
        servicesApi.getService(urlServiceId).then(res => {
            if (res.type === "ok") setService(res.value);
        });
    }, [urlServiceId]);

    useEffect(() => {
        if (!urlDoctorId) { setDoctor(null); return; }
        profilesApi.getDoctorById(urlDoctorId).then(res => {
            if (res.type === "ok") setDoctor(res.value);
        });
    }, [urlDoctorId]);

    useEffect(() => {
        if (!urlSpecId) { setServices([]); return; }
        servicesApi.getServices(undefined, urlSpecId).then(res => {
            if (res.type === "ok") setServices(res.value.services);
        });
    }, [urlSpecId]);

    useEffect(() => {
        if (!urlSpecId) { setDoctors([]); return; }
        profilesApi.getDoctors({
            specializationIds: [urlSpecId],
            officeIds: urlOfficeId ? [urlOfficeId] : []
        }).then(res => {
            if (res.type === "ok") setDoctors(res.value.items);
        });
    }, [urlSpecId, urlOfficeId]);

    useEffect(() => {
        if (!urlDoctorId || !date || !patient) { setTimeSlots(null); return; }
        servicesApi.getAvailableDoctorSlots(urlDoctorId, date, patient?.accountId).then(res => {
            if (res.type === "ok") setTimeSlots(res.value);
        });
    }, [urlDoctorId, date, patient]);

    async function BookAnAppointment() {
        const targetPatientId = patient?.accountId || patient?.id || urlUserId;
        if (!urlDoctorId || !urlOfficeId || !date || timeSlot === -1 || timeSlot === null || !urlServiceId || !urlSpecId) return;
        if (isAdmin && !targetPatientId) return;

        setBookingStatus('loading');
        setErrorMessage(null);

        const command = {
            doctorAccountId: urlDoctorId,
            officeId: urlOfficeId,
            date: dateToDateOnly(date),
            startSlotIndex: timeSlot,
            serviceId: urlServiceId,
            specializationId: urlSpecId
        };

        try {
            const result = isAdmin
                ? await appointmentsApi.bookAppointmentForUser(targetPatientId!, command)
                : await appointmentsApi.bookAppointment(command);

            if (result.type === "ok") {
                setBookingStatus('success');
                onSuccess?.();
            } else {
                setBookingStatus('error');
                setErrorMessage(result.error?.title || 'Cant book an appointment');
            }
        } catch {
            setBookingStatus('error');
            setErrorMessage('Unhandled exception');
        }
    }

    const isSubmitDisabled = !(
        service &&
        timeSlot !== -1 &&
        timeSlot !== null &&
        doctor &&
        date &&
        urlOfficeId &&
        urlSpecId &&
        (!isAdmin || patient || urlUserId) &&
        bookingStatus !== 'loading'
    );

    return (
        <div className="appointment-form-container">
            <h2 className="appointment-form-title">
                {isAdmin ? "Book an Appointment for Patient" : "Book an Appointment"}
            </h2>

            {isAdmin && (
                <div className="form-field">
                    <PatientInputFilter
                        label="Patient"
                        valueId={urlUserId}
                        onChange={p => {
                            setPatient(p);
                            updateUrlParams({ userId: p?.accountId || p?.id || null });
                        }}
                    />
                </div>
            )}

            <div className="form-field">
                <SpecializationInputFilter
                    label="Specialization"
                    valueId={urlSpecId}
                    onChange={spec => {
                        setSpecialization(spec);
                        updateUrlParams({ specId: spec?.id ?? null, serviceId: null, doctorId: null, slot: null });
                    }}
                />
            </div>

            <div className="form-field">
                <OfficeInputFilter
                    label="Office"
                    valueId={urlOfficeId}
                    onChange={off => {
                        setOffice(off);
                        updateUrlParams({ officeId: off?.id ?? null, doctorId: null, slot: null });
                    }}
                />
            </div>

            <SearchableSelect
                options={services}
                getLabel={serviceToString}
                label="Service"
                getKey={o => o.id}
                onChange={s => updateUrlParams({ serviceId: s?.id ?? null, doctorId: null, slot: null })}
                value={service}
            />

            <SearchableSelect
                options={doctors}
                getLabel={doctorToString}
                label="Doctor"
                getKey={o => o.accountId}
                onChange={d => updateUrlParams({ doctorId: d?.accountId ?? null, slot: null })}
                value={doctor}
            />

            <div className="form-field">
                <label className="form-label">Select Date</label>
                <DatePicker
                    selected={date}
                    onChange={setDate}
                    dateFormat="yyyy-MM-dd"
                    placeholderText="Choose a date"
                    className="date-picker-input"
                    wrapperClassName="date-picker-wrapper"
                />
            </div>

            {service && timeSlots && (
                <div className="form-field">
                    <label className="form-label">Available Time Slots</label>
                    <TimeSlotPicker
                        selected={timeSlot ?? -1}
                        positions={timeSlots}
                        slotAmount={service.slotLength}
                        onChange={x => setTimeSlot(x)}
                    />
                </div>
            )}

            <button
                className="submit-book-btn"
                disabled={isSubmitDisabled}
                onClick={BookAnAppointment}
            >
                Book an appointment
            </button>

            {bookingStatus === 'success' && (
                <div className="status-message success">
                    Appointment accepted.
                </div>
            )}

            {bookingStatus === 'error' && (
                <div className="status-message error">
                    Error: {errorMessage}
                </div>
            )}

            {bookingStatus === 'loading' && (
                <div className="status-message loading">Sending...</div>
            )}
        </div>
    );
}

interface SearchableSelectProps<T extends {}> {
    label?: string;
    options: T[];
    value: T | null;
    onChange: (value: T | null) => void;
    getLabel: (item: T) => string;
    getKey: (item: T) => string | number;
    placeholder?: string;
    isClearable?: boolean;
    disabled?: boolean;
    className?: string;
}

export function SearchableSelect<T extends {}>({
                                                   label,
                                                   options,
                                                   value,
                                                   onChange,
                                                   getLabel,
                                                   getKey,
                                                   placeholder,
                                                   isClearable = true,
                                                   disabled = false,
                                                   className,
                                               }: SearchableSelectProps<T>) {
    const selectOptions = useMemo(() => {
        return (options ?? []).map(item => ({
            value: item,
            label: getLabel(item),
            key: String(getKey(item)),
        }));
    }, [options, getLabel, getKey]);

    const currentValue = value
        ? { value, label: getLabel(value), key: String(getKey(value)) }
        : null;

    return (
        <div className={className || "searchable-select-container"}>
            {label && <label className="form-label">{label}</label>}
            <Select
                classNamePrefix="custom-select"
                options={selectOptions}
                value={currentValue}
                onChange={(selected) => onChange(selected?.value ?? null)}
                placeholder={placeholder}
                isClearable={isClearable}
                isDisabled={disabled}
                getOptionLabel={(option) => option.label}
                getOptionValue={(option) => option.key}
                filterOption={(option, inputValue) =>
                    option.label.toLowerCase().includes(inputValue.toLowerCase())
                }
                noOptionsMessage={() => 'None'}
            />
        </div>
    );
}