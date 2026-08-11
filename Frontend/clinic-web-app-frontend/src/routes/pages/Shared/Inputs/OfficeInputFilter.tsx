import { type OfficeDto, officesApi } from "../../../../services/api/OfficesApi.ts";
import {useEffect, useMemo, useState} from "react";
import { SearchableSelect } from "../../actionable/MakeAppointmentForm.tsx";
import { servicesApi, type SpecializationDto } from "../../../../services/api/ServicesApi.ts";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import Select from "react-select";

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

interface PatientInputFilterProps {
    label?: string;
    valueId?: string | null;
    onChange: (patient: PatientProfileDto | null) => void;
    placeholder?: string;
    disabled?: boolean;
    className?: string;
}

function getPatientLabel(p: PatientProfileDto): string {
    const firstName = p.accountFirstName || p.firstName || "";
    const lastName = p.accountLastName || p.lastName || "";
    const middleName = p.accountMiddleName || p.middleName || "";
    const email = p.accountEmail || p.email || "";

    const fullName = `${firstName} ${lastName} ${middleName}`.trim();
    return email ? `${fullName} (${email})` : fullName || "Unknown Patient";
}

function getPatientKey(p: PatientProfileDto): string {
    return p.accountId || p.id || "";
}

export function PatientInputFilter({
                                       label = "Patient",
                                       valueId,
                                       onChange,
                                       placeholder = "Type name or email to search...",
                                       disabled = false,
                                       className,
                                   }: PatientInputFilterProps) {
    const [patients, setPatients] = useState<PatientProfileDto[]>([]);
    const [selectedPatient, setSelectedPatient] = useState<PatientProfileDto | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    const [inputValue, setInputValue] = useState("");
    const [searchQuery, setSearchQuery] = useState("");

    useEffect(() => {
        const timer = setTimeout(() => {
            setSearchQuery(inputValue);
        }, 300);
        return () => clearTimeout(timer);
    }, [inputValue]);

    useEffect(() => {
        setIsLoading(true);
        profilesApi.getPatients( 1, 20, searchQuery )
            .then((res) => {
                if (res?.type === "ok") {
                    const items = res.value.items || res.value || [];
                    setPatients(Array.isArray(items) ? items : []);
                }
            })
            .finally(() => setIsLoading(false));
    }, [searchQuery]);

    useEffect(() => {
        if (!valueId) {
            setSelectedPatient(null);
            return;
        }

        const foundInList = patients.find(
            p => (p.accountId && p.accountId === valueId) || (p.id && p.id === valueId)
        );

        if (foundInList) {
            setSelectedPatient(foundInList);
        } else {
            profilesApi.getPatient(valueId).then((res) => {
                if (res?.type === "ok") {
                    setSelectedPatient(res.value);
                }
            });
        }
    }, [valueId, patients]);

    const selectOptions = useMemo(() => {
        return patients.map((p) => ({
            value: p,
            label: getPatientLabel(p),
            key: getPatientKey(p),
        }));
    }, [patients]);

    const currentValue = selectedPatient
        ? {
            value: selectedPatient,
            label: getPatientLabel(selectedPatient),
            key: getPatientKey(selectedPatient),
        }
        : null;

    return (
        <div className={className || "searchable-select-container"}>
            {label && <label className="form-label">{label}</label>}
            <Select
                classNamePrefix="custom-select"
                options={selectOptions}
                value={currentValue}
                inputValue={inputValue}
                onInputChange={(val, action) => {
                    if (action.action === "input-change") {
                        setInputValue(val);
                    } else if (action.action === "input-blur" || action.action === "menu-close") {
                        setInputValue("");
                    }
                }}
                onChange={(selected) => {
                    const val = selected?.value ?? null;
                    setSelectedPatient(val);
                    onChange(val);
                }}
                isLoading={isLoading}
                placeholder={placeholder}
                isClearable
                isDisabled={disabled}
                filterOption={() => true}
                getOptionLabel={(option) => option.label}
                getOptionValue={(option) => option.key}
                noOptionsMessage={() => isLoading ? "Searching..." : "No patients found"}
            />
        </div>
    );
}