import {useEffect, useMemo, useState} from "react";
import {
    DateOnly,
    type ServiceDto,
    servicesApi,
    type SpecializationDto,
    type TimeSlotWindow
} from "../../../services/api/ServicesApi.ts";
import {type OfficeDto, officesApi} from "../../../services/api/OfficesApi.ts";
import {profilesApi} from "../../../services/api/ProfilesApi.ts";

import Select from 'react-select';

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

function officeToString(office: OfficeDto){
    return `${office.city} ${office.street} ${office.houseNumber}`;
}


function specializationToString(s: SpecializationDto){
    return `${s.specializationName}`;
}


function serviceToString(s: ServiceDto){
    return `${s.serviceName}`;
}

function doctorToString(doctor: DoctorProfileDto){
    return `${doctor.accountFirstName} ${doctor.accountLastName} ${doctor.accountMiddleName}`;
}


export function MakeAppointmentForm(){
    const [specializations, setSpecializations] = useState<SpecializationDto[]>([]);
    const [services, setServices] = useState<ServiceDto[]>([]);
    const [offices, setOffices] = useState<OfficeDto[]>([]);
    const [doctors, setDoctors] = useState<DoctorProfileDto[]>([]);
    const [timeSlots, setTimeSlots] = useState<TimeSlotWindow[]>([]);
    
    const [specialization, setSpecialization] = useState<SpecializationDto | null>(null);   
    const [service, setService] = useState<ServiceDto | null>(null);   
    const [office, setOffice] = useState<OfficeDto | null>(null);
    const [doctor, setDoctor] = useState<DoctorProfileDto | null>(null);
    
    const [date, setDate] = useState<DateOnly | null>(null);
    const [timeSlot, setTimeSlot] = useState<number | null>(null);

    useEffect(() => {
        servicesApi.getSpecializations().then(result => {
            if (result.type === "ok") setSpecializations(result.value.specializations);
        });
        officesApi.getOffices().then(result => {
            if (result.type === "ok") setOffices(result.value.offices);
        });
    }, []);

    useEffect(() => {
        if (!specialization) {
            setServices([]);
            setService(null);
            return;
        }
        servicesApi.getServices(undefined, specialization.id).then(result => {
            if (result.type === "ok") setServices(result.value.services);
        });
    }, [specialization]);
    
    useEffect(() => {
        if (!service) {
            setDoctors([]);
            setDoctor(null);
            return;
        }
        profilesApi.getDoctors({ specializationId: Number(service.specializationId) }).then(result => {
            if (result.type === "ok") setDoctors(result.value.items);
            
            console.log(result)
        });
    }, [service]);


    useEffect(() => {
        if (!doctor || !date) {
            setTimeSlots([]);
            setTimeSlot(null);
            return;
        }
        
        servicesApi.getAvailableDoctorSlots(doctor.accountId, date).then(result =>{
            if (result.type === "ok") setTimeSlots(result.value);
        });
    }, [doctor, date]);
    
    return (
      <div>
          <SearchableSelect options={specializations} 
                  getLabel={specializationToString} 
                  label="Specialization" 
                  getKey={o=>o.id} 
                  onChange={setSpecialization} 
                  value={specialization}></SearchableSelect>
          
          <SearchableSelect options={offices}
                  getLabel={officeToString}
                  label="Office"
                  getKey={o=>o.id}
                  onChange={setOffice}
                  value={office}></SearchableSelect>


          <SearchableSelect options={services}
                  getLabel={serviceToString}
                  label="Service"
                  getKey={o=>o.id}
                  onChange={setService}
                  value={service}></SearchableSelect>


          <SearchableSelect options={doctors}
                  getLabel={doctorToString}
                  label="Doctor"
                  getKey={o=>o.accountId}
                  onChange={setDoctor}
                  value={doctor}></SearchableSelect>

      </div>  
    );
}
interface SearchableSelectProps<T extends {}> {
    label: string;
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
        return options.map(item => ({
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
            <label style={{ display: 'block', marginBottom: '4px' }}>{label}</label>
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