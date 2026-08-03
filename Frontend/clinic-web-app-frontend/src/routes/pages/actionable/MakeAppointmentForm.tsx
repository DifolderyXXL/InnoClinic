import { useEffect, useMemo, useState} from "react";
import {
    type AvailablePositionsOnDay,
    DateOnly, dateToDateOnly,
    type ServiceDto,
    servicesApi,
    type SpecializationDto
} from "../../../services/api/ServicesApi.ts";
import {type OfficeDto} from "../../../services/api/OfficesApi.ts";
import {profilesApi} from "../../../services/api/ProfilesApi.ts";

import Select from 'react-select';

import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import {appointmentsApi} from "../../../services/api/AppointmentApi.ts";
import {TimeSlotPicker} from "./TimeSlotPicker.tsx";
import {OfficeInputFilter, SpecializationInputFilter} from "../Shared/Inputs/OfficeInputFilter.tsx";
import {useUpdateUrlParams} from "../specific/doctors/DoctorsPage.tsx";

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


function serviceToString(s: ServiceDto){
    return `${s.serviceName}`;
}

function doctorToString(doctor: DoctorProfileDto){
    return `${doctor.accountFirstName} ${doctor.accountLastName} ${doctor.accountMiddleName}`;
}


export function MakeAppointmentForm(){
    const { searchParams, updateUrlParams } = useUpdateUrlParams();

    const urlOfficeId = searchParams.get("officeId");
    const urlSpecId = searchParams.get("specId") ? Number(searchParams.get("specId")) : null;
    const urlServiceId = searchParams.get("serviceId") ? Number(searchParams.get("serviceId")) : null;
    const urlDoctorId = searchParams.get("doctorId");

    
    const [bookingStatus, setBookingStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const [services, setServices] = useState<ServiceDto[]>([]);
    const [doctors, setDoctors] = useState<DoctorProfileDto[]>([]);
    const [timeSlots, setTimeSlots] = useState<AvailablePositionsOnDay | null>(null);
    
    const [specialization, setSpecialization] = useState<SpecializationDto | null>(null);   
    const [service, setService] = useState<ServiceDto | null>(null);   
    const [office, setOffice] = useState<OfficeDto | null>(null);
    const [doctor, setDoctor] = useState<DoctorProfileDto | null>(null);
    
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
        if (!urlDoctorId || !date) { setTimeSlots(null); return; }
        servicesApi.getAvailableDoctorSlots(urlDoctorId, date).then(res => {
            if (res.type === "ok") setTimeSlots(res.value);
        });
    }, [urlDoctorId, date]);

    async function BookAnAppointment() {
        if (!urlDoctorId || !urlOfficeId || !date || timeSlot==-1 || !urlServiceId || !urlSpecId) return;

        setBookingStatus('loading');
        setErrorMessage(null);

        try {
            const result = await appointmentsApi.bookAppointment({
                doctorAccountId: urlDoctorId,
                officeId: urlOfficeId,
                date: dateToDateOnly(date),
                startSlotIndex: timeSlot,
                serviceId: urlServiceId,
                specializationId: urlSpecId
            });

            if (result.type === "ok") {
                setBookingStatus('success');
            } else {
                setBookingStatus('error');
                setErrorMessage(result.error?.title || 'Cant book an appointment');
            }
        } catch (err) {
            setBookingStatus('error');
            setErrorMessage('Unhandled exception');
        }
    }
    
    return (
      <div>
          <SpecializationInputFilter label="Specialization" valueId={urlSpecId} 
                                     onChange={spec => 
                                     {
                setSpecialization(spec);
                updateUrlParams({ specId: spec?.id ?? null, serviceId: null, doctorId: null, slot: null });
          }}/>

          <OfficeInputFilter label="Office" valueId={urlOfficeId}
                             onChange={off => {
                                 setOffice(off);
                                 updateUrlParams({ officeId: off?.id ?? null, doctorId: null, slot: null });
                             }}/>

          <SearchableSelect options={services}
                  getLabel={serviceToString}
                  label="Service"
                  getKey={o=>o.id}
                  onChange={s => updateUrlParams({ serviceId: s?.id ?? null, doctorId: null, slot: null })}
                  value={service}></SearchableSelect>


          <SearchableSelect options={doctors}
                  getLabel={doctorToString}
                  label="Doctor"
                  getKey={o=>o.accountId}
                  onChange={d => updateUrlParams({ doctorId: d?.accountId ?? null, slot: null })}
                  value={doctor}></SearchableSelect>

          <DatePicker selected={date} onChange={setDate} />

          {service && timeSlots && (<TimeSlotPicker 
              selected={timeSlot ?? -1} 
              positions={timeSlots} 
              slotAmount={service.slotLength}
              onChange={x=>setTimeSlot(x)}/> )}
          

          <button
              disabled={!(service && timeSlot != -1 && doctor && date && urlOfficeId && urlSpecId && bookingStatus !== 'loading')}
              onClick={BookAnAppointment}
          >
              Book an appointment
          </button>

          {bookingStatus === 'success' && (
              <div style={{ color: 'green', margin: '12px 0' }}>
                  Appointment accepted. 
              </div>
          )}

          {bookingStatus === 'error' && (
              <div style={{ color: 'red', margin: '12px 0' }}>
                  Error: {errorMessage}
              </div>
          )}

          {bookingStatus === 'loading' && <div>Sending...</div>}
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
}

export function SearchableSelect<T  extends {}>({
                                        label,
                                        options,
                                        value,
                                        onChange,
                                        getLabel,
                                        getKey,
                                        placeholder,
                                        isClearable = true,
                                        disabled = false,
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
        <div style={{ marginBottom: '12px' }}>
            {label && <label style={{display: 'block', marginBottom: '4px'}}>{label}</label>}
            <Select
                classNamePrefix="option-select"
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