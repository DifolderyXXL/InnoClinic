import {useEffect, useMemo, useState} from "react";
import {
    type AvailablePositionsOnDay,
    DateOnly, dateToDateOnly, getSlotsInHour, minutesToTimeSpan,
    type ServiceDto,
    servicesApi,
    type SpecializationDto,
    type TimeSlotWindow, type TimeSpan, timeSpanToMinutes
} from "../../../services/api/ServicesApi.ts";
import {type OfficeDto, officesApi} from "../../../services/api/OfficesApi.ts";
import {profilesApi} from "../../../services/api/ProfilesApi.ts";

import Select from 'react-select';

import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import {appointmentsApi} from "../../../services/api/AppointmentApi.ts";

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
    const [bookingStatus, setBookingStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    
    const [specializations, setSpecializations] = useState<SpecializationDto[]>([]);
    const [services, setServices] = useState<ServiceDto[]>([]);
    const [offices, setOffices] = useState<OfficeDto[]>([]);
    const [doctors, setDoctors] = useState<DoctorProfileDto[]>([]);
    const [timeSlots, setTimeSlots] = useState<AvailablePositionsOnDay | null>(null);
    
    const [specialization, setSpecialization] = useState<SpecializationDto | null>(null);   
    const [service, setService] = useState<ServiceDto | null>(null);   
    const [office, setOffice] = useState<OfficeDto | null>(null);
    const [doctor, setDoctor] = useState<DoctorProfileDto | null>(null);
    
    const [date, setDate] = useState<Date | null>(null);
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
        });
    }, [service]);


    useEffect(() => {
        if (!doctor || !date) {
            setTimeSlots(null);
            setTimeSlot(null);
            return;
        }
        
        servicesApi.getAvailableDoctorSlots(doctor.accountId, date).then(result =>{
            if (result.type === "ok") setTimeSlots(result.value);
        });
    }, [doctor, date]);
    
    async function BookAnAppointment(){
        setBookingStatus('loading');
        setErrorMessage(null);

        try {
            const result = await appointmentsApi.bookAppointment({
                doctorAccountId: doctor!.accountId,
                officeId: office!.id,
                date: dateToDateOnly(date!),
                startSlotIndex: timeSlot!,
                serviceId: Number(service!.id),
                specializationId: Number(specialization!.id)
            });

            console.log({
                doctorAccountId: doctor!.accountId,
                officeId: office!.id,
                date: dateToDateOnly(date!),
                startSlotIndex: timeSlot!,
                serviceId: service!.id,
                specializationId: specialization!.id
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

          <DatePicker selected={date} onChange={setDate} />

          {service && timeSlots && (<TimeSlotPicker 
              selected={timeSlot ?? -1} 
              positions={timeSlots} 
              slotAmount={service.slotLength}
              onChange={x=>setTimeSlot(x)}/> )}
          
          <button disabled={!(service && timeSlot && doctor && date && office && specialization && bookingStatus != 'loading')}
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

interface TimeSlotPickerProps{
    positions: AvailablePositionsOnDay;
    selected: number;
    slotAmount: number;
    onChange: (value: number) => void;
}
export function TimeSlotPicker({positions, selected, slotAmount, onChange}: TimeSlotPickerProps){
    const [hoverIndex, setHoverIndex] = useState<number | null>(null);
    
    const slotsInHour = getSlotsInHour(positions.timeSlotLength);
    const slotMinute = timeSpanToMinutes(positions.timeSlotLength);
    const hours = Math.ceil(positions.slotAmount / slotsInHour);
    
    const minuteLabels = Array.from({ length: slotsInHour }, (_, i) => 
        `${i*slotMinute}-${i*slotMinute+slotMinute}`);
    const dayBeginMinutes = timeSpanToMinutes(positions.dayBeginTime);

    const hourLabels = Array.from({ length: hours }, (_, i) =>
        `${minutesToTimeSpan(dayBeginMinutes + i*60)}`);

    function isSlotAvailable(slotStartMinutes: number): boolean {
        return positions.availableTimeWindows.some(w => {
            const windowStart = dayBeginMinutes + w.timeSlotStart * slotMinute;
            const windowEnd = windowStart + w.timeSlotSize * slotMinute;
            return slotStartMinutes >= windowStart && slotStartMinutes < windowEnd;
        });
    }
    
    const slotsByHour: Record<number, { index: number; startMinutes: number; isAvailable: boolean }[]> = {};
    hourLabels.forEach((_, idx) => { slotsByHour[idx] = []; });

    for (let i = 0; i < positions.slotAmount; i++) {
        const startMinutes = dayBeginMinutes + i * slotMinute;
        const hour = Math.floor(i / slotsInHour);
        if (slotsByHour[hour]) {
            slotsByHour[hour].push({
                index: i,
                startMinutes,
                isAvailable: isSlotAvailable(startMinutes),
            });
        }
    }

    const allSlots = useMemo(() => {
        const slots: { index: number; startMinutes: number; isAvailable: boolean }[] = [];
        for (let i = 0; i < positions.slotAmount; i++) {
            const startMinutes = dayBeginMinutes + i * slotMinute;
            slots.push({
                index: i,
                startMinutes,
                isAvailable: isSlotAvailable(startMinutes),
            });
        }
        return slots;
    }, [positions.slotAmount, positions.availableTimeWindows]);
    
    const isInPreviewRange = (index: number): boolean => {
        if (hoverIndex === null) return false;
        if (index < hoverIndex || index >= hoverIndex + slotAmount) return false;
        for (let i = hoverIndex; i < hoverIndex + slotAmount; i++) {
            if (!allSlots[i]?.isAvailable) return false;
        }
        return true;
    };

    const isInSelectedRange = (index: number): boolean => {
        return index >= selected && index < selected + slotAmount;
    };
    
    return(
        <table className="slots-table" style={{ borderCollapse: 'collapse' }}>
            <thead>
            <tr>
                <th style={{ padding: '4px 8px', border: '1px solid #555' }}>Час</th>
                {minuteLabels.map((min, idx) => (
                    <th key={idx} style={{ padding: '4px 8px', border: '1px solid #555', fontSize: '12px' }}>
                        {String(min).padStart(5, '0')}
                    </th>
                ))}
            </tr>
            </thead>
            <tbody>
            {hourLabels.map((hour, idx) => {
                const slots = slotsByHour[idx] || [];

                const paddedSlots = [...slots];
                while (paddedSlots.length < slotsInHour) {
                    paddedSlots.push({ index: -1, startMinutes: 0, isAvailable: false });
                }
                return (
                    <tr key={hour}>
                        <td style={{ padding: '4px 8px', border: '1px solid #555', fontWeight: 'bold' }}>
                            {String(hour).padStart(2, '0')}:00
                        </td>
                        {paddedSlots.map((slot, idx) => (
                            <td
                                key={`${hour}-${idx}`}
                                className={`slot-item ${slot.isAvailable ? 'available' : 'unavailable'} 
                                ${isInSelectedRange(slot.index)? 'selected' : ''} 
                                ${isInPreviewRange(slot.index) ? 'preview' : ''}`}
                                onMouseEnter={() => setHoverIndex(slot.index)}
                                onMouseLeave={() => setHoverIndex(null)}
                                
                                onClick={() => {
                                    if (!slot.isAvailable) return;
                                  
                                    if (hoverIndex !== null && slot.index >= hoverIndex && slot.index < hoverIndex + slotAmount) {
                                        let allAvailable = true;
                                        for (let i = hoverIndex; i < hoverIndex + slotAmount; i++) {
                                            if (!allSlots[i]?.isAvailable) { allAvailable = false; break; }
                                        }
                                        if (allAvailable) {
                                           
                                            onChange(hoverIndex);
                                            setHoverIndex(null);
                                            return;
                                        }
                                    }
                                }}
                                title={slot.isAvailable ? `Слот ${slot.index+1}` : 'Занято'}
                            />
                        ))}
                    </tr>
                );
            })}
            </tbody>
        </table>
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