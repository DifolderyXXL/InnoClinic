import { useEffect, useState } from "react";
import { officesApi, type OfficeDto } from "../../../../services/api/OfficesApi.ts";
import { AvatarFromSource } from "../../Shared/Avatar.tsx";
import { Link } from "react-router";
import "./OfficeComponents.css";
import {documentsService} from "../../../../services/DocumentsService.ts";

export function formatOfficeAddress(office: OfficeDto): string {
    const parts = [
        office.city,
        office.street,
        office.houseNumber,
        office.officeNumber ? `Office number ${office.officeNumber}` : null
    ].filter(Boolean);

    return parts.join(", ");
}

interface BaseOfficeProps {
    office?: OfficeDto | null;
    officeId?: string | null;
}

export function OfficeAddress({ office, officeId }: BaseOfficeProps) {
    const [data, setData] = useState<OfficeDto | null>(office || null);

    useEffect(() => {
        if (office) {
            setData(office);
            return;
        }
        if (officeId) {
            officesApi.getOffice(officeId).then((res) => {
                if (res.type === "ok") setData(res.value);
            });
        }
    }, [office, officeId]);

    if (!data) return <span>Loading address...</span>;

    return <span>{formatOfficeAddress(data)}</span>;
}

function useOfficeData(office?: OfficeDto | null, officeId?: string | null) {
    const [data, setData] = useState<OfficeDto | null>(office || null);
    const [loading, setLoading] = useState(!office && Boolean(officeId));

    useEffect(() => {
        if (office) {
            setData(office);
            setLoading(false);
            return;
        }

        if (officeId) {
            setLoading(true);
            officesApi.getOffice(officeId).then((res) => {
                if (res.type === "ok") setData(res.value);
                setLoading(false);
            });
        } else {
            setLoading(false);
        }
    }, [office, officeId]);

    return { data, loading };
}

export function OfficeCompactCard({ office, officeId }: BaseOfficeProps) {
    const { data, loading } = useOfficeData(office, officeId);

    if (loading) return <div className="office-loading">Loading office...</div>;
    if (!data) return <div className="office-error">Office not found</div>;
    
    return (
        <div className="office-compact-card">
            <AvatarFromSource
                PhotoUrl={data && data.photoId 
                    ? documentsService.getOfficePhotoUrl(data.id, data.photoId) 
                    : null}
                TextIfPhotoNull={data.city[0] ?? "O"}
            />
            <div className="office-compact-info">
                <strong className="office-compact-title">Office: {data.city}</strong>
                <span className="office-compact-address">
                    Address: {formatOfficeAddress(data)}
                </span>
                <span className="office-compact-phone">
                    Reception: {data.registryPhoneNumber}
                </span>
            </div>
        </div>
    );
}

interface OfficeFullCardProps extends BaseOfficeProps {
    onClick?: () => void;
    isClickable?: boolean;
}

export function OfficeFullCard({ office, officeId, onClick, isClickable = false }: OfficeFullCardProps) {
    const { data, loading } = useOfficeData(office, officeId);

    if (loading) return <div className="office-loading">Loading office...</div>;
    if (!data) return <div className="office-error">Office not found</div>;

    const cardContent = (
        <div
            onClick={onClick}
            className={`office-full-card ${isClickable ? "clickable" : ""}`}
        >
            <div className="office-full-card-header">
                <AvatarFromSource
                    PhotoUrl={data.photoId}
                    TextIfPhotoNull={data.city[0] ?? "O"}
                />
                <div className="office-full-card-body">
                    <div className="office-full-card-title-row">
                        <h3 className="office-full-card-title">{data.city} Office</h3>
                        <span className={`office-status ${data.isActive ? "active" : "inactive"}`}>
                            {data.isActive ? "Active" : "Inactive"}
                        </span>
                    </div>
                    <p className="office-full-card-address">
                        Address: {formatOfficeAddress(data)}
                    </p>
                </div>
            </div>

            <div className="office-full-card-footer">
                <span>Phone: <strong>{data.registryPhoneNumber}</strong></span>
            </div>
        </div>
    );

    if (isClickable && data.id) {
        return (
            <Link
                to={`/offices/details?id=${data.id}`}
                className="office-link"
            >
                {cardContent}
            </Link>
        );
    }

    return cardContent;
}