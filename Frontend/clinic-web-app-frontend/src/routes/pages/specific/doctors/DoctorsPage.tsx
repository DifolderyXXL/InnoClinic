import "./DoctorsPage.css";
import { useState, useCallback, useEffect } from "react";
import { profilesApi } from "../../../../services/api/ProfilesApi.ts";
import { AvatarFromSource } from "../../Shared/Avatar.tsx";
import { OfficeInputFilter, SpecializationInputFilter } from "../../Shared/Inputs/OfficeInputFilter.tsx";
import { useSearchParams } from "react-router";
import { Link } from "react-router-dom";
import { OfficeAddress } from "../offices/OfficeCompactCard.tsx";
import { PaginatedListView, type PaginatedResult } from "../../common/PaginatedListView.tsx";

const pageSize: number = 50;

export function useUpdateUrlParams() {
    const [searchParams, setSearchParams] = useSearchParams();

    const updateUrlParams = (
        newParams: Record<string, string | number | null | undefined>,
        withPages: boolean = false
    ) => {
        const nextParams = new URLSearchParams(searchParams);

        Object.entries(newParams).forEach(([key, val]) => {
            if (val !== null && val !== undefined && val !== "") {
                nextParams.set(key, String(val));
            } else {
                nextParams.delete(key);
            }
        });

        if (withPages && !("page" in newParams)) {
            nextParams.set("page", "1");
        }

        setSearchParams(nextParams, { replace: true });
    };

    return { searchParams, updateUrlParams };
}


export function DoctorsPage() {
    const [searchParams, setSearchParams] = useSearchParams();

    const urlFullName = searchParams.get("fullName") || "";
    const urlOfficeId = searchParams.get("officeId") || "";
    const urlSpecId = Number(searchParams.get("specId")) || null;

    const [fullName, setFullName] = useState(urlFullName);
    const [office, setOffice] = useState<string | null>(urlOfficeId);
    const [specialization, setSpecialization] = useState<number | null>(urlSpecId);

    useEffect(() => {
        setFullName(urlFullName);
        setOffice(urlOfficeId || null);
        setSpecialization(urlSpecId);
    }, [urlFullName, urlOfficeId, urlSpecId]);

    const updateFilter = (newParams: Record<string, string | number | null>) => {
        const nextParams = new URLSearchParams(searchParams);

        Object.entries(newParams).forEach(([key, val]) => {
            if (val !== null && val !== undefined && val !== "") {
                nextParams.set(key, String(val));
            } else {
                nextParams.delete(key);
            }
        });

        nextParams.set("page", "1");
        setSearchParams(nextParams, { replace: true });
    };

    const fetchDoctors = useCallback(
        async (page: number): Promise<PaginatedResult<DoctorProfile>> => {
            try {
                const result = await profilesApi.getDoctors({
                    page,
                    pageSize,
                    officeIds: urlOfficeId ? [urlOfficeId] : undefined,
                    specializationIds: urlSpecId ? [Number(urlSpecId)] : undefined,
                    fullName: urlFullName || undefined
                });

                if (result.type === "ok") {
                    return {
                        items: result.value.items ?? [],
                        total: result.value.total ?? 0
                    };
                }

                return {
                    items: [],
                    total: 0,
                    error: result.error?.title || "Error loading doctors"
                };
            } catch {
                return {
                    items: [],
                    total: 0,
                    error: "Unhandled error occurred"
                };
            }
        },
        [urlOfficeId, urlSpecId, urlFullName]
    );

    return (
        <div className="doctors-page">
            <div className="filter-container">
                <div className="filter-block">
                    <form onSubmit={(e) => {
                        e.preventDefault();
                        updateFilter({ fullName });
                    }}>
                        <div className="filter-field">
                            <label>Full name</label>
                            <div className="input-with-button">
                                <input
                                    type="text"
                                    placeholder="Search by name..."
                                    value={fullName}
                                    onChange={e => setFullName(e.target.value)}
                                />
                                <button
                                    type="submit"
                                    disabled={urlFullName === fullName}
                                >
                                    Apply
                                </button>
                            </div>
                        </div>
                    </form>
                </div>

                <div className="filter-divider" />

                <div className="filter-block">
                    <div className="filter-field">
                        <OfficeInputFilter
                            label="Office"
                            valueId={office}
                            onChange={x => {
                                setOffice(x?.id ?? null);
                                updateFilter({ officeId: x?.id ?? null });
                            }}
                        />
                    </div>
                </div>

                <div className="filter-block">
                    <div className="filter-field">
                        <SpecializationInputFilter
                            label="Specialization"
                            valueId={specialization}
                            onChange={x => {
                                setSpecialization(x?.id ? Number(x.id) : null);
                                updateFilter({ specId: x?.id ? String(x.id) : null });
                            }}
                        />
                    </div>
                </div>
            </div>

            <PaginatedListView<DoctorProfile>
                pageSize={pageSize}
                fetchRequest={fetchDoctors}
                dependencies={[urlOfficeId, urlSpecId, urlFullName]}
                renderItems={(items) => {
                    if (items.length === 0) {
                        return (
                            <div className="status-message">
                                No doctors found matching your criteria.
                            </div>
                        );
                    }

                    return (
                        <div className="doctors-grid">
                            {items.map((doctor) => (
                                <Link
                                    key={doctor.accountId}
                                    to={`/doctors/details?id=${doctor.accountId}`}
                                    className="doctor-card-link"
                                >
                                    <DoctorViewCard doctor={doctor} />
                                </Link>
                            ))}
                        </div>
                    );
                }}
            />
        </div>
    );
}

export interface DoctorProfile {
    accountId: string;
    accountFirstName: string;
    accountLastName: string;
    accountMiddleName?: string | null;
    accountPhotoId?: string | null;
    photoUrl?: string | null;
    dateOfBirth: string;
    specializationId: number;
    specializationName: string;
    officeId: string;
    careerStartYear: number;
}

interface DoctorViewCardProps {
    doctor: DoctorProfile;
}

export function DoctorViewCard({ doctor }: DoctorViewCardProps) {
    const currentYear = new Date().getFullYear();
    const experience =
        doctor.careerStartYear > 0 && doctor.careerStartYear <= currentYear
            ? currentYear - doctor.careerStartYear
            : 0;

    const fullName = [doctor.accountLastName, doctor.accountFirstName, doctor.accountMiddleName]
        .filter(Boolean)
        .join(' ');

    return (
        <div className="doctor-card">
            <AvatarFromSource PhotoUrl={doctor.photoUrl} TextIfPhotoNull={fullName[0] ?? "?"} />

            <div className="doctor-info">
                <div className="doctor-name">
                    <strong>{fullName}</strong>
                    <span>{doctor.specializationName}</span>
                </div>

                <div className="doctor-details">
                    <OfficeAddress officeId={doctor.officeId} />
                    <span>Exp: {experience > 0 ? `${experience} years` : 'Newbie'}</span>
                </div>
            </div>
        </div>
    );
}