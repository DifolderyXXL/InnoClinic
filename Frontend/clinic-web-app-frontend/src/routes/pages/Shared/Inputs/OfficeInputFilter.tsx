import { type OfficeDto, officesApi } from "../../../../services/api/OfficesApi.ts";
import { useEffect, useState } from "react";
import { SearchableSelect } from "../../actionable/MakeAppointmentForm.tsx";
import { servicesApi, type SpecializationDto } from "../../../../services/api/ServicesApi.ts";

interface OfficeInputFilterProps {
    label?: string;
    valueId: string | null;
    onChange: (office: OfficeDto | null) => void;
}

function officeToString(office: OfficeDto) {
    return `${office.city}, ${office.street} ${office.houseNumber}`;
}

export function OfficeInputFilter({ label, valueId, onChange }: OfficeInputFilterProps) {
    const [offices, setOffices] = useState<OfficeDto[]>([]);

    useEffect(() => {
        officesApi.getOffices().then(result => {
            if (result.type === "ok") {
                const items = result.value.items || result.value.offices || result.value;
                setOffices(Array.isArray(items) ? items : []);
            }
        });
    }, []);

    const selectedOffice = (offices ?? []).find(o => String(o.id) === String(valueId)) || null;

    return (
        <SearchableSelect
            options={offices}
            label={label}
            getLabel={officeToString}
            getKey={o => o.id}
            onChange={onChange}
            value={selectedOffice}
            placeholder="Select office..."
        />
    );
}

interface SpecializationInputFilterProps {
    label?: string;
    valueId: number | null;
    onChange: (spec: SpecializationDto | null) => void;
}

function specializationToString(s: SpecializationDto) {
    return `${s.specializationName}`;
}

export function SpecializationInputFilter({ label, valueId, onChange }: SpecializationInputFilterProps) {
    const [specializations, setSpecializations] = useState<SpecializationDto[]>([]);

    useEffect(() => {
        servicesApi.getSpecializations().then(result => {
            if (result.type === "ok") {
                const items = result.value.specializations || result.value.items || result.value;
                setSpecializations(Array.isArray(items) ? items : []);
            }
        });
    }, []);

    const selectedSpec = (specializations ?? []).find(s => Number(s.id) === Number(valueId)) || null;

    return (
        <SearchableSelect
            options={specializations}
            getLabel={specializationToString}
            label={label}
            getKey={o => o.id}
            onChange={onChange}
            value={selectedSpec}
            placeholder="Select specialization..."
        />
    );
}