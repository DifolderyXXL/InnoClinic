import {type OfficeDto, officesApi} from "../../../../services/api/OfficesApi.ts";
import {useEffect, useState} from "react";
import {SearchableSelect} from "../../actionable/MakeAppointmentForm.tsx";
import {servicesApi, type SpecializationDto} from "../../../../services/api/ServicesApi.ts";

interface OfficeInputFilterProps{
    label?: string;
    value: OfficeDto | null;
    onChange: (office: OfficeDto | null) => void
}
function officeToString(office: OfficeDto){
    return `${office.city} ${office.street} ${office.houseNumber}`;
}
export function OfficeInputFilter({label, value, onChange}: OfficeInputFilterProps){
    const [offices, setOffices] = useState<OfficeDto[]>([]);

    useEffect(() => {
        officesApi.getOffices().then(result => {
            if (result.type === "ok") setOffices(result.value.offices);
        });
    }, []);

    return (
        <SearchableSelect options={offices}
                          label={label}
                          getLabel={officeToString}
                          getKey={o=>o.id}
                          onChange={onChange}
                          value={value}></SearchableSelect>
    );
}


interface SpecializationInputFilterProps{
    label?: string;
    value: SpecializationDto | null;
    onChange: (spec: SpecializationDto | null) => void
}
function specializationToString(s: SpecializationDto){
    return `${s.specializationName}`;
}
export function SpecializationInputFilter({label, value, onChange}: SpecializationInputFilterProps){
    const [specializations, setSpecializations] = useState<SpecializationDto[]>([]);

    useEffect(() => {
        servicesApi.getSpecializations().then(result => {
            if (result.type === "ok") setSpecializations(result.value.specializations);
        });
    }, []);

    return (
        <SearchableSelect options={specializations}
                          getLabel={specializationToString}
                          label={label}
                          getKey={o=>o.id}
                          onChange={onChange}
                          value={value}></SearchableSelect>
    );
}
