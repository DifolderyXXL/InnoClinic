import { useSearchParams } from "react-router";
import { useCallback } from "react";
import { OfficeFullCard } from "./OfficeCompactCard.tsx";
import { type OfficeDto, officesApi } from "../../../../services/api/OfficesApi.ts";
import { PaginatedListView, type PaginatedResult } from "../../common/PaginatedListView.tsx";
import "./OfficeComponents.css";

export function OfficePage() {
    const [searchParams] = useSearchParams();
    const targetId = searchParams.get("id") || null;

    if (targetId == null) {
        return <div className="status-message error">Not found</div>;
    }

    return (
        <div className="office-details-page">
            <OfficeFullCard officeId={targetId} />
        </div>
    );
}

const pageSize: number = 50;

export function OfficesPage() {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = Number(searchParams.get("page")) || 1;

    const updateUrlParams = (page: number) => {
        const nextParams = new URLSearchParams(searchParams);
        nextParams.set("page", String(page));
        setSearchParams(nextParams, { replace: true });
    };

    const fetchOffices = useCallback(
        async (page: number): Promise<PaginatedResult<OfficeDto>> => {
            try {
                const result = await officesApi.getOffices(page, pageSize);
                if (result.type === "ok") {
                    const items = result.value.items || result.value.offices || result.value;
                    return {
                        items: Array.isArray(items) ? items : [],
                        total: result.value.total ?? (Array.isArray(items) ? items.length : 0)
                    };
                }
                return {
                    items: [],
                    total: 0,
                    error: result.error?.title || "Error loading offices"
                };
            } catch {
                return {
                    items: [],
                    total: 0,
                    error: "Unhandled error"
                };
            }
        },
        []
    );

    return (
        <div className="offices-page">
            <PaginatedListView<OfficeDto>
                currentPage={currentPage}
                pageSize={pageSize}
                onPageChange={(page) => updateUrlParams(page)}
                fetchRequest={fetchOffices}
                dependencies={[]}
                renderItems={(items) => (
                    <div className="offices-grid">
                        {items.map((office) => (
                            <OfficeFullCard
                                key={office.id}
                                office={office}
                                isClickable={true}
                            />
                        ))}
                    </div>
                )}
            />
        </div>
    );
}