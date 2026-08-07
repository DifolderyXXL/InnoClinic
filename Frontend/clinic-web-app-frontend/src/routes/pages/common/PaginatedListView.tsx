import {useEffect, useState, useCallback, type ReactNode} from "react";
import { useSearchParams } from "react-router";
import { PageSelector } from "../Shared/PageSelector.tsx";

export interface PaginatedResult<T> {
    items: T[];
    total: number;
    error?: string;
}

interface PaginatedListViewProps<T> {
    pageSize: number;
    fetchRequest: (page: number) => Promise<PaginatedResult<T>>;
    renderItems: (items: T[]) => ReactNode;
    dependencies?: unknown[];
    pageParamName?: string;
    syncWithUrl?: boolean;
}

export function PaginatedListView<T>({
                                         pageSize,
                                         fetchRequest,
                                         renderItems,
                                         dependencies = [],
                                         pageParamName = "page",
                                         syncWithUrl = true,
                                     }: PaginatedListViewProps<T>) {
    const [searchParams, setSearchParams] = useSearchParams();

    const urlPage = Number(searchParams.get(pageParamName)) || 1;
    const [localPage, setLocalPage] = useState(1);

    const currentPage = syncWithUrl ? urlPage : localPage;

    const [items, setItems] = useState<T[]>([]);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const handlePageChange = useCallback(
        (newPage: number) => {
            if (syncWithUrl) {
                const nextParams = new URLSearchParams(searchParams);
                nextParams.set(pageParamName, String(newPage));
                setSearchParams(nextParams, { replace: true });
            } else {
                setLocalPage(newPage);
            }
        },
        [syncWithUrl, searchParams, setSearchParams, pageParamName]
    );

    useEffect(() => {
        let isMounted = true;
        setLoading(true);
        setError(null);

        fetchRequest(currentPage).then((res) => {
            if (!isMounted) return;

            if (res.error) {
                setError(res.error);
                setItems([]);
                setTotal(0);
            } else {
                setItems(res.items);
                setTotal(res.total);
            }
            setLoading(false);
        });

        return () => {
            isMounted = false;
        };
    }, [currentPage, fetchRequest, ...dependencies]);

    if (loading) {
        return <div className="status-message">Loading...</div>;
    }

    if (error) {
        return <div className="status-message error">{error}</div>;
    }

    return (
        <div className="paginated-list-container">
            <div className="paginated-list-content">
                {renderItems(items)}
            </div>

            {total > pageSize && (
                <PageSelector
                    currentPage={currentPage}
                    pageSize={pageSize}
                    total={total}
                    onPageChange={handlePageChange}
                />
            )}
        </div>
    );
}