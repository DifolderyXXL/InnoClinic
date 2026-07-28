import { useSearchParams } from "react-router";
import { OfficeFullCard } from "./OfficeCompactCard.tsx";
import {PageSelector} from "../../Shared/PageSelector.tsx";
import {useEffect, useState} from "react";
import {type OfficeDto, officesApi} from "../../../../services/api/OfficesApi.ts";

export function OfficePage() {
    const [searchParams] = useSearchParams();
    const targetId = searchParams.get("id") || null;

    if (targetId == null) {
        return <div>Not found</div>;
    }

    return (
        <div style={{ padding: "20px", maxWidth: "600px", margin: "0 auto" }}>
            <OfficeFullCard officeId={targetId} />
        </div>
    );
}

const pageSize: number = 50;

export function OfficesPage() {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = Number(searchParams.get("page")) || 1;

    const [offices, setOffices] = useState<OfficeDto[]>([]);
    const [total, setTotal] = useState<number>(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const updateUrlParams = (page: number) => {
        const nextParams = new URLSearchParams(searchParams);
        nextParams.set("page", String(page));
        setSearchParams(nextParams);
    };

    useEffect(() => {
        const loadData = async () => {
            setLoading(true);
            setError(null);

            try {
                const result = await officesApi.getOffices(currentPage, pageSize);
                if (result.type === "ok") {
                    const items = result.value.items || result.value.offices || result.value;
                    setOffices(Array.isArray(items) ? items : []);
                    setTotal(result.value.total ?? (Array.isArray(items) ? items.length : 0));
                } else {
                    setError(result.error?.title || "Error loading offices");
                }
            } catch (err) {
                setError("Unhandled error");
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [currentPage]);

    if (loading) {
        return <div style={{ textAlign: 'center', padding: '40px' }}>Loading offices...</div>;
    }

    if (error) {
        return <div style={{ textAlign: 'center', padding: '40px', color: 'red' }}>{error}</div>;
    }

    return (
        <div style={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden' }}>
            <div style={{
                flex: 1,
                overflowY: 'auto',
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
                gap: '16px',
                padding: '20px'
            }}>
                {offices.map((office) => (
                    <OfficeFullCard
                        key={office.id}
                        office={office}
                        isClickable={true}
                    />
                ))}
            </div>

            <PageSelector
                pageSize={pageSize}
                total={total}
                onPageChange={(page) => updateUrlParams(page)}
            />
        </div>
    );
}