import { useCallback } from "react";
import { useSearchParams } from "react-router";
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

const PAGE_SIZE = 50;

export function OfficesPage() {
    const fetchOffices = useCallback(
        async (page: number): Promise<PaginatedResult<OfficeDto>> => {
            try {
                const result = await officesApi.getOffices(page, PAGE_SIZE);

                if (result.type === "ok") {
                    const data = result.value;
                    const items = data?.items || data?.offices || (Array.isArray(data) ? data : []);
                    const total = data?.total ?? items.length;

                    return {
                        items: Array.isArray(items) ? items : [],
                        total: typeof total === "number" ? total : 0,
                    };
                }

                return {
                    items: [],
                    total: 0,
                    error: result.error?.title || result.error?.message || "Failed to load offices",
                };
            } catch (err: any) {
                return {
                    items: [],
                    total: 0,
                    error: err?.message || "An unexpected error occurred while loading offices",
                };
            }
        },
        []
    );

    return (
        <div className="offices-page">
            <PaginatedListView<OfficeDto>
                pageSize={PAGE_SIZE}
                fetchRequest={fetchOffices}
                renderItems={(items) => (
                    <div className="offices-grid">
                        {items.map((office) => (
                            <OfficeFullCard
                                key={office.id}
                                office={office}
                                isClickable={true}
                                isEditable={false}
                            />
                        ))}
                    </div>
                )}
            />
        </div>
    );
}